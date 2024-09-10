using Mirror;
using UnityEngine;

public class Death : NetworkBehaviour
{
    [SerializeField] private GameObject deadPlayerPrefab;

    [Server]
    public void StartDeathProcess(Teams team)
    {
        NetworkServer.Destroy(gameObject);

        GameObject deadPlayerObject = Instantiate(deadPlayerPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(deadPlayerObject);
        NetworkServer.ReplacePlayerForConnection(GetComponent<NetworkIdentity>().connectionToClient, deadPlayerObject, true);

        DeadPlayer deadPlayer = deadPlayerObject.GetComponent<DeadPlayer>();
        deadPlayer.SetVariable(team);
        deadPlayer.RpcPlayDeadSound();
        
    }
}
