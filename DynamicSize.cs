using ML.SDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicSize : MonoBehaviour
{
    public MLGrab grabComponent;

    // Size limits for size change (relative change)
    [SerializeField] private float minSize = 0.5f; // Minimum scale factor
    [SerializeField] private float maxSize = 2.0f; // Maximum scale factor

    // Absolute size limits for local scale
    [SerializeField] private float absoluteMinSize = 0.3f;
    [SerializeField] private float absoluteMaxSize = 3.0f;

    [SerializeField] private float throwForceMultiplier = 10.0f; // Multiplier for throw force
    [SerializeField] private float maxThrowForce = 100.0f; // Cap for throw force magnitude

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

    private void Start()
    {
        // Setup event listeners
        grabComponent.OnPrimaryGrabBegin.AddListener(OnPrimaryGrabBegin);
        grabComponent.OnPrimaryGrabEnd.AddListener(OnPrimaryGrabEnd);
        grabComponent.OnSecondaryGrabBegin.AddListener(OnSecondaryGrabBegin);
        grabComponent.OnSecondaryGrabEnd.AddListener(OnSecondaryGrabEnd);
        grabComponent.OnPrimaryTriggerDown.AddListener(OnPrimaryTriggerDown);

        tokenThrow = this.AddEventHandler(EVENT_ID_Throw, OnThrowEvent);

        // Ensure Rigidbody is attached
        objectRigidbody = objectToChange.GetComponent<Rigidbody>();
        if (objectRigidbody == null)
        {
            Debug.LogError("No Rigidbody component found on the object!");
        }
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
            lastPosition = grabComponent.PrimaryHand.transform.position;

            if (grabComponent.GrabMechanic == SDKGrabMechanicsBehaviorV2.Custom)
            {
                objectToChange.transform.parent = grabComponent.PrimaryHand.transform;
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
            objectRigidbody.AddForce(throwForce, ForceMode.Impulse);

            // Debug visualization for force direction
        //    Debug.DrawRay(transform.position, releaseDirection * 2.0f, Color.red, 2.0f);
            Debug.Log($"Object thrown with impulse force: {throwForce}");
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
        Debug.Log("Throw event triggered.");
        objectRigidbody.AddForce((Vector3)args[0], ForceMode.Impulse);
        
    }
}
