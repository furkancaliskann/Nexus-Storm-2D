using Mirror;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpectatorFirstConnection : NetworkBehaviour
{
    private ServerScoreboardManager serverScoreboardManager;
    private ServerManager serverManager;

    public Teams selectedTeam;
    [SerializeField] private GameObject enterNamePanel;
    [SerializeField] private GameObject teamSelectPanel;
    [SerializeField] private GameObject canvas;

    [SerializeField] private InputField nameInputfield;

    private float timeLeftToSpawn = 1f;
    private bool newPlayerSpawned;

    public override void OnStartServer()
    {
        serverScoreboardManager = GameObject.FindGameObjectWithTag("ServerManager").GetComponent<ServerScoreboardManager>();
        serverManager = serverScoreboardManager.GetComponent<ServerManager>();
    }
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        teamSelectPanel.SetActive(false);
        enterNamePanel.SetActive(true);
        canvas.SetActive(true);
        HideSpawnPointSprites();
    }
    void Update()
    {
        if (!isServer) return;
        if (newPlayerSpawned) return;

        if (selectedTeam != Teams.Spectator && timeLeftToSpawn > 0) timeLeftToSpawn -= Time.deltaTime;
        if (timeLeftToSpawn < 0)
        {
            NetworkIdentity networkIdentity = GetComponent<NetworkIdentity>();
            ServerManager serverManager = GameObject.FindGameObjectWithTag("ServerManager").GetComponent<ServerManager>();

            serverManager.SpawnPlayer(gameObject, Vector2.zero, selectedTeam, nameInputfield.text, networkIdentity.connectionToClient);
            serverManager.SpawnInfoDisplay(selectedTeam, nameInputfield.text, networkIdentity.connectionToClient);
            newPlayerSpawned = true;
        }   
    }
    private void HideSpawnPointSprites()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        foreach (GameObject spawnPoint in spawnPoints)
        {
            spawnPoint.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    public void SaveName()
    {
        CmdSaveName(nameInputfield.text);
    }
    [Command]
    private void CmdSaveName(string name)
    {
        if (name == "") return;
        if (!serverScoreboardManager.IsNameUsable(name)) return;
        nameInputfield.text = name;

        TargetOpenSelectTeamPanel();
    }
    [TargetRpc]
    private void TargetOpenSelectTeamPanel()
    {
        enterNamePanel.SetActive(false);
        teamSelectPanel.SetActive(true);
    }
    [Command]
    public void CmdSelectTeam(int teamId) // 1 -> Blue 2 -> Red
    {
        if (selectedTeam != Teams.Spectator) return;

        if (teamId == 1) selectedTeam = Teams.Blue;
        else if (teamId == 2) selectedTeam = Teams.Red;

        TargetCloseTeamSelectPanel();
    }

    [TargetRpc]
    private void TargetCloseTeamSelectPanel()
    {
        teamSelectPanel.SetActive(false);
    }
}

public enum Teams
{
    Spectator,
    Blue,
    Red
}