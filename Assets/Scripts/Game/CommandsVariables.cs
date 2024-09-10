using UnityEngine;

public class CommandsVariables : MonoBehaviour
{
    public Commands commands;
    public GameObject openPanelButton;
    public GameObject commandsPanel;

    public void OpenCommandsPanel()
    {
        commands.OpenCommandsPanel();
    }
    public void CloseCommandsPanel()
    {
        commands.CloseCommandsPanel();
    }
    public void StartGameRequest()
    {
        commands.StartGameRequest();
    }
}
