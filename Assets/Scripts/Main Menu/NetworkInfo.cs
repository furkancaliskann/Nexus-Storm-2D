using UnityEngine;

public class NetworkInfo : MonoBehaviour
{
    public bool isHost {  get; private set; }
    public string ipAddress { get; private set; }
    public string map {  get; private set; }

    public void SetVariables(bool isHost, string ipAddress)
    {
        this.isHost = isHost;
        this.ipAddress = ipAddress;
        map = "dust2";
    }
}
