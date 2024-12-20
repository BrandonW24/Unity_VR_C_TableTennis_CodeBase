using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.SDK;
using UnityEngine.UI;

public class Paddle4 : MonoBehaviour
{
    public GameObject ball;
    public GameObject ballSpawnPoint;
    public Rigidbody ballRigidbody;
    public MLGrab grabComponent;
    public float forceMultiplier = 10f;
    public float maxForceMultiplier = 8000f;
    public float sphereCastRadius = 0.1f;
    public float maxDetectionDistance = 0.5f;
    public GameObject visualGameObject;
    public Slider PowerBarMeter;
    public GameObject visualBar;
    public GameObject GrabLocation;
    public GameObject VFXHit;
    public Transform golfClubVisual; // Reference to the golf club visual

    private bool isHeld;
    private bool clientIsOwner = false;
    private Vector3 previousPosition;
    private Vector3 paddleVelocity;
    private bool isReleased = false;
    private bool isCharging = false;
    private float currentCharge = 0f;
    public float chargeRate = 100f;

    private Color greenColor = Color.green;
    private Color yellowColor = Color.yellow;
    private Color redColor = Color.red;

    const string EVENT_ID = "HitEvent";
    EventToken token;
    const string EVENT_Grab_Key = "GrabKey";
    EventToken grabToken;
    const string EVENT_Charge_Power = "ChargePower";
    EventToken chargeToken;
    const string EVENT_Release_Power = "ReleasePower";
    EventToken releaseToken;

    const string EVENT_ID_Hit_2 = "Hit_PushEvent";
    EventToken pushEventToken;

    private MLPlayer player;
    private bool isTriggerPressed = false;
    private const float DM_KEY_TRESHOLD = 0.1f;
    private bool isGripPressed = false;

    private float hitCooldown = 0.05f; // Delay in seconds between hits
    private float lastHitTime = -Mathf.Infinity; // Tracks the last hit time

    private float lastEffectSpawnTime = 0f; // Tracks the last time the effect was spawned
    private float effectCooldown = 0.5f; // Cooldown duration for the effect in seconds

    [SerializeField] private LineRenderer trajectoryLine; // Assign this in the inspector
    [SerializeField] private int lineSegmentCount = 20; // Number of points for the line to show trajectory

    // Size limits for the golf club
    [SerializeField] private float minSize = 0.5f; // Minimum scale factor
    [SerializeField] private float maxSize = 2.0f; // Maximum scale factor
    private float initialDistance; // Distance at the time of secondary grab
    private Vector3 initialScale;  // Scale at the time of secondary grab
    private bool SizeChange_Flag = false;

    private bool isBallInContact = false; // Track if the ball is in contact with the paddle
    public float impactThreshold = 2.0f;
    private Vector3 lastPaddleVelocity = Vector3.zero; // Track the paddle's last velocity

    [SerializeField] public GameObject golfClub;
    private void OnHitEvent(object[] args)
    {
        if (isHeld && ball != null)
        {
            Vector3 direction = ball.transform.position - transform.position;
            RaycastHit hit;

            if (Physics.SphereCast(transform.position, sphereCastRadius, direction, out hit, maxDetectionDistance))
            {
                if (hit.collider.gameObject == ball)
                {
                    Debug.Log("SphereCast detected the ball. Applying force.");
                    Vector3 hitForce = paddleVelocity * forceMultiplier;
                    ballRigidbody.AddForce(hitForce, ForceMode.Impulse);
                }
            }
        }
    }

    private void OnPushEvent(object[] args)
    {
        if (isHeld && ball != null)
        {
            Vector3 direction = ball.transform.position - transform.position;
            RaycastHit hit;

            if (Physics.SphereCast(transform.position, sphereCastRadius, direction, out hit, maxDetectionDistance))
            {
                if (hit.collider.gameObject == ball)
                {
                    Debug.Log("SphereCast detected the ball. Applying force.");
                    Vector3 pushForce = paddleVelocity.normalized * Mathf.Clamp(paddleVelocity.magnitude * forceMultiplier, 0.1f, 1f);
                    ballRigidbody.AddForce(pushForce * 25, ForceMode.Force);
                }
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        Debug.Log("Low-velocity nudge from oncollisionstay! Applying continuous push force.");
        Vector3 pushForce = paddleVelocity.normalized * Mathf.Clamp(paddleVelocity.magnitude * forceMultiplier, 0.1f, 1f);
        ballRigidbody.AddForce(pushForce * 25, ForceMode.Force);
        this.InvokeNetwork(EVENT_ID_Hit_2, EventTarget.Master, null);

    }

    private void OnGrabKeyPressEvent(object[] args)
    {
        SpawnBall();
    }

    private void OnTriggerDownPress(object[] args)
    {
        isCharging = true;
    }

    private void OnTriggerRelease(object[] args)
    {
        isCharging = false;
        ReleaseSwing();
    }



    private void OnVRModeSecondaryGrab()
    {
        if (grabComponent.PrimaryHand && grabComponent.SecondaryHand)
        {
            // Enable size change
            SizeChange_Flag = true;

            // Store the initial distance and scale
            initialDistance = Vector3.Distance(
                grabComponent.PrimaryHand.transform.position,
                grabComponent.SecondaryHand.transform.position
            );
            initialScale = golfClub.transform.localScale;

            Debug.Log($"[Secondary Grab] Initial Distance: {initialDistance}, Initial Scale: {initialScale}");
        }
        else
        {
            Debug.LogWarning("[Secondary Grab] Either PrimaryHand or SecondaryHand is missing!");
        }
    }

    private void OnVRModeSecondaryGrab_End()
    {
        // Disable size change
        SizeChange_Flag = false;
        Debug.Log("[Secondary Grab End] Size change disabled.");
    }

    private void OnVRModeTriggerDown()
    {
        if (!MassiveLoopClient.IsInDesktopMode)
        {
            this.InvokeNetwork(EVENT_Grab_Key, EventTarget.Master, null);
        }
    }

    void Start()
    {
        Debug.Log("Paddle2 start");

        if (MassiveLoopRoom.GetLocalPlayer().IsMasterClient)
        {
            clientIsOwner = true;
        }

        if (grabComponent != null)
        {
            grabComponent.OnPrimaryGrabBegin.AddListener(OnPrimaryGrabBegin);
            grabComponent.OnPrimaryGrabEnd.AddListener(OnPrimaryGrabEnd);
            grabComponent.OnPrimaryTriggerUp.AddListener(OnPrimaryTriggerUp);
            grabComponent.OnPrimaryTriggerDown.AddListener(OnVRModeTriggerDown);
            grabComponent.OnSecondaryGrabBegin.AddListener(OnVRModeSecondaryGrab);
            grabComponent.OnSecondaryGrabEnd.AddListener(OnVRModeSecondaryGrab_End);

            token = this.AddEventHandler(EVENT_ID, OnHitEvent);
            grabToken = this.AddEventHandler(EVENT_Grab_Key, OnGrabKeyPressEvent);
            chargeToken = this.AddEventHandler(EVENT_Charge_Power, OnTriggerDownPress);
            releaseToken = this.AddEventHandler(EVENT_Release_Power, OnTriggerRelease);

            pushEventToken = this.AddEventHandler(EVENT_ID_Hit_2, OnPushEvent);

            PowerBarMeter.gameObject.SetActive(true);
            PowerBarMeter.maxValue = maxForceMultiplier;
        }

        isHeld = false;
    }

    void FixedUpdate()
    {
        if (isHeld)
        {
            paddleVelocity = (transform.position - previousPosition) / Time.fixedDeltaTime;
            previousPosition = transform.position;

            if (SizeChange_Flag && grabComponent.PrimaryHand && grabComponent.SecondaryHand)
            {
                // Calculate the current distance between hands
                float currentDistance = Vector3.Distance(
                    grabComponent.PrimaryHand.transform.position,
                    grabComponent.SecondaryHand.transform.position
                );

                // Determine the scaling factor based on the distance ratio
                float scaleFactor = Mathf.Clamp(currentDistance / initialDistance, minSize, maxSize);

                // Apply the new scale
                golfClub.transform.localScale = initialScale * scaleFactor;

                // Size check log
                //  Debug.Log($"[FixedUpdate] Current Distance: {currentDistance}, Scale Factor: {scaleFactor}, New Scale: {golfClub.transform.localScale}");
            }
            else if (!SizeChange_Flag)
            {
                //To determine if the flag is being set correctly. Which it is. But in case you wanted to check that, here it is.
                // Debug.Log("[FixedUpdate] SizeChange_Flag is false, skipping size adjustment.");
            }
            if (ball != null && Time.time >= lastHitTime + hitCooldown) // Check hit cooldown
            {
                Vector3 direction = ball.transform.position - transform.position;
                RaycastHit hit;

                if (Physics.SphereCast(transform.position, sphereCastRadius, direction, out hit, maxDetectionDistance))
                {
                    if (hit.collider.gameObject == ball)
                    {
                        Debug.Log("SphereCast detected the ball.");

                        // Apply force based on paddle velocity
                        if (paddleVelocity.magnitude > impactThreshold) // High-velocity impact
                        {
                            Debug.Log("High-velocity impact! Applying impulse force.");
                            Vector3 hitForce = paddleVelocity * forceMultiplier;
                            ballRigidbody.AddForce(hitForce, ForceMode.Impulse);
                            this.InvokeNetwork(EVENT_ID, EventTarget.Master, null);
                        }
                        else // Low-velocity nudge
                        {
                            Debug.Log("Low-velocity nudge! Applying continuous push force.");
                            Vector3 pushForce = paddleVelocity.normalized * Mathf.Clamp(paddleVelocity.magnitude * forceMultiplier, 0.1f, 1f);
                            ballRigidbody.AddForce(pushForce * 25, ForceMode.Force);
                            this.InvokeNetwork(EVENT_ID_Hit_2, EventTarget.Master, null);
                        }



                        // Spawn VFX with a cooldown
                        if (Time.time >= lastEffectSpawnTime + effectCooldown)
                        {
                            Object.Instantiate(VFXHit, ball.transform.position, Quaternion.identity);
                            lastEffectSpawnTime = Time.time; // Update the last effect spawn time
                        }

                        lastHitTime = Time.time; // Update the last hit time
                    }
                }
            }

        }


        //Desktop mode mechanic
        if (isCharging)
        {
            currentCharge += chargeRate * Time.fixedDeltaTime;
            currentCharge = Mathf.Clamp(currentCharge, forceMultiplier, maxForceMultiplier);
            PowerBarMeter.value = currentCharge;

            // Calculate the charge percentage as a value between 0 and 1
            float chargePercentage = (currentCharge - forceMultiplier) / (maxForceMultiplier - forceMultiplier);

            // Interpolate color based on charge level
            Color targetColor;

            if (chargePercentage < 0.33f)
            {
                targetColor = Color.Lerp(greenColor, yellowColor, chargePercentage / 0.33f);
            }
            else if (chargePercentage < 0.66f)
            {
                targetColor = Color.Lerp(yellowColor, redColor, (chargePercentage - 0.33f) / 0.33f);
            }
            else
            {
                targetColor = redColor;
            }

            PowerBarMeter.fillRect.GetComponent<Image>().color = targetColor;

            // Display the trajectory based on the charge
            DisplayTrajectory();
        }
        else
        {
            // Hide the trajectory line when not charging
            trajectoryLine.positionCount = 0;
        }
    }

    private void DisplayTrajectory()
    {
        if (ball == null) return;

        // Adjust the direction to give more influence to GrabLocation.transform.forward
        float forwardInfluence = 10.0f; // Adjust this value to increase/decrease forward influence
        Vector3 offset = new Vector3(0, 0, 0);
        Vector3 direction = (ball.transform.position - GrabLocation.transform.position).normalized + (GrabLocation.transform.forward + offset * forwardInfluence);
        direction.Normalize();

        Vector3 chargedForce = direction * currentCharge;

        // Simulate the trajectory path
        Vector3[] trajectoryPoints = new Vector3[lineSegmentCount];
        Vector3 startPosition = ball.transform.position;
        Vector3 velocity = chargedForce / ballRigidbody.mass; // Initial velocity based on charge

        for (int i = 0; i < lineSegmentCount; i++)
        {
            float time = i * Time.fixedDeltaTime;
            trajectoryPoints[i] = startPosition + velocity * time + 0.5f * Physics.gravity * time * time;
        }

        // Set the points to the LineRenderer
        trajectoryLine.positionCount = lineSegmentCount;
        trajectoryLine.SetPositions(trajectoryPoints);
    }



    private float triggerCooldown = 1.25f; // Cooldown time in seconds
    private float lastTriggerTime = -Mathf.Infinity; // Tracks the last trigger press time

    public void Update()
    {
        if (player != null)
        {
            if (MassiveLoopClient.IsInDesktopMode)
            {
                if (player.UserInput != null)
                {
                    // Handle TriggerPress1 with cooldown
                    if (player.UserInput.TriggerPress1 && !isReleased && Time.time >= lastTriggerTime + triggerCooldown)
                    {
                        isReleased = true;
                        lastTriggerTime = Time.time; // Update the last trigger time
                        this.InvokeNetwork(EVENT_Charge_Power, EventTarget.All, null);
                    }
                    else if (!player.UserInput.TriggerPress1 && isReleased)
                    {
                        this.InvokeNetwork(EVENT_Release_Power, EventTarget.All, null);
                        isReleased = false;
                    }

                    // Handle Grip1
                    if (player.UserInput.Grip1 > DM_KEY_TRESHOLD && !isGripPressed)
                    {
                        Debug.Log("Grab pressed");
                        isGripPressed = true;
                        this.InvokeNetwork(EVENT_Grab_Key, EventTarget.Master, null);
                    }
                    else if (player.UserInput.Grip1 <= DM_KEY_TRESHOLD && isGripPressed)
                    {
                        isGripPressed = false;
                    }
                }
            }
            else
            {
                // VR or other mode input handling
            }
        }
    }

    private void ReleaseSwing()
    {
        Debug.Log($"Swing released with charge: {currentCharge}");

        if (ball != null && MassiveLoopClient.IsMasterClient)
        {

            Object.Instantiate(VFXHit, ball.transform.position, Quaternion.identity);
            // Use the forward direction of the GrabLocation for the force direction
            Vector3 offset = new Vector3(0, 0.25f, 0);
            Vector3 direction = GrabLocation.transform.forward + offset;
            Vector3 chargedForce = direction.normalized * currentCharge;
            ballRigidbody.AddForce(chargedForce, ForceMode.Impulse);
        }

        currentCharge = 0;
        PowerBarMeter.value = currentCharge;

        // Simulate the golf club swing
        StartCoroutine(SimulateSwing());
    }

    private IEnumerator SimulateSwing()
    {
        if (ball == null) yield break;

        // Calculate direction to the ball
        Vector3 directionToBall = (ball.transform.position - golfClubVisual.position).normalized;

        // Project the direction onto the local space of the golf club
        Vector3 localDirection = golfClubVisual.InverseTransformDirection(directionToBall);

        // Determine swing rotations based on direction
        Quaternion initialRotation = golfClubVisual.localRotation;
        Quaternion backSwingRotation = initialRotation * Quaternion.Euler(-45 * localDirection.z, -45 * localDirection.x, 0);
        Quaternion forwardSwingRotation = initialRotation * Quaternion.Euler(45 * localDirection.z, 45 * localDirection.x, 0);

        // Back swing
        float duration = 0.25f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            golfClubVisual.localRotation = Quaternion.Slerp(initialRotation, forwardSwingRotation, t / duration);
            yield return null;
        }

        // Forward swing
        duration = 0.25f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            golfClubVisual.localRotation = Quaternion.Slerp(forwardSwingRotation, backSwingRotation, t / duration);
            yield return null;
        }

        // Return to initial position
        golfClubVisual.localRotation = initialRotation;
    }
    private void OnPrimaryTriggerDown() { /* Optional additional logic for trigger down */ }

    private void OnPrimaryTriggerUp() { /* Optional additional logic for trigger up */ }

    private void OnPrimaryGrabBegin()
    {
        isHeld = true;
        Debug.Log("Paddle grabbed.");
        previousPosition = transform.position;

        if (grabComponent.CurrentUser != null)
        {
            this.player = grabComponent.CurrentUser;
            Debug.Log($"Owner set to {player.NickName}");

            visualBar.SetActive(MassiveLoopClient.IsInDesktopMode);
        }
    }

    private void OnPrimaryGrabEnd()
    {
        isHeld = false;
        Debug.Log("Paddle released.");
        this.player = null;
    }

    private void SpawnBall()
    {
        if (ball != null && MassiveLoopClient.IsMasterClient)
        {
            ball.transform.position = ballSpawnPoint.transform.position;
            ballRigidbody = ball.GetComponent<Rigidbody>();
            ballRigidbody.velocity = Vector3.zero;
            Debug.Log("Spawned new ball at spawn point.");
        }
    }
}
