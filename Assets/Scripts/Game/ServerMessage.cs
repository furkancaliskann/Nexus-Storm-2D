using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class ServerMessage : NetworkBehaviour
{
    private ServerMessageVariables serverMessageVariables;

    private Text messageText;

    public override void OnStartClient()
    {
        serverMessageVariables = Camera.main.GetComponent<ServerMessageVariables>();
        messageText = serverMessageVariables.messageText;
    }

    [TargetRpc]
    public void TargetSendMessage(string message)
    {
        if (message == "") messageText.text = "";
        else
            messageText.text = "Server : " + message;
    }
}
