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
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isHeld && other.gameObject == ball)
        {
            Vector3 hitDirection = (ball.transform.position - transform.position).normalized;

            // Apply force to the ball
            ballRigidbody.AddForce(hitDirection * forceMultiplier, ForceMode.Impulse);

            Debug.Log("Ball hit by paddle.");
            Debug.Log("Hit direction: " + hitDirection);
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
        Debug.Log("Paddle grabbed.");
    }

    private void OnPrimaryGrabEnd()
    {
        isHeld = false;
        Debug.Log("Paddle released.");
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
