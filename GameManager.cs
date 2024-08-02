using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.SDK;
using UnityEngine.UIElements;
using System.Diagnostics;


/* Game Manager :
 * Not complete yet
 * 
 * Idea behind this script is to give the players a way to interface with the table tennis menu
 * We capture the MLClickable component from each button
 * and add listeners to each button to tell when they've been pressed.
 * 
 * The event of pressing a button will eventually record the players name
 * begin a timer, and then initialize the scoreboard.
 * 
 */

public class GameManager : MonoBehaviour
{

    public MLClickable player1_button;
    public MLClickable player2_button;
    public MLClickable startGameButton;

    private int player_1score;
    private int player_2score;

    public GameObject paddle_1;
    public GameObject paddle_2;
    public GameObject ball;

   // public TextMesh score_text;


    public void OnPlayer_1Interact()
    {
        UnityEngine.Debug.Log("P1 Clicked!");
    }

    public void OnPlayer_2Interact()
    {
        UnityEngine.Debug.Log("P2 Clicked!");
    }

    public void OnStartGameButtonInteract()
    {
        UnityEngine.Debug.Log("SB Clicked!");
    }



    void Start()
    {

        UnityEngine.Debug.Log("MlClickable Test");

        if (player1_button != null)
        {
            // Not working properly.
            player1_button.OnClick.AddListener(OnPlayer_1Interact);
            player2_button.OnClick.AddListener(OnPlayer_2Interact);
            startGameButton.OnClick.AddListener(OnStartGameButtonInteract);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
