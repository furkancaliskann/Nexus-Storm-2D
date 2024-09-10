using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Scoreboard : NetworkBehaviour
{
    private ScoreboardVariables scoreboardVariables;
    private ServerRoundManager serverRoundManager;
    private ServerScoreboardManager serverScoreboardManager;

    [SyncVar] public string nickName;
    [SyncVar] public Teams team;

    private GameObject panel;
    private Transform blueTeamParent;
    private Text blueTeamWinText;
    private Transform redTeamParent;
    private Text redTeamWinText;
    private GameObject playerPrefab;

    public List<GameObject> spawnedScores = new List<GameObject>();

    public void SetVariables(string nickName, Teams team)
    {
        this.nickName = nickName;
        this.team = team;
    }
    public override void OnStartServer()
    {
        serverRoundManager = GameObject.FindGameObjectWithTag("ServerManager").GetComponent<ServerRoundManager>();
        serverScoreboardManager = serverRoundManager.GetComponent<ServerScoreboardManager>();
        serverScoreboardManager.AddNewPlayer(nickName, team);
    }
    public override void OnStartAuthority()
    {
        if (!isOwned) return;

        scoreboardVariables = Camera.main.GetComponent<ScoreboardVariables>();
        panel = scoreboardVariables.panel;
        blueTeamParent = scoreboardVariables.blueTeamParent;
        blueTeamWinText = scoreboardVariables.blueTeamWinText;
        redTeamParent = scoreboardVariables.redTeamParent;
        redTeamWinText = scoreboardVariables.redTeamWinText;
        playerPrefab = scoreboardVariables.playerPrefab;

        InvokeRepeating(nameof(SendPing), 0f, 1f);

        CmdUpdateScoreRequest();
    }
    void Update()
    {
        if (!isOwned) return;
        CheckInputs();
    }
    private void CheckInputs()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OpenPanel();
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            ClosePanel();
        }
    }
    private void DestroyOldList()
    {
        for (int i = 0; i < spawnedScores.Count; i++)
        {
            Destroy(spawnedScores[i]);
        }

        spawnedScores.Clear();
    }
    private void OpenPanel()
    {
        panel.SetActive(true);
    }
    private void ClosePanel()
    {
        panel.SetActive(false);
    }
    private void SendPing()
    {
        int ping = (int)(NetworkTime.rtt * 1000);
        CmdSendPingValue(ping);
    }
    [Command]
    private void CmdSendPingValue(int ping)
    {
        if (ping > 999) ping = 999;
        serverScoreboardManager.RefreshPing(nickName, team, ping);
    }
    [Command]
    private void CmdUpdateScoreRequest()
    {
        TargetUpdateScores(serverScoreboardManager.blueTeamPlayers, serverScoreboardManager.redTeamPlayers,
            serverRoundManager.blueTeamWinCount, serverRoundManager.redTeamWinCount);
    }
    [TargetRpc]
    public void TargetUpdateScores(List<ScoreboardPlayer> blueTeam, List<ScoreboardPlayer> redTeam, int blueTeamWin, int redTeamWin)
    {
        DestroyOldList();

        blueTeamWinText.text = "Blue Team (" + blueTeamWin + ")";
        redTeamWinText.text = "Red Team (" + redTeamWin + ")";

        for (int i = 0; i < blueTeam.Count; i++)
        {
            GameObject player = Instantiate(playerPrefab, blueTeamParent);
            if (blueTeam[i].nickName == nickName) player.GetComponentsInChildren<Image>()[0].enabled = true;
            player.GetComponentsInChildren<Text>()[0].text = i + 1 + "-)";
            player.GetComponentsInChildren<Text>()[1].text = blueTeam[i].nickName;
            player.GetComponentsInChildren<Text>()[2].text = blueTeam[i].ping + "ms";
            player.GetComponentsInChildren<Text>()[3].text = blueTeam[i].kill.ToString();
            player.GetComponentsInChildren<Text>()[4].text = blueTeam[i].death.ToString();
            player.GetComponentsInChildren<Text>()[5].text = blueTeam[i].damage.ToString();

            spawnedScores.Add(player);
        }

        for (int i = 0; i < redTeam.Count; i++)
        {
            GameObject player = Instantiate(playerPrefab, redTeamParent);
            if (redTeam[i].nickName == nickName) player.GetComponentsInChildren<Image>()[0].enabled = true;
            player.GetComponentsInChildren<Text>()[0].text = i + 1 + "-)";
            player.GetComponentsInChildren<Text>()[1].text = redTeam[i].nickName;
            player.GetComponentsInChildren<Text>()[2].text = redTeam[i].ping + "ms";
            player.GetComponentsInChildren<Text>()[3].text = redTeam[i].kill.ToString();
            player.GetComponentsInChildren<Text>()[4].text = redTeam[i].death.ToString();
            player.GetComponentsInChildren<Text>()[5].text = redTeam[i].damage.ToString();

            spawnedScores.Add(player);
        }
    }
    [TargetRpc]
    public void TargetScoreSceneWithScores()
    {
        OpenPanel();
    }
    [TargetRpc]
    public void TargetLoadMainMenu()
    {
        NetworkManager networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
        if (isServer) return;
        else networkManager.StopClient();
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public override void OnStopServer()
    {
        serverRoundManager.CheckPlayerCount(nickName);
        serverScoreboardManager.RemovePlayer(nickName, team);
    }
}
