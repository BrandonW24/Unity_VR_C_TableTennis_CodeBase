using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.SDK;
public class Paddle : MonoBehaviour
{
    public GameObject ball;
    public GameObject hitTransformSpot;
    public MLGrab grabComponent;
    private Rigidbody rb;
    public bool isHeld;
    private Transform currentClubPosition;
    private Transform previousPosition;
    private float deltaDistance;
    private float ballToClubDistance;
    public Rigidbody ballRigidbody;
    private Vector3 ballVelocity;
    private Vector3 paddleforward;



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
        isHeld = true;
    }


    void OnPrimaryGrabEnd()
    {
        isHeld = false;

    }

    void CalculateDistance()
    {
        if (isHeld)
        {
            float ballDistance = 0;
            if (ball)
            {
                ballDistance = Vector3.Distance(ball.transform.position, hitTransformSpot.transform.position);
                ballDistance = Mathf.Floor(ballDistance * 10) / 10;
            }

        }
    }



    void HitBallWithPaddle()
    {
        if (isHeld)
        {
            Debug.Log("Hit ball function");
            paddleforward = hitTransformSpot.transform.forward;
            ballVelocity = paddleforward * deltaDistance;
            ballRigidbody.AddForce(ballVelocity, ForceMode.Impulse);

            //hitaudioplay


        }
    }

    // Start is called before the first frame update
    void Start()
    {

        Debug.Log("Start paddle");

        if (grabComponent != null)
        {
            grabComponent.OnPrimaryGrabBegin.AddListener(OnPrimaryGrabBegin);
            grabComponent.OnPrimaryGrabEnd.AddListener(OnPrimaryGrabEnd);
            grabComponent.OnPrimaryTriggerDown.AddListener(OnPrimaryTriggerDown);
            grabComponent.OnPrimaryTriggerUp.AddListener(OnPrimaryTriggerUp);
        }

       
        isHeld = false;
        currentClubPosition.transform.position = hitTransformSpot.transform.position;

    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (isHeld)
        {
            Debug.Log("Paddle is being held");

            currentClubPosition.transform.position = hitTransformSpot.transform.position;
            Debug.Log("Checking previous club position");
            if (previousPosition)
            {
                deltaDistance = Vector3.Distance(currentClubPosition.position, previousPosition.position);
                if (ball)
                {
                    CalculateDistance();

                    ballToClubDistance = Vector3.Distance(ball.transform.position, currentClubPosition.position);
                    if (deltaDistance > ballToClubDistance)
                    {
                        HitBallWithPaddle();
                    }
                }
            }

            previousPosition.position = currentClubPosition.position;

        }


    }
}
