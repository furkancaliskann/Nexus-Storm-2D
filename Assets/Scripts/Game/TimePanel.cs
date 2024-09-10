using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class TimePanel : NetworkBehaviour
{
    private TimePanelVariables timePanelVariables;

    private GameObject timePanel;
    private Text timeText;

    public override void OnStartAuthority()
    {
        timePanelVariables = Camera.main.GetComponent<TimePanelVariables>();
        timePanel = timePanelVariables.timePanel;
        timeText = timePanelVariables.timeText;
    }

    [TargetRpc]
    public void TargetUpdateTime(int remainedTime)
    {
        if(!timePanel.activeInHierarchy) timePanel.SetActive(true);

        int minutes = remainedTime / 60;
        int seconds = remainedTime % 60;

        string formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds);

        timeText.text = formattedTime;
    }
}
