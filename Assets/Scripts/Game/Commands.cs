using Mirror;
using UnityEngine;

public class Commands : NetworkBehaviour
{
    private CommandsVariables commandsVariables;
    private ServerManager serverManager;

    private GameObject openPanelButton;
    private GameObject commandsPanel;

    public override void OnStartServer()
    {
        commandsVariables = Camera.main.GetComponent<CommandsVariables>();
        serverManager = GameObject.FindGameObjectWithTag("ServerManager").GetComponent<ServerManager>();
        commandsVariables.commands = this;
        openPanelButton = commandsVariables.openPanelButton;
        commandsPanel = commandsVariables.commandsPanel;
        openPanelButton.SetActive(true);
    }

    public void OpenCommandsPanel()
    {
        openPanelButton.SetActive(false);
        commandsPanel.SetActive(true);
    }

    public void CloseCommandsPanel()
    {
        commandsPanel.SetActive(false);
        openPanelButton.SetActive(true);
    }

    public void StartGameRequest()
    {
        serverManager.StartGame();
    }

}
