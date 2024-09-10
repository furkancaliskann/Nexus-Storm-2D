using Mirror;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class Walkie : NetworkBehaviour
{
    private LockMovement lockMovement;
    private PlayerStats playerStats;

    [SerializeField] private AudioSource walkieAudioSource;
    [SerializeField] private AudioClip[] zSounds;
    [SerializeField] private AudioClip[] xSounds;
    [SerializeField] private AudioClip[] cSounds;

    [SerializeField] private GameObject zPanel;
    [SerializeField] private GameObject xPanel;
    [SerializeField] private GameObject cPanel;

    private GameObject currentPanel;

    [SerializeField] private Transform walkiePlayersParent;
    [SerializeField] private GameObject walkiePlayerPrefab;
    private List<GameObject> walkiePlayers = new List<GameObject>();
    private int maxWalkiePlayersLength = 5;

    private bool canPlayAudio;

    public override void OnStartServer()
    {
        playerStats = GetComponent<PlayerStats>();
        canPlayAudio = true;
    }
    public override void OnStartLocalPlayer()
    {
        lockMovement = GetComponent<LockMovement>();
    }
    void Update()
    {
        if (!isLocalPlayer) return;
        CheckInputs();
    }
    private void CheckInputs()
    {
        if (Input.GetKeyDown(KeyCode.Z)) OpenPanel(zPanel);
        if (Input.GetKeyDown(KeyCode.X)) OpenPanel(xPanel);
        if (Input.GetKeyDown(KeyCode.C)) OpenPanel(cPanel);
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlayAudio(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) PlayAudio(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) PlayAudio(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) PlayAudio(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) PlayAudio(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) PlayAudio(5);
    }
    private void OpenPanel(GameObject panel)
    {
        if (currentPanel != null)
        {
            if (currentPanel == panel)
            {
                ClosePanels();
                return;
            }
            else currentPanel.SetActive(false);
        }

        lockMovement.Lock(LockTypes.Input);
        currentPanel = panel;
        panel.SetActive(true);
    }
    private void ClosePanels()
    {
        zPanel.SetActive(false);
        xPanel.SetActive(false);
        cPanel.SetActive(false);

        currentPanel = null;
        lockMovement.Unlock();
    }
    private void PlayAudio(int index)
    {
        if (currentPanel == null) return;

        if (currentPanel == zPanel && zSounds.Length > index) CmdPlayAudio(0, index);
        else if (currentPanel == xPanel && xSounds.Length > index) CmdPlayAudio(1, index);
        else if (currentPanel == cPanel && cSounds.Length > index) CmdPlayAudio(2, index);

        ClosePanels();
    }

    [Command]
    private void CmdPlayAudio(int soundsIndex, int clipIndex)
    {
        if (!canPlayAudio) return;
        canPlayAudio = false;
        Teams sendTeam = playerStats.team;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if(player.GetComponent<PlayerStats>().team == sendTeam)
            {
                player.GetComponent<Walkie>().TargetPlayAudio(soundsIndex, clipIndex, playerStats.nickName);
            }
        }

        Invoke(nameof(SetCanPlayAudio), zSounds[clipIndex].length + 0.2f);
    }
    [TargetRpc]
    private void TargetPlayAudio(int soundsIndex, int clipIndex, string sendedPlayer)
    {
        switch (soundsIndex)
        {
            case 0: walkieAudioSource.PlayOneShot(zSounds[clipIndex]); AddWalkiePlayer(sendedPlayer, zSounds[clipIndex].length + 0.2f); break;
            case 1: walkieAudioSource.PlayOneShot(xSounds[clipIndex]); AddWalkiePlayer(sendedPlayer, zSounds[clipIndex].length + 0.2f); break;
            case 2: walkieAudioSource.PlayOneShot(cSounds[clipIndex]); AddWalkiePlayer(sendedPlayer, zSounds[clipIndex].length + 0.2f); break;
        }

    }
    private void AddWalkiePlayer(string sendedPlayer, float destroyTime)
    {
        CheckWalkiePlayerList();
        GameObject walkiePlayer = Instantiate(walkiePlayerPrefab, walkiePlayersParent);
        walkiePlayer.GetComponent<Text>().text = "*Walkie - Talkie (" + sendedPlayer + ")";
        Destroy(walkiePlayer, destroyTime);
        walkiePlayers.Add(walkiePlayer);
    }
    private void CheckWalkiePlayerList()
    {
        for (int i = 0; i < walkiePlayers.Count; i++)
        {
            if (walkiePlayers[i] == null) walkiePlayers.RemoveAt(i);
        }

        if(walkiePlayers.Count >= maxWalkiePlayersLength)
        {
            Destroy(walkiePlayers[0]);
            walkiePlayers.RemoveAt(0);
        }
    }
    private void SetCanPlayAudio()
    {
        canPlayAudio = true;
    }
}
