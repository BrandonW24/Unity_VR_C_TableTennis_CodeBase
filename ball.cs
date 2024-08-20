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

    private bool clientIsOwner = false;

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
        if (rb != null)
        {
            rb.isKinematic = true;
            isGrabbed = true;
        }

        if (MassiveLoopRoom.GetLocalPlayer().IsMasterClient)
        {
            clientIsOwner = true;
        }

        Debug.Log("Grabbed");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!MassiveLoopRoom.GetLocalPlayer().IsMasterClient)
        {
            return; // Only master client handles collision logic
        }

        int randomInt = Random.Range(0, ballSounds.Length);
        ballSound.clip = ballSounds[randomInt];
        ballSound.Play();

        if (collision.gameObject.name == "Floor")
        {
            int i = Random.Range(0, respawnpos.Length);
            transform.position = respawnpos[i].position;
            rb.velocity = Vector3.zero; // Reset the ball's velocity
            Debug.Log("Ball respawned at the given position.");
        }
    }

    void OnPrimaryGrabEnd()
    {
        if (rb != null)
        {
            rb.isKinematic = false;
            isGrabbed = false;
            velocity = (transform.position - previousPosition) / Time.deltaTime;
            rb.velocity = velocity * grabComponent.ForceMultiplier;
        }
    }

    void Start()
    {
        grabComponent = GetComponent<MLGrab>();
        rb = GetComponent<Rigidbody>();
        ballRadius = GetComponent<SphereCollider>().radius;

        if (MassiveLoopRoom.GetLocalPlayer().IsMasterClient)
        {
            clientIsOwner = true;
        }
        else
        {
          //  rb.isKinematic = true;
        }

        if (grabComponent != null)
        {
            grabComponent.OnPrimaryGrabBegin.AddListener(OnPrimaryGrabBegin);
            grabComponent.OnPrimaryGrabEnd.AddListener(OnPrimaryGrabEnd);
            grabComponent.OnPrimaryTriggerDown.AddListener(OnPrimaryTriggerDown);
            grabComponent.OnPrimaryTriggerUp.AddListener(OnPrimaryTriggerUp);
        }

        randomIndex = 0;

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    private void FixedUpdate()
    {
        if (!clientIsOwner)
        {
            if (MassiveLoopRoom.GetLocalPlayer().IsMasterClient)
            {
                clientIsOwner = true;
            }
            else
            {
              return; // Only master client handles physics updates
            }
        }

        if (!isGrabbed)
        {
            rb.AddForce(Vector3.down * 30);

            Vector3 direction = -rb.velocity.normalized;
            float dragForceMagnitude = 1.225f * rb.velocity.magnitude * rb.velocity.magnitude * 0.47f * (Mathf.PI * 0.4f * 0.4f) / 2;

            if (direction != Vector3.zero)
            {
                rb.AddForce(direction * dragForceMagnitude);
            }
        }

        previousPosition = transform.position;
    }
}
