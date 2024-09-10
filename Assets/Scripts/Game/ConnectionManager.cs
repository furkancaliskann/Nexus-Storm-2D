using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviour
{
    private NetworkManager networkManager;

    [SerializeField] private GameObject serverManagerPrefab;

    void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
    }
    void Start()
    {
        GameObject networkInfoObject = GameObject.FindGameObjectWithTag("NetworkInfo");
        if (networkInfoObject == null)
        {
            SceneManager.LoadScene("Menu", LoadSceneMode.Single);
            return;
        }

        NetworkInfo networkInfo = networkInfoObject.GetComponent<NetworkInfo>();

        if (networkInfo.isHost)
        {
            
            networkManager.StartHost();
            GameObject serverManager = Instantiate(serverManagerPrefab);
            serverManager.GetComponent<ServerManager>().SetMap(networkInfo.map);
            NetworkServer.Spawn(serverManager);
        }
        else
        {
            networkManager.networkAddress = networkInfo.ipAddress;
            networkManager.StartClient(); 
        }

        Destroy(networkInfoObject);
    }
}
