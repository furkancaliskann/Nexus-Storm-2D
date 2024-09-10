using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject networkInfoPrefab;
    [SerializeField] private InputField ipAddressInputfield;

    void Awake()
    {
        DestroyNetworkManagers();
    }
    public void ChangeScene(bool isHost)
    {
        if (ipAddressInputfield.text == "") return;

        GameObject networkInfo = Instantiate(networkInfoPrefab);
        networkInfo.GetComponent<NetworkInfo>().SetVariables(isHost, ipAddressInputfield.text);
        DontDestroyOnLoad(networkInfo);
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }
    public void Quit()
    {
        Application.Quit();
    }
    private void DestroyNetworkManagers()
    {
        GameObject[] networkManagers = GameObject.FindGameObjectsWithTag("NetworkManager");

        foreach (GameObject networkManager in networkManagers)
        {
            Destroy(networkManager);
        }
    }
}
