using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.SDK;
using UnityEngine.UI;

public class Paddle2 : MonoBehaviour
{
    public GameObject ball;
    public GameObject ballSpawnPoint;
    public Rigidbody ballRigidbody;
    public MLGrab grabComponent;
    public float forceMultiplier = 10f;
    public float sphereCastRadius = 0.1f; // Radius for sphere cast
    public float maxDetectionDistance = 0.5f; // Maximum distance to check for collisions

    private bool isHeld;
    private bool clientIsOwner = false;

    const string EVENT_ID = "HitEvent";
    EventToken token;


    private void OnHitEvent(object[] args)
    {
        if (isHeld && ball != null)
        {
            Vector3 direction = ball.transform.position - transform.position;
            RaycastHit hit;

            // Perform a sphere cast in the direction of the ball to check for collisions
            if (Physics.SphereCast(transform.position, sphereCastRadius, direction, out hit, maxDetectionDistance))
            {
                if (hit.collider.gameObject == ball)
                {
                    Debug.Log("SphereCast detected the ball. Applying force.");

                    Vector3 hitDirection = direction.normalized;

                    // Apply force to the ball
                    ballRigidbody.AddForce(hitDirection * forceMultiplier, ForceMode.Impulse);
                }
            }
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
            grabComponent.OnPrimaryTriggerDown.AddListener(SpawnBall);
            grabComponent.OnPrimaryTriggerUp.AddListener(OnPrimaryTriggerUp);
            token = this.AddEventHandler(EVENT_ID, OnHitEvent);

        }

        isHeld = false;
    }

    void FixedUpdate()
    {

          if (isHeld && ball != null)
          {
              Vector3 direction = ball.transform.position - transform.position;
              RaycastHit hit;

              // Perform a sphere cast in the direction of the ball to check for collisions
              if (Physics.SphereCast(transform.position, sphereCastRadius, direction, out hit, maxDetectionDistance))
              {
                  if (hit.collider.gameObject == ball)
                  {
                      Debug.Log("SphereCast detected the ball. Applying force.");

                      Vector3 hitDirection = direction.normalized;

                    // Apply force to the ball
                    ballRigidbody.AddForce(hitDirection * forceMultiplier, ForceMode.Impulse);
                    this.InvokeNetwork(EVENT_ID, EventTarget.All, null);
                }
            }
          }


    }

    void OnTriggerEnter(Collider other)
    {

        this.InvokeNetwork(EVENT_ID, EventTarget.All, null);


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
        Debug.Log("Paddle grabbed.");
    }

    private void OnPrimaryGrabEnd()
    {
        isHeld = false;
        Debug.Log("Paddle released.");
    }

    private void SpawnBall()
    {
        if (ball != null && clientIsOwner)
        {
            ball.transform.position = ballSpawnPoint.transform.position;
            ballRigidbody = ball.GetComponent<Rigidbody>();
            ballRigidbody.velocity = Vector3.zero;
            Debug.Log("Spawned new ball at spawn point.");
        }
    }
}
