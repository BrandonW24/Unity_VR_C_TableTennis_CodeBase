using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.SDK;
using UnityEngine.UI;

public class Paddle2 : MonoBehaviour
{
    public GameObject ball;
    public GameObject ballSpawnPoint;
    public GameObject hitTransformSpot;
    // public TextMesh distanceText;
    // public AudioSource hitAudio;
    public float clubSpeed;
    public int rangeNum;
    public bool isDrivingRange;
    public float forceMultiplier = 10f; // Add this line to define a force multiplier
    public float hitThreshold = 0.1f; // Add this line to define a hit threshold

    public Rigidbody ballRigidbody;
    public MLGrab grabComponent;
    private bool isHeld;
    private Vector3 startingClubPosition;
    private Quaternion startingClubRotation;
    private Vector3 currentClubPosition;
    private Vector3 previousClubPosition;
    private float deltaDistance;
    private float ballToClubDistance;
    private float timeElapsed;
    private float timeBetweenSignUpdates = 3.0f;
    private bool debugMode = true; // Set to true for enabling debug mode
    private Transform currentPlayer;
    private Transform owner;
    private float defaultScale;

    void Start()
    {
        Debug.Log("Paddle2 start");

        if (grabComponent != null)
        {
            grabComponent.OnPrimaryGrabBegin.AddListener(OnPrimaryGrabBegin);
            grabComponent.OnPrimaryGrabEnd.AddListener(OnPrimaryGrabEnd);
            grabComponent.OnPrimaryTriggerDown.AddListener(SpawnBall);
            grabComponent.OnPrimaryTriggerUp.AddListener(OnPrimaryTriggerUp);
        }

        isHeld = false;

        startingClubPosition = transform.position;
        startingClubRotation = transform.rotation;
        currentClubPosition = hitTransformSpot.transform.position;

        previousClubPosition = transform.position;

        defaultScale = transform.localScale.x; // Assuming uniform scaling
    }

    void FixedUpdate()
    {
        if (isHeld)
        {
            // owner = MassiveLoopRoom.FindPlayerCloseToPosition(transform.position);
            currentClubPosition = hitTransformSpot.transform.position;

            if (previousClubPosition != Vector3.zero)
            {
                deltaDistance = Vector3.Distance(currentClubPosition, previousClubPosition);

                if (ball != null)
                {
                    CalculateDistance();
                    ballToClubDistance = Vector3.Distance(ball.transform.position, currentClubPosition);
                    // Debug.Log("Ball to club distance : " + ballToClubDistance);
                    if (deltaDistance > (ballToClubDistance - hitThreshold))
                    {
                        HitBallWithClub();
                    }
                }
            }
            previousClubPosition = currentClubPosition;
        }
    }

    private void OnPrimaryTriggerDown()
    {
        // Handle primary trigger down logic if needed
    }

    private void OnPrimaryTriggerUp()
    {
        // Handle primary trigger up logic if needed
    }

    private void OnPrimaryGrabBegin()
    {
        isHeld = true;
        Debug.Log("Club grabbed.");
    }

    private void OnPrimaryGrabEnd()
    {
        isHeld = false;
        Debug.Log("Club released.");
    }

    private void CalculateDistance()
    {
        if (isHeld)
        {
            float ballDistance = 0.0f;
            if (ball != null)
            {
                ballDistance = Vector3.Distance(ball.transform.position, hitTransformSpot.transform.position);
                ballDistance = Mathf.Floor(ballDistance * 10) / 10;
                // Debug.Log("Calculated ball distance: " + ballDistance + " yds.");
            }

            UpdateDistanceSigns(ballDistance);
        }
    }

    private void UpdateDistanceSigns(float ballDistance)
    {
        /* distanceText.text = ballDistance.ToString() + " yds";

        // Implement network updates if needed
        timeElapsed += Time.deltaTime;
        if (timeElapsed > timeBetweenSignUpdates)
        {
            // Send network updates
            timeElapsed = 0;
        } */
    }

    private void HitBallWithClub()
    {
        if (isHeld)
        {
            Vector3 clubForward = hitTransformSpot.transform.forward;
            Vector3 ballVelocity = clubForward * deltaDistance * forceMultiplier;
            ballRigidbody.AddForce(ballVelocity, ForceMode.Impulse);
            // hitAudio.Play();

            Debug.Log("Hit ball with club.");
            Debug.Log("Club forward vector: " + clubForward);
            Debug.Log("Delta distance: " + deltaDistance);
            Debug.Log("Applied force: " + ballVelocity);
        }
    }

    private void SpawnBall()
    {
        if (ball != null)
        {
            ball.transform.position = ballSpawnPoint.transform.position;
            ballRigidbody = ball.GetComponent<Rigidbody>();
            ballRigidbody.velocity = Vector3.zero;
            Debug.Log("Spawned new ball at spawn point.");
        }
    }
}
