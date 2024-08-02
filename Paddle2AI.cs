using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle2AI : MonoBehaviour
{
    //TODO: change this user type to a blank gameobject, it's invoking an error
    public GameObject paddle;
    public Transform aiTriggerBox;
    public float aiSpeed = 7.0f;
    public float windUpDistance = 1.0f;
    public float swingSpeedMultiplier = 2.5f;
    public float delayBeforeSwing = 0.1f;
    public float stoppingDistance = 0.05f;
    public float returnSpeedMultiplier = 1.0f;
    public float ballSpeedThreshold = 0.1f; // Threshold to consider the ball as stopped
    public float ballCheckInterval = 1.0f; // Interval to check ball speed
    public Transform ballRespawnPoint; // Position to respawn the ball
    public float hitForce = 10f; // Force applied to the ball when hit
    public float hitRange = 0.5f; // Range within which the ball can be hit
    public Transform table; // The table's transform to aim at
    public float tableHeight = 0.75f; // Height of the table for aiming
   // public LayerMask floorLayer; // Layer for the floor to check collisions

   // private GameObject ball;
    private bool isWindingUp = false;
    private bool isSwinging = false;
    private bool isReturning = false;
    private Vector3 windUpPosition;
    private Vector3 targetPosition;
    private Vector3 originalPosition;
    public GameObject ball;


    /* This script holds the AI that moves the paddle to respond to the ball entering its trigger zone.
     *  It could be made to be more accurate, and the serialized numbers can be tweaked to make it a harder
     *  opponent.
     *  
     *  One of the buttons from the game manager would enable this AI player for the normal human player to play against.
     */

    void Start()
    {
        /*
        if (paddle == null)
        {
            paddle = GetComponent<Paddle2>();
        }*/

        if (aiTriggerBox == null)
        {
            Debug.LogError("AI Trigger Box not assigned.");
        }

        if (ballRespawnPoint == null)
        {
            Debug.LogError("Ball Respawn Point not assigned.");
        }

        originalPosition = paddle.transform.position;

        StartCoroutine(CheckBallSpeed());
    }

    void FixedUpdate()
    {
        if (ball != null)
        {
    

            if (IsBallInTriggerBox() && !isSwinging && !isReturning)
            {
                StartCoroutine(WindUpAndSwing());
            }
        }
    }

    private bool IsBallInTriggerBox()
    {
        if (aiTriggerBox != null && ball != null)
        {
            return aiTriggerBox.GetComponent<Collider>().bounds.Contains(ball.transform.position);
        }
        return false;
    }

    private IEnumerator WindUpAndSwing()
    {
        isWindingUp = true;

        // Calculate the wind-up position
        windUpPosition = paddle.transform.position - (paddle.transform.forward * windUpDistance);
        targetPosition = new Vector3(ball.transform.position.x, ball.transform.position.y, ball.transform.position.z);

        // Move the paddle to the wind-up position
        while (Vector3.Distance(paddle.transform.position, windUpPosition) > stoppingDistance)
        {
            paddle.transform.position = Vector3.MoveTowards(paddle.transform.position, windUpPosition, aiSpeed * Time.deltaTime);
            yield return null;
        }

        isWindingUp = false;

        // Add a delay before swinging
        yield return new WaitForSeconds(delayBeforeSwing);

        // Aim the paddle towards the table
        AimTowardsTable();

        isSwinging = true;

        // Move the paddle to hit the ball
        while (Vector3.Distance(paddle.transform.position, targetPosition) > stoppingDistance)
        {
            paddle.transform.position = Vector3.MoveTowards(paddle.transform.position, targetPosition, aiSpeed * swingSpeedMultiplier * Time.deltaTime);

            // Check if the ball is within hit range and hit it
            if (Vector3.Distance(paddle.transform.position, ball.transform.position) <= hitRange)
            {
                HitBall();
                break;
            }

            yield return null;
        }

        isSwinging = false;

        // Move back to original position
        StartCoroutine(ReturnToOriginalPosition());
    }

    private void AimTowardsTable()
    {
        if (table != null)
        {
            Vector3 tablePosition = new Vector3(table.position.x, tableHeight, table.position.z);
            Vector3 directionToTable = (tablePosition - paddle.transform.position).normalized;
            paddle.transform.rotation = Quaternion.LookRotation(directionToTable);
        }
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        isReturning = true;

        // Move the paddle back to its original position
        while (Vector3.Distance(paddle.transform.position, originalPosition) > stoppingDistance)
        {
            paddle.transform.position = Vector3.MoveTowards(paddle.transform.position, originalPosition, aiSpeed * returnSpeedMultiplier * Time.deltaTime);
            yield return null;
        }

        isReturning = false;
    }

    private void HitBall()
    {
        if (ball != null)
        {
            Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();
            Vector3 hitDirection = (ball.transform.position - paddle.transform.position).normalized;
            ballRigidbody.AddForce(hitDirection * hitForce, ForceMode.Impulse);
            Debug.Log("Ball hit by paddle.");
        }
    }

    private IEnumerator CheckBallSpeed()
    {
        while (true)
        {
            yield return new WaitForSeconds(ballCheckInterval);

            if (ball != null)
            {
                Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();

                if (ballRigidbody != null && ballRigidbody.velocity.magnitude < ballSpeedThreshold && IsBallInTriggerBox())
                {
                    RespawnBall();
                }
            }
        }
    }

    private void RespawnBall()
    {
        if (ball != null && ballRespawnPoint != null)
        {
            ball.transform.position = ballRespawnPoint.position;
            ball.GetComponent<Rigidbody>().velocity = Vector3.zero; // Reset the ball's velocity
            Debug.Log("Ball respawned at the given position.");
        }
    }

}