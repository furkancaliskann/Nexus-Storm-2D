using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

public class ServerManager : NetworkBehaviour
{
    private ServerRoundManager serverRoundManager;
    private ServerScoreboardManager serverScoreboardManager;

    private string map;
    private GameObject mapObject;
    public List<GameObject> redSpawnPoints = new List<GameObject>();
    public List<GameObject> blueSpawnPoints = new List<GameObject>();

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject infoDisplayPrefab;

    void Start()
    {
        if (!isServer) return;
        SpawnMap();
        FindSpawnPoints();
    }
    public override void OnStartServer()
    {
        serverRoundManager = GetComponent<ServerRoundManager>();
        serverScoreboardManager = GetComponent<ServerScoreboardManager>();
    }
    public void SetMap(string map)
    {
        this.map = map;
    }
    private void SpawnMap()
    {
        GameObject loadedMap = Resources.Load<GameObject>("Maps/" + map);
        if (loadedMap == null) SceneManager.LoadScene("Menu", LoadSceneMode.Single);

        GameObject spawnedMap = Instantiate(loadedMap);
        mapObject = spawnedMap;

        NetworkServer.Spawn(spawnedMap);
    }
    private void FindSpawnPoints()
    {
        for (int i = 0; i < mapObject.transform.childCount; i++)
        {
            GameObject child = mapObject.transform.GetChild(i).gameObject;
            if (child.name == "red_spawn_point")
            {
                redSpawnPoints.Add(child);
                //child.GetComponent<SpriteRenderer>().enabled = false;
            }
            else if (child.name == "blue_spawn_point")
            {
                blueSpawnPoints.Add(child);
                //child.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }

    [Server]
    public void SpawnPlayer(GameObject oldPlayer, Vector2 pos, Teams team, string nickName, NetworkConnectionToClient client)
    {
        GameObject newPlayer;
        Vector3 spawnPos;

        if(pos == Vector2.zero)
        {
            if (team == Teams.Blue)
                spawnPos = blueSpawnPoints[Random.Range(0, blueSpawnPoints.Count)].transform.position;
            else
                spawnPos = redSpawnPoints[Random.Range(0, redSpawnPoints.Count)].transform.position;

            newPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            newPlayer = Instantiate(playerPrefab, pos, Quaternion.identity);
        }

        if(oldPlayer != null)
        NetworkServer.Destroy(oldPlayer);

        newPlayer.GetComponent<PlayerStats>().SetVariables(nickName, team, 100);
        NetworkServer.Spawn(newPlayer);
        NetworkServer.ReplacePlayerForConnection(client, newPlayer, true);
    }
    [Server]
    public void SpawnAllPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("PlayerInfoDisplay");

        List<GameObject> blueSpawns = new List<GameObject>();
        List<GameObject> redSpawns = new List<GameObject>();
        blueSpawns.AddRange(blueSpawnPoints);
        redSpawns.AddRange(redSpawnPoints);

        for (int i = 0; i < players.Length; i++)
        {
            Scoreboard scoreboard = players[i].GetComponent<Scoreboard>();

            if (scoreboard.team == Teams.Blue)
            {
                int random = Random.Range(0, blueSpawns.Count);
                SpawnPlayer(null, blueSpawns[random].transform.position,
                scoreboard.team, scoreboard.nickName, players[i].GetComponent<NetworkIdentity>().connectionToClient);

                blueSpawns.RemoveAt(random);
            }
            if (scoreboard.team == Teams.Red)
            {
                int random = Random.Range(0, redSpawns.Count);
                SpawnPlayer(null, redSpawns[random].transform.position,
                scoreboard.team, scoreboard.nickName, players[i].GetComponent<NetworkIdentity>().connectionToClient);

                redSpawns.RemoveAt(random);
            }
        }
    }
    [Server]
    public void SpawnInfoDisplay(Teams team, string nickName, NetworkConnectionToClient client)
    {
        GameObject infoDisplay = Instantiate(infoDisplayPrefab);
        infoDisplay.GetComponent<Scoreboard>().SetVariables(nickName, team);
        NetworkServer.Spawn(infoDisplay, client);
    }
    [Server]
    public void SendDeathNoticeToAllPlayers(string killer, Teams killerTeam, string weaponName, string killed, Teams killedTeam)
    {
        GameObject[] infoDisplays = GameObject.FindGameObjectsWithTag("PlayerInfoDisplay");

        for (int i = 0; i < infoDisplays.Length; i++) 
        {
            infoDisplays[i].GetComponent<DeathNotice>().TargetAddNotice(killer, killerTeam, weaponName, killed, killedTeam);
        }
    }
    [Server]
    public void StartGame()
    {
        if (serverRoundManager.gameStage != GameStage.Warmup) return;
        GameObject[] connectedPlayers = GameObject.FindGameObjectsWithTag("PlayerInfoDisplay");
        if (connectedPlayers.Length < 2) return;

        serverRoundManager.NewRound(true);
    }
}
