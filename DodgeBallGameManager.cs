using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using ML.SDK;
using TMPro;
using System.Linq;

public class DodgeBallGameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject BlueTeleportObjectLocation;

    [SerializeField]
    private GameObject RedTeleportObjectLocation;

    [SerializeField]
    private GameObject ResetBallPosition;

    [SerializeField]
    private GameObject GameBall;

    [SerializeField]
    private GameObject RespawnPosition;

    [SerializeField]
    private ML.SDK.MLClickable taggersClickable;

    [SerializeField]
    private ML.SDK.MLClickable runnersClickable;

    [SerializeField]
    private ML.SDK.MLClickable StartGameClickable;

    [SerializeField]
    private TextMeshPro timeText;

    public List<MLPlayer> playersArray = new List<MLPlayer>();

    public float TimeLimit;
    public TextMeshPro GameStatusText;
    public TextMeshPro BlueTeamString;
    public TextMeshPro RedTeamString;
    public TextMeshPro BlueTeamScore;
    public TextMeshPro RedTeamScore;

    private Stopwatch stopwatch;
    private List<string> blueTeam = new List<string>();
    private List<string> redTeam = new List<string>();

    const string EVENT_SELECT_TEAM = "TeamSelectEvent";
    private EventToken tokenSelectTeam;
    const string EVENT_START_GAME = "EventStartGame";
    private EventToken tokenStartGame;

    const string EVENT_TELEPORT_PLAYERS_INTO_ARENA = "TeleportPlayersEvent";
    private EventToken tokenTeleportPlayers;

    const string EVENT_TELEPORT_PLAYERS_BACK_TO_SPAWN = "TeleportPlayersEvent_BackToSpawn";
    private EventToken tokenTeleportPlayers_backToSpawn;

    public bool isGameActive = false;

    public GameObject BluePortal;
    public GameObject RedPortal;

    public GameObject BlueCelebration;
    public GameObject RedCelebration;
    public GameObject Tip;
    public Collider ArenaCollider;
    public GameObject[] GameBalls;

    private int blueTeamNumberScore;
    private int redTeamNumberScore;

    public void OnStartGameEvent(object[] args)
    {



        GameStatusText.text = "Game Active";
        GameStatusText.color = Color.green;

        isGameActive = true;
        stopwatch.Start();
        TeleportPlayersToArena();

        Tip.SetActive(true);

        BluePortal.SetActive(false);
        RedPortal.SetActive(false);

        BlueCelebration.SetActive(false);
        RedCelebration.SetActive(false);

        if (isGameActive == true)
        {
            taggersClickable.gameObject.SetActive(false);
            runnersClickable.gameObject.SetActive(false);

        }

    }


    public void OnResetBallPosition()
    {
     //   GameBall.transform.position = ResetBallPosition.transform.position;

        foreach( GameObject b in GameBalls)
        {
            b.transform.position = ResetBallPosition.transform.position;
        }
    }

    public void OnplayerClickStart(MLPlayer player)
    {
        // Player started the game
        if(player.GetProperty("team") != null && (string)player.GetProperty("team") != "none")
        {
            this.InvokeNetwork(EVENT_START_GAME, EventTarget.All, null);
            this.InvokeNetwork(EVENT_TELEPORT_PLAYERS_INTO_ARENA, EventTarget.All, null);
        }
    }

    public void OnTeleportPlayers(object[] args)
    {
        TeleportPlayersToArena();
    }

    public void OnTeleportPlayersBackToSpawn(object[] args)
    {
        TeleportPlayersToSpawn();
    }

    void Start()
    {
        stopwatch = new Stopwatch();

        if (taggersClickable != null)
        {
            taggersClickable.OnPlayerClick.AddListener(player => AssignTeam(player, "Blue"));
        }

        if (runnersClickable != null)
        {
            runnersClickable.OnPlayerClick.AddListener(player => AssignTeam(player, "Red"));
        }

        if (StartGameClickable != null)
        {
            StartGameClickable.OnPlayerClick.AddListener(OnplayerClickStart);
        }

        tokenSelectTeam = this.AddEventHandler(EVENT_SELECT_TEAM, OnTeamSelectEvent);
        tokenStartGame = this.AddEventHandler(EVENT_START_GAME, OnStartGameEvent);
        tokenTeleportPlayers = this.AddEventHandler(EVENT_TELEPORT_PLAYERS_INTO_ARENA, OnTeleportPlayers);
        tokenTeleportPlayers_backToSpawn = this.AddEventHandler(EVENT_TELEPORT_PLAYERS_BACK_TO_SPAWN, OnTeleportPlayersBackToSpawn);

        MassiveLoopRoom.OnPlayerLeft += OnPlayerLeft;
    }

    private void Update()
    {
        if (stopwatch != null && stopwatch.IsRunning)
        {
            float remainingTime = TimeLimit - (float)stopwatch.Elapsed.TotalSeconds;
            if (remainingTime <= 0)
            {
                stopwatch.Stop();
                EndGame();
            }
            else
            {
                timeText.text = $"Time: \n {remainingTime:F2} \n ";
            }

            if (blueTeam.Count == 0)
            {
                stopwatch.Stop();
                EndGame();

            }
            else if (redTeam.Count == 0)
            {
                stopwatch.Stop();
                EndGame();

            }

        }


    }

    private void AssignTeam(MLPlayer player, string team)
    {
        string playerName = player.NickName;

      //  player.SetProperty("team", team); // Assign the team property

        // Add the player to the playersArray if not already added
        /*if (!playersArray.Contains(player))
        {
            playersArray.Add(player);
        }*/

        // Synchronize team assignment across all players
        this.InvokeNetwork(EVENT_SELECT_TEAM, EventTarget.All, null, playerName, team);
        UpdateTeamLocally(playerName, team);
    }

    private void OnTeamSelectEvent(object[] args)
    {
        string playerName = args[0] as string;
        string team = args[1] as string;

        UnityEngine.Debug.Log($"Passed in team is : {team}");

        MLPlayer truePlayer = MassiveLoopRoom.FindPlayerByName(playerName);
        truePlayer.SetProperty("team", team);
        UnityEngine.Debug.Log($"Player {truePlayer.NickName} team set to : {truePlayer.GetProperty("team")}");
        UpdateTeamLocally(playerName, team);
    }

    private void UpdateTeamLocally(string playerName, string team)
    {
        if (team == "Blue")
        {
            if (!blueTeam.Contains(playerName))
            {
                if (redTeam.Contains(playerName)) redTeam.Remove(playerName);
                blueTeam.Add(playerName);
            }
        }
        else if (team == "Red")
        {
            if (!redTeam.Contains(playerName))
            {
                if (blueTeam.Contains(playerName)) blueTeam.Remove(playerName);
                redTeam.Add(playerName);
            }
        }

        UpdateTeamText();
    }


    private void TeleportPlayersToArena()
    {
        MLPlayer localPlayer = MassiveLoopRoom.GetLocalPlayer();
        MLPlayer[] playersArray = MassiveLoopRoom.FindPlayersInCollider(ArenaCollider);
        

        // Check if the localPlayer has a valid "team" property
        if (localPlayer.GetProperty("team") != null)
        {
            // If the player is already in the arena, do nothing
            if (playersArray.Contains(localPlayer))
            {
                UnityEngine.Debug.Log("Local player is already in the arena, skipping teleport.");
                return;
            }

            UnityEngine.Debug.Log("Get Property on local player did not return null");
            string team = (string)localPlayer.GetProperty("team");
            if (team == "Red" && !playersArray.Contains(localPlayer))
            {
                UnityEngine.Debug.Log("Red team member found");
                localPlayer.Teleport(RedTeleportObjectLocation.transform.position);
            }
            else if (team == "Blue" && !playersArray.Contains(localPlayer))
            {
                UnityEngine.Debug.Log("Blue team member found");
                localPlayer.Teleport(BlueTeleportObjectLocation.transform.position);
            }
        }

        /*
        foreach (MLPlayer player in playersArray)
        {
            string playerName = player.NickName;

            // Use FindPlayerByName to ensure we're working with the correct player object
            MLPlayer truePlayer = MassiveLoopRoom.FindPlayerByName(playerName);

            // Retrieve the player's team and teleport accordingly
            string team = (string)truePlayer.GetProperty("team");
            if (team == "Blue")
            {
                truePlayer.Teleport(BlueTeleportObjectLocation.transform.position);
            }
            else if (team == "Red")
            {
                truePlayer.Teleport(RedTeleportObjectLocation.transform.position);
            }
        }
        */
    }


    private void TeleportPlayersToSpawn()
    {
        /*foreach (MLPlayer player in MassiveLoopRoom.GetAllPlayers())
        {
            player.Teleport(RespawnPosition.transform.position); // Teleport back to the spawn location
        }*/

        MLPlayer localPlayer = MassiveLoopRoom.GetLocalPlayer();
        localPlayer.Teleport(RespawnPosition.transform.position); // Teleport back to the spawn location
    }

    private void UpdateTeamText()
    {
        BlueTeamString.text = $"Blue Team: \n {string.Join(", \n", blueTeam)}  \n";
        RedTeamString.text = $"Red Team: \n {string.Join(", \n", redTeam)}  \n";
    }

    private void EndGame()
    {
        int blueTeamCount = blueTeam.Count;
        int redTeamCount = redTeam.Count;
        isGameActive = false;

        BluePortal.SetActive(true);
        RedPortal.SetActive(true);

        Tip.SetActive(false);

        if (blueTeamCount > redTeamCount)
        {
            GameStatusText.text = "Blue Team Wins!";
            GameStatusText.color = Color.cyan;
            BlueCelebration.SetActive(true);
            RedCelebration.SetActive(false);

            blueTeamNumberScore += 1;
            BlueTeamScore.text = blueTeamNumberScore.ToString();
        }
        else if (redTeamCount > blueTeamCount)
        {
            GameStatusText.text = "Red Team Wins!";
            GameStatusText.color = Color.red;
            BlueCelebration.SetActive(false);
            RedCelebration.SetActive(true);

            redTeamNumberScore += 1;
            RedTeamScore.text = redTeamNumberScore.ToString();

        }
        else
        {
            GameStatusText.text = "It's a Tie!";
            GameStatusText.color = new Color(255f / 255f, 97f / 255f, 0f / 255f, 1f); // RGBA

        }

        this.InvokeNetwork(EVENT_TELEPORT_PLAYERS_BACK_TO_SPAWN, EventTarget.All, null);
        ResetGame();
    }

    private void ResetGame()
    {
        blueTeam.Clear();
        redTeam.Clear();
        UpdateTeamText();
        stopwatch.Reset();


        MLPlayer localPlayer = MassiveLoopRoom.GetLocalPlayer();
        localPlayer.SetProperty("team", "none");


        if (isGameActive == false)
        {
            taggersClickable.gameObject.SetActive(true);
            runnersClickable.gameObject.SetActive(true);
        }

    }


    public void OnPlayerHit(MLPlayer player)
    {
        string playerName = player.NickName;
        UnityEngine.Debug.Log($"Player hit, recieved by manager : {player.NickName}");

        if (blueTeam.Contains(playerName))
        {
            blueTeam.Remove(playerName);
        }
        else if (redTeam.Contains(playerName))
        {
            redTeam.Remove(playerName);
        }
        //Teleport the player back to spawn
        player.Teleport(RespawnPosition.transform.position);
        player.SetProperty("team", "none");

        UpdateTeamText();

    }


    private void OnPlayerLeft(MLPlayer player)
    {
        string playerName = player.NickName;

        if (blueTeam.Contains(playerName))
        {
            blueTeam.Remove(playerName);
        }
        else if (redTeam.Contains(playerName))
        {
            redTeam.Remove(playerName);
        }

        // Remove the player from the playersArray
        /*if (playersArray.Contains(player))
        {
            playersArray.Remove(player);
        }*/

        UpdateTeamText();
    }
}
