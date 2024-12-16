using ML.SDK;
using UnityEngine;

public class DodgeballScript : MonoBehaviour
{
    [SerializeField] private MLGrab grabComponent;
    [SerializeField] private Rigidbody ballRigidbody;
    private MLPlayer currentUser;

    // Size limits for size change (relative change)
    [SerializeField] private float minSize = 0.5f; // Minimum scale factor
    [SerializeField] private float maxSize = 2.0f; // Maximum scale factor

    // Absolute size limits for local scale
    [SerializeField] private float absoluteMinSize = 0.3f;
    [SerializeField] private float absoluteMaxSize = 3.0f;

    [SerializeField] private float throwForceMultiplier = 10.0f; // Multiplier for throw force
    [SerializeField] private float maxThrowForce = 100.0f; // Cap for throw force magnitude

    [SerializeField] private Material BlueTeamMaterial; // Change to this material whenever a blue player is holding the ball
    [SerializeField] private Material RedTeamMaterial; // Change to this material whenever a blue player is holding the ball
    [SerializeField] private Material ResetMaterial; // Change to this material whenever The game gets reset
    [SerializeField] private GameObject HitEffect;

    [SerializeField] private GameObject DodgeBallGameManagerObjectReference;

    //  [SerializeField] private TrailRenderer LineRenderer; //Change this color to whichever team is holding it.


    private Vector3 initialScale;
    private float initialDistance;
    private bool isSizeChangeEnabled = false;
    private bool isHeld = false;

    [SerializeField] private GameObject objectToChange;
    private Rigidbody objectRigidbody;
    private Vector3 lastPosition;

    const string EVENT_ID_Throw = "ThrowEvent";
    private EventToken tokenThrow;
    public MLPlayer player;

    const string EVENT_HIT = "HitEvent";
    private EventToken tokenHit;

    private DodgeBallGameManager DodgeBallGameManagerSCRIPTReference;

    private void Start()
    {
        // Setup event listeners
        grabComponent.OnPrimaryGrabBegin.AddListener(OnPrimaryGrabBegin);
        grabComponent.OnPrimaryGrabEnd.AddListener(OnPrimaryGrabEnd);
        grabComponent.OnSecondaryGrabBegin.AddListener(OnSecondaryGrabBegin);
        grabComponent.OnSecondaryGrabEnd.AddListener(OnSecondaryGrabEnd);
        grabComponent.OnPrimaryTriggerDown.AddListener(OnPrimaryTriggerDown);

        tokenThrow = this.AddEventHandler(EVENT_ID_Throw, OnThrowEvent);
        tokenHit = this.AddEventHandler(EVENT_HIT, OnHitEvent);

        // Ensure Rigidbody is attached
        objectRigidbody = objectToChange.GetComponent<Rigidbody>();
        if (objectRigidbody == null)
        {
            Debug.LogError("No Rigidbody component found on the object!");
        }

        DodgeBallGameManagerSCRIPTReference = (DodgeBallGameManager)DodgeBallGameManagerObjectReference.GetComponent(typeof(DodgeBallGameManager));


    }


    private void FixedUpdate()
    {
        // Handle size change during secondary grab
        if (isSizeChangeEnabled && grabComponent.PrimaryHand && grabComponent.SecondaryHand)
        {
            AdjustSizeBasedOnGrab();
        }

        // Update last position while object is held
        if (isHeld && grabComponent.PrimaryHand)
        {
            lastPosition = grabComponent.PrimaryHand.transform.position;
        }

        if (DodgeBallGameManagerSCRIPTReference.isGameActive == false)
        {
            DodgeBallGameManagerSCRIPTReference.OnResetBallPosition();
            grabComponent.ForceRelease();
            ballRigidbody.isKinematic = true;
        }else if(DodgeBallGameManagerSCRIPTReference.isGameActive == true)
        {
            ballRigidbody.isKinematic = false;
        }

        /*
        if (MassiveLoopClient.IsInDesktopMode)
        { 
            if(player.UserInput != null)
            {
                if(player.UserInput.ALT)
            }
        }*/
    }


    private void OnPrimaryTriggerDown()
    {
        grabComponent.ForceRelease();
        // this.InvokeNetwork(EVENT_ID_Throw, EventTarget.Master, null);
    }
    private void OnPrimaryGrabBegin()
    {
        if (grabComponent.CurrentUser != null)
        {
            //    player = grabComponent.CurrentUser;
            isHeld = true;
            currentUser = grabComponent.CurrentUser;
            lastPosition = grabComponent.PrimaryHand.transform.position;

            if (grabComponent.GrabMechanic == SDKGrabMechanicsBehaviorV2.Custom)
            {
                objectToChange.transform.parent = grabComponent.PrimaryHand.transform;
            }


            if ((string)currentUser.GetProperty("team") == "Blue")
            {
                MeshRenderer meshRenderReference = (MeshRenderer)this.gameObject.GetComponent(typeof(MeshRenderer));
                meshRenderReference.material = BlueTeamMaterial;
           //     LineRenderer.startColor = Color.blue;

            }else if ((string) currentUser.GetProperty("team") == "Red")
            {
                MeshRenderer meshRenderReference = (MeshRenderer)this.gameObject.GetComponent(typeof(MeshRenderer));
                meshRenderReference.material = RedTeamMaterial;
            //    LineRenderer.startColor = Color.red;

            }
        }
    }

    private void OnPrimaryGrabEnd()
    {
        // Unparent the object if using custom grab mechanics
        if (grabComponent.GrabMechanic == SDKGrabMechanicsBehaviorV2.Custom)
        {
            transform.parent = null;
        }

        if (isHeld && objectRigidbody != null)
        {
            // Ensure Rigidbody is dynamic
            objectRigidbody.isKinematic = false;

            // Calculate the release direction based on the hand's orientation
            Vector3 releaseDirection = grabComponent.PrimaryHand.transform.forward;

            // Scale the release force
            Vector3 throwForce = releaseDirection * throwForceMultiplier;

            // Clamp the force to a maximum value
            if (throwForce.magnitude > maxThrowForce)
            {
                throwForce = throwForce.normalized * maxThrowForce;
            }

            // Apply the force as an impulse
        //    objectRigidbody.AddForce(throwForce, ForceMode.Impulse);
            Debug.Log($"Object thrown with impulse force: {throwForce}");
            this.InvokeNetwork(EVENT_ID_Throw, EventTarget.All, null, throwForce, this.gameObject.GetInstanceID());


            // Debug visualization for force direction
            //    Debug.DrawRay(transform.position, releaseDirection * 2.0f, Color.red, 2.0f);
           
            //   this.InvokeNetwork(EVENT_ID_Throw, EventTarget.Master, null, throwForce);
        }

        isHeld = false; // Reset the held flag
    }
    private void OnSecondaryGrabBegin()
    {
        if (grabComponent.PrimaryHand && grabComponent.SecondaryHand)
        {
            isSizeChangeEnabled = true;
            initialDistance = Vector3.Distance(
                grabComponent.PrimaryHand.transform.position,
                grabComponent.SecondaryHand.transform.position
            );
            initialScale = objectToChange.transform.localScale;
        }
    }

    private void OnSecondaryGrabEnd()
    {
        isSizeChangeEnabled = false;
    }

    private void AdjustSizeBasedOnGrab()
    {
        // Calculate current distance between hands
        float currentDistance = Vector3.Distance(
            grabComponent.PrimaryHand.transform.position,
            grabComponent.SecondaryHand.transform.position
        );

        // Calculate scale factor and new scale
        float scaleFactor = Mathf.Clamp(currentDistance / initialDistance, minSize, maxSize);
        Vector3 newScale = initialScale * scaleFactor;

        // Clamp to absolute scale limits
        newScale.x = Mathf.Clamp(newScale.x, absoluteMinSize, absoluteMaxSize);
        newScale.y = Mathf.Clamp(newScale.y, absoluteMinSize, absoluteMaxSize);
        newScale.z = Mathf.Clamp(newScale.z, absoluteMinSize, absoluteMaxSize);

        objectToChange.transform.localScale = newScale;
    }

    private void OnThrowEvent(object[] args)
    {
        Debug.Log($"Throw event triggered. Passed in force amount : {(Vector3)args[0]}");

        if ((int)args[1] != this.gameObject.GetInstanceID() || this.gameObject == null)
        {
            return;
        }

        if (this == null || gameObject == null || gameObject.name == null || args[0] == null)
        {
            return;
        }

        // Calculate the release direction based on the hand's orientation
        Vector3 releaseDirection = this.gameObject.transform.forward;

        // Scale the release force
        Vector3 throwForce = releaseDirection * throwForceMultiplier;

        // Clamp the force to a maximum value
        if (throwForce.magnitude > maxThrowForce)
        {
            throwForce = throwForce.normalized * maxThrowForce;
        }


        if (MassiveLoopClient.IsMasterClient)
        {
        //    this.gameObject.RequestOwnership();
            objectRigidbody.AddForce(throwForce, ForceMode.Impulse);
        }
        else
        {
            objectRigidbody.AddForce(throwForce, ForceMode.Impulse);
        }


    }

    private void OnHitEvent(object[] args) {

        int throwingPlayer = (int)args[0];
        int hitPlayer = (int)args[1];

        Object.Instantiate(HitEffect, this.gameObject.transform.position, Quaternion.identity);

        MLPlayer hitplyerMLReference = MassiveLoopRoom.FindPlayerByActorNumber(hitPlayer);

        DodgeBallGameManagerSCRIPTReference.OnPlayerHit(hitplyerMLReference);

       // Debug.Log($"Player : {throwingPlayer} hit player : {hitPlayer} with a dodgeball ");
    
    }


    private void OnGrabBegin()
    {
        currentUser = grabComponent.CurrentUser;
    }

    private void OnGrabEnd()
    {
        currentUser = null; // Reset current user when the ball is released
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the ball collides with a player
        MLPlayer hitPlayer = (MLPlayer)collision.gameObject.GetPlayer();

        if (hitPlayer != null && currentUser != null)
        {
            Debug.Log($"Player found : {hitPlayer.NickName}");
            // Ensure the hit player is not the current user
            if (hitPlayer != currentUser)
            {
                // Check team property
                string currentUserTeam = (string)currentUser.GetProperty("team");
                string hitPlayerTeam = (string)hitPlayer.GetProperty("team");

                if (currentUser != null && hitPlayer != null && hitPlayerTeam != currentUserTeam)
                {
                    Debug.Log("Local hit detected from ball thrower attempting to call network function");
                    this.InvokeNetwork(EVENT_HIT, EventTarget.All, null, currentUser.ActorId, hitPlayer.ActorId);

                    currentUserTeam = "Empty";
                    hitPlayerTeam = "none";
                }


                if (hitPlayerTeam != currentUserTeam)
                {
                   // Debug.Log($"Player from team {hitPlayerTeam} was hit by team {currentUserTeam}!");

                    // Handle game logic for a valid hit
                    OnPlayerHit(hitPlayer);
                    currentUser = null;
                }
                else
                {
                    //Debug.Log("Hit player is on the same team. No action taken.");
                }
            }
        }
    }

    /*private void OnTriggerEnter(Collider collision)
    {
        // Check if the ball collides with a player
        MLPlayer hitPlayer = (MLPlayer)collision.gameObject.GetPlayer();

        if (hitPlayer != null && currentUser != null)
        {
            Debug.Log($"Player found : {hitPlayer.NickName}");
            // Ensure the hit player is not the current user
            if (hitPlayer != currentUser)
            {
                // Check team property
                string currentUserTeam = (string)currentUser.GetProperty("team");
                string hitPlayerTeam = (string)hitPlayer.GetProperty("team");

                if (hitPlayerTeam != currentUserTeam)
                {
                    Debug.Log($"Player from team {hitPlayerTeam} was hit by team {currentUserTeam}!");

                    // Handle game logic for a valid hit
                    OnPlayerHit(hitPlayer);
                    currentUser = null;
                }
                else
                {
                    Debug.Log("Hit player is on the same team. No action taken.");
                }
            }
        }
    }
    */

    private void OnPlayerHit(MLPlayer hitPlayer)
    {
        // Logic for when a player is hit
       // Debug.Log($"Player {hitPlayer.NickName} has been hit!");
        this.InvokeNetwork(EVENT_HIT, EventTarget.All, null, currentUser.ActorId, hitPlayer.ActorId);
        // Example: Disable player temporarily
        //  hitPlayer.gameObject.SetActive(false);

        // Optionally, add more game-specific logic here (e.g., scoring, respawn mechanics)
    }
}
