using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.SDK;
using Random = UnityEngine.Random;

public class Ball : MonoBehaviour
{
    private MLGrab grabComponent;
    private Rigidbody rb;
    private Vector3 previousPosition;
    private Vector3 velocity;
    private bool isGrabbed = false;
    private float ballRadius;

    public Collider Floor;

    public Transform[] respawnpos;
    public AudioSource ballSound;
    public AudioClip[] ballSounds;
    private int randomIndex;

    void OnPrimaryTriggerDown()
    {
        // Handle primary trigger down logic if needed
    }

    void OnPrimaryTriggerUp()
    {
        // Handle primary trigger up logic if needed
    }

    void OnPrimaryGrabBegin()
    {
        // Pause physics when the ball is grabbed
        if (rb != null)
        {
            rb.isKinematic = true;
            isGrabbed = true;
        }

        Debug.Log("Grabbed");
    }

    void OnCollisionEnter(Collision collision)
    {
        int randomInt = Random.Range(0, 2);
        ballSound.clip = ballSounds[randomInt];
        ballSound.Play();

        if (collision.gameObject.name == "Floor")
        {
            int i = Random.Range(0, 2);

            transform.position = respawnpos[i].position;
            GetComponent<Rigidbody>().velocity = Vector3.zero; // Reset the ball's velocity
            Debug.Log("Ball respawned at the given position.");

            

        }
    }

    void OnPrimaryGrabEnd()
    {
        // Apply custom physics when the ball is released
        if (rb != null)
        {
            rb.isKinematic = false;
            isGrabbed = false;
            // Calculate the velocity based on previous and current position
            velocity = (transform.position - previousPosition) / Time.deltaTime;
            rb.velocity = velocity * grabComponent.ForceMultiplier;
        }
    }

    void Start()
    {
        grabComponent = GetComponent<MLGrab>();
        rb = GetComponent<Rigidbody>();
        ballRadius = GetComponent<SphereCollider>().radius;

        if (grabComponent != null)
        {
            grabComponent.OnPrimaryGrabBegin.AddListener(OnPrimaryGrabBegin);
            grabComponent.OnPrimaryGrabEnd.AddListener(OnPrimaryGrabEnd);
            grabComponent.OnPrimaryTriggerDown.AddListener(OnPrimaryTriggerDown);
            grabComponent.OnPrimaryTriggerUp.AddListener(OnPrimaryTriggerUp);
        }

        randomIndex = 0;

        // Set continuous collision detection mode
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    void FixedUpdate()
    {
        // Update the previous position only when the ball is grabbed
        if (isGrabbed)
        {
            previousPosition = transform.position;
        }

        /*
        // Perform spherecasting for custom collision detection
        RaycastHit hit;
        Vector3 direction = rb.velocity.normalized;
        float distance = rb.velocity.magnitude * Time.deltaTime;

        if (Physics.SphereCast(transform.position, ballRadius, direction, out hit, distance))
        {
            if (hit.collider.CompareTag("Paddle"))
            {
                Debug.Log("Spherecast detected collision with paddle");
                Rigidbody paddleRb = hit.collider.GetComponent<Rigidbody>();
                if (paddleRb != null)
                {
                    // Calculate the impact velocity
                    Vector3 paddleVelocity = paddleRb.velocity;
                    Vector3 impactVelocity = paddleVelocity * grabComponent.ForceMultiplier;
                    


                    // Apply the impact velocity to the ball
                    rb.velocity += impactVelocity;

                    // Adjust the ball's position to prevent clipping
                    transform.position = hit.point;
                }
            }
        }
    }*/
    }
}
