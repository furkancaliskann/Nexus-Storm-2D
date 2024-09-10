using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chat : NetworkBehaviour
{
    private ChatVariables chatVariables;
    [HideInInspector] public LockMovement lockMovement;
    private Scoreboard scoreboard;

    private Image[] panelBackgrounds;

    private Text chatText;
    private InputField chatInput;
    private Scrollbar chatScrollbar;

    private List<string> cachedInputs = new List<string>();
    private int selectedInputNumber;
    private int inputCacheLength = 10;

    private int maxLineSize = 100;
    private bool resetScrollbar;

    private float clearChatTime = 60f;
    private float clearChatTimeMax = 60f;

    public override void OnStartServer()
    {
        scoreboard = GetComponent<Scoreboard>();
    }
    public override void OnStartAuthority()
    {
        chatVariables = Camera.main.GetComponent<ChatVariables>();

        panelBackgrounds = chatVariables.panelBackgrounds;
        chatText = chatVariables.chatText;
        chatInput = chatVariables.chatInput;
        chatScrollbar = chatVariables.chatScrollbar;

    }
    void Update()
    {
        if (!isOwned) return;

        CheckInputs();
        CheckClearChatTime();
        ResetScrollbarValue();
    }
    private void CheckInputs()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (panelBackgrounds[0].enabled && chatInput.text == "") ClosePanel();
            else if (!panelBackgrounds[0].enabled) OpenPanel();
            else
            {
                SendMessage();
                ClosePanel();
            }       
        }

        if(Input.GetKeyDown(KeyCode.UpArrow) && panelBackgrounds[0].enabled)
        {
            SelectPreviousInput();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) && panelBackgrounds[0].enabled)
        {
            SelectNextInput();
        }
    }
    private void CheckClearChatTime()
    {
        if (chatText.text != "" && clearChatTime > 0)
            clearChatTime -= Time.deltaTime;

        if(clearChatTime <= 0)
        {
            ClearChat();
            clearChatTime = clearChatTimeMax;
        }
    }
    private void OpenPanel()
    {
        ClearInput();

        for (int i = 0; i < panelBackgrounds.Length; i++)
        {
            panelBackgrounds[i].enabled = true;
        }
        chatInput.gameObject.SetActive(true);
        lockMovement?.Lock(LockTypes.All);
        ActivateInput();
    }
    private void ClosePanel()
    {
        if (!panelBackgrounds[0].enabled) return;

        for (int i = 0; i < panelBackgrounds.Length; i++)
        {
            panelBackgrounds[i].enabled = false;
        }
        chatInput.gameObject.SetActive(false);
        lockMovement?.Unlock();
    }
    private void SendMessage()
    {
        if (!panelBackgrounds[0].enabled || chatInput.text == "") return;

        AddPreviousInput(chatInput.text);
        CmdSendMessage(chatInput.text);

        ClearInput();
        ActivateInput();
    }

    [Command]
    private void CmdSendMessage(string message)
    {
        if (message == "") return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("PlayerInfoDisplay");

        for (int i = 0;i < players.Length;i++)
        {
            players[i].GetComponent<Chat>().TargetSendMessage(scoreboard.nickName + " : " + message);
        }
    }

    [TargetRpc]
    private void TargetSendMessage(string message)
    {
        clearChatTime = clearChatTimeMax;
        WriteChat(message);
    }
    private void AddPreviousInput(string input)
    {
        cachedInputs.Add(input);

        if(cachedInputs.Count > inputCacheLength)
        {
            int deleteCount = cachedInputs.Count - inputCacheLength;

            for (int i = 0; i < deleteCount; i++)
            {
                cachedInputs.RemoveAt(0);
            }
        }

        selectedInputNumber = cachedInputs.Count;
    }
    private void SelectPreviousInput()
    {
        if (cachedInputs.Count <= 0 || selectedInputNumber < 0) return; 

        if(selectedInputNumber >= 1)
            selectedInputNumber--;

        chatInput.text = cachedInputs[selectedInputNumber];
        chatInput.MoveTextEnd(false);
    }
    private void SelectNextInput()
    {
        if (selectedInputNumber >= cachedInputs.Count) return;

        if (selectedInputNumber < cachedInputs.Count - 1)
            selectedInputNumber++;

        chatInput.text = cachedInputs[selectedInputNumber];        
        chatInput.MoveTextEnd(false);
    }
    private void ClearInput()
    {
        chatInput.text = string.Empty;
    }
    private void ActivateInput()
    {
        chatInput.ActivateInputField();
    }
    private void WriteChat(string text)
    {
        chatText.text += text + "\n";
        CheckLineSize();
        resetScrollbar = true;

        LayoutRebuilder.ForceRebuildLayoutImmediate(chatText.GetComponent<RectTransform>());
    }
    private void ClearChat()
    {
        chatText.text = "";
    }
    private void CheckLineSize()
    {
        string[] lines = chatText.text.Split('\n');

        if (lines.Length > maxLineSize)
        {
            ClearChat();
            for (int i = lines.Length - maxLineSize; i < lines.Length; i++)
            {
                if (lines[i] != "")
                    WriteChat(lines[i]);
            }
        }
    }
    private void ResetScrollbarValue()
    {
        if (!resetScrollbar || chatScrollbar.value == 0) return;

        chatScrollbar.value = 0;
        if(chatScrollbar.value == 0) resetScrollbar = false;
    }
}
