using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerRoundManager : NetworkBehaviour
{
    private ServerManager serverManager;
    private ServerMessageManager serverMessageManager;
    private ServerScoreboardManager serverScoreboardManager;

    public int blueTeamWinCount {  get; private set; }
    public int redTeamWinCount { get; private set; }
    public int roundForWin { get; private set; }
    public float freezeTime { get; private set; }
    public float freezeTimeMax { get; private set; }
    public float remainedTime { get; private set; }
    public float remainedTimeMax { get; private set; }
    public int newRoundSecond { get; private set; }
    public GameStage gameStage;

    private int sendedTime = -1;
    private Teams latestWinnerTeam;

    public override void OnStartServer()
    {
        serverManager = GetComponent<ServerManager>();
        serverMessageManager = GetComponent<ServerMessageManager>();
        serverScoreboardManager = GetComponent<ServerScoreboardManager>();

        freezeTime = freezeTimeMax = 5f;
        remainedTime = remainedTimeMax = 150;
        roundForWin = 14;
        newRoundSecond = 4;

    }
    void Update()
    {
        if (!isServer) return;
        DecreaseFreezeTime();
        DecreaseRoundTime();
    }
    private void DecreaseFreezeTime()
    {
        if (gameStage != GameStage.PlayersFreezed) return;
        if (freezeTime > 0) freezeTime -= Time.deltaTime;

        int sendedFreezeTime = Mathf.CeilToInt(freezeTime);
        if (sendedTime != sendedFreezeTime)
        {
            SendTimeToAllPlayers(sendedFreezeTime);
            sendedTime = sendedFreezeTime;
        }
            
        if (freezeTime <= 0)
        {
            sendedTime = -1;
            AccessMovementToAllPlayers();
            gameStage = GameStage.RoundStarted;
            freezeTime = freezeTimeMax;
        }
    }
    private void DecreaseRoundTime()
    {
        if (gameStage != GameStage.RoundStarted) return;
        if (remainedTime > 0)
        {
            remainedTime -= Time.deltaTime;

            int sendedRemainedTime = Mathf.CeilToInt(remainedTime);

            if (sendedTime != sendedRemainedTime)
            {
                SendTimeToAllPlayers(sendedRemainedTime);
                sendedTime = sendedRemainedTime;
            }
            if (remainedTime <= 0)
            {
                sendedTime = -1;
                StartCoroutine(CheckRound());
            }
        }
    }
    public void CheckPlayerCount(string nickName)
    {
        if (gameStage == GameStage.Warmup) return;

        List<GameObject> players = new List<GameObject>();
        players.AddRange(GameObject.FindGameObjectsWithTag("PlayerInfoDisplay"));
        players.Remove(players.Find(x => x.GetComponent<Scoreboard>().nickName == nickName));

        List<ScoreboardPlayer> blueTeamPlayers = new List<ScoreboardPlayer>();
        List<ScoreboardPlayer> redTeamPlayers = new List<ScoreboardPlayer>();

        blueTeamPlayers.AddRange(serverScoreboardManager.blueTeamPlayers);
        redTeamPlayers.AddRange(serverScoreboardManager.redTeamPlayers);

        var playerBlue = blueTeamPlayers.Find(x => x.nickName == nickName);
        var playerRed = redTeamPlayers.Find(x => x.nickName == nickName);
        
        if(playerBlue != null) blueTeamPlayers.Remove(playerBlue);
        if(playerRed != null) redTeamPlayers.Remove(playerRed);


        if (players.Count <= 1 || blueTeamPlayers.Count <= 0 || redTeamPlayers.Count <= 0)
        {
            gameStage = GameStage.GameFinished;
            serverMessageManager.SendToAllPlayers("All players of the opposing team left!");
            FinishTheGame();
        }
    }
    public IEnumerator CheckRound()
    {
        yield return new WaitForSeconds(0.1f);
        if (gameStage == GameStage.Warmup || gameStage == GameStage.RoundFinished ||
            gameStage == GameStage.WaitingForNewRound || gameStage == GameStage.GameFinished) yield break;

        List<GameObject> players = GameObject.FindGameObjectsWithTag("Player").ToList();
        List<GameObject> blueTeamAlive = players.FindAll(x => x.GetComponent<PlayerStats>().team == Teams.Blue);
        List<GameObject> redTeamAlive = players.FindAll(x => x.GetComponent<PlayerStats>().team == Teams.Red);

        if (blueTeamAlive.Count <= 0)
        {
            redTeamWinCount++;
            serverMessageManager.SendToAllPlayers("'RED' team wins this round!");
            gameStage = GameStage.WaitingForNewRound;
            SendRoundFinishSoundToAllPlayers(Teams.Red);
            latestWinnerTeam = Teams.Red;
        }
        else if (redTeamAlive.Count <= 0)
        {
            blueTeamWinCount++;
            serverMessageManager.SendToAllPlayers("'BLUE' team wins this round!");
            gameStage = GameStage.WaitingForNewRound;
            SendRoundFinishSoundToAllPlayers(Teams.Blue);
            latestWinnerTeam = Teams.Blue;
        }

        if (remainedTime <= 0)
        {
            int random = Random.Range(0, 10);
            if(random < 5)
            {
                blueTeamWinCount++;
                serverMessageManager.SendToAllPlayers("'BLUE' team wins this round!");
                SendRoundFinishSoundToAllPlayers(Teams.Blue);
                latestWinnerTeam = Teams.Blue;
            }
            else
            {
                redTeamWinCount++;
                serverMessageManager.SendToAllPlayers("'RED' team wins this round!");
                SendRoundFinishSoundToAllPlayers(Teams.Red);
                latestWinnerTeam = Teams.Red;
            }
            gameStage = GameStage.WaitingForNewRound;
            
        }

        if (blueTeamWinCount >= roundForWin || redTeamWinCount >= roundForWin)
        {
            gameStage = GameStage.GameFinished;
            Invoke(nameof(FinishTheGame), newRoundSecond);
        }

        if(gameStage == GameStage.WaitingForNewRound && blueTeamWinCount < roundForWin && redTeamWinCount < roundForWin)
        {
            remainedTime = remainedTimeMax;
            Invoke(nameof(CreateRound), newRoundSecond);
        }
    }
    private void CreateRound()
    {
        NewRound(false);
    }
    public void NewRound(bool first)
    {
        serverMessageManager.SendToAllPlayers("");

        if(first)
        {
            serverScoreboardManager.ResetScores();
            serverScoreboardManager.BalanceTeams();
        }
        
        DestroyDroppedItems();

        GameObject[] oldPlayers = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] deadPlayers = GameObject.FindGameObjectsWithTag("DeadPlayer");

        for (int i = 0; i < oldPlayers.Length; i++)
        {
            NetworkServer.Destroy(oldPlayers[i]);
        }
        for (int i = 0; i < deadPlayers.Length; i++)
        {
            NetworkServer.Destroy(deadPlayers[i]);
        }

        serverManager.SpawnAllPlayers();

        if (first)
            serverScoreboardManager.GiveStartingMoneyToAllPlayers();
        else
        {
            StartCoroutine(serverScoreboardManager.GiveRoundMoneyToAllPlayers(latestWinnerTeam));
        }

        latestWinnerTeam = Teams.Spectator;
        FreezeMovementToAllPlayers();
        gameStage = GameStage.PlayersFreezed;
    }
    private void FinishTheGame()
    {
        serverScoreboardManager.SendScoreToAllPlayers(true);

        GameObject[] player = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < player.Length; i++)
        {
            NetworkServer.Destroy(player[i]);
        }

        Invoke(nameof(DisconnectAllPlayers), 5);
    }
    private void DisconnectAllPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("PlayerInfoDisplay");

        for (int i = 0; i < players.Length; i++)
        {
            players[i].GetComponent<Scoreboard>().TargetLoadMainMenu();
        }

        Invoke(nameof(DisconnectServer), 0.5f);
    }
    private void DisconnectServer()
    {
        NetworkManager networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
        networkManager.StopHost();
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }
    public void DestroyDroppedItems()
    {
        GameObject[] droppedItems = GameObject.FindGameObjectsWithTag("DroppedItem");

        for (int i = 0; i < droppedItems.Length; i++)
        {
            NetworkServer.Destroy(droppedItems[i]);
        }
    }
    private void AccessMovementToAllPlayers()
    {
        GameObject[] player = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < player.Length; i++)
        {
            player[i].GetComponent<PlayerMovement>().TargetSetFreezeVariable(false);
        }

        SendRoundStartedSoundToAllPlayers();
    }
    private void FreezeMovementToAllPlayers()
    {
        GameObject[] player = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < player.Length; i++)
        {
            player[i].GetComponent<PlayerMovement>().TargetSetFreezeVariable(true);
        }
    }
    private void SendTimeToAllPlayers(int time)
    {
        GameObject[] player = GameObject.FindGameObjectsWithTag("PlayerInfoDisplay");

        for (int i = 0; i < player.Length; i++)
        {
            player[i].GetComponent<TimePanel>().TargetUpdateTime(time);
        }
    }
    private void SendRoundStartedSoundToAllPlayers()
    {
        GameObject[] player = GameObject.FindGameObjectsWithTag("PlayerInfoDisplay");
        int random = Random.Range(0, 15);

        int index;
        if (random < 5) index = 0;
        else if (random >= 5 && random < 10) index = 1;
        else index = 2;

        for (int i = 0; i < player.Length; i++)
        {
            RoundSounds roundSounds = player[i].GetComponent<RoundSounds>();
            roundSounds.TargetPlayRoundStartedSound(index);
        }
    }
    private void SendRoundFinishSoundToAllPlayers(Teams winnedTeam)
    {
        GameObject[] player = GameObject.FindGameObjectsWithTag("PlayerInfoDisplay");

        for (int i = 0; i < player.Length; i++)
        {
            RoundSounds roundSounds = player[i].GetComponent<RoundSounds>();

            if (winnedTeam == Teams.Blue) roundSounds.TargetPlayBlueTeamWinsSound();
            else roundSounds.TargetPlayRedTeamWinsSound();
        }
    }

}

public enum GameStage
{
    Warmup,
    PlayersFreezed,
    RoundStarted,
    RoundFinished,
    WaitingForNewRound,
    GameFinished,
}
