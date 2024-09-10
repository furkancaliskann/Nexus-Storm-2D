using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DeadPlayer : NetworkBehaviour
{
    [SyncVar]
    public Teams team;
    private Transform spectatingPlayer;
    private int spectatingId;
    [SerializeField] Text spectatingPlayerText;

    [SerializeField] private GameObject canvas;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip deadSound;

    public void SetVariable(Teams team)
    {
        this.team = team;
    }
    public override void OnStartLocalPlayer()
    {
        canvas.SetActive(true);
    }
    void Update()
    {
        if (!isLocalPlayer) return;
        CheckSpectatingPlayerHealth();
    }
    void LateUpdate()
    {
        if (!isLocalPlayer || spectatingPlayer == null) return;
        Camera.main.transform.position = new Vector3(spectatingPlayer.position.x, spectatingPlayer.position.y, -10);
    }
    private void CheckSpectatingPlayerHealth()
    {
        if (spectatingPlayer != null || IsInvoking(nameof(SetSpectatePlayer))) return;

        spectatingPlayerText.text = "";
        Invoke(nameof(SetSpectatePlayer), 3f);
    }

    [ClientRpc]
    public void RpcPlayDeadSound()
    {
        audioSource.PlayOneShot(deadSound);
    }
    public void SetSpectatePlayer()
    {
        if (IsInvoking(nameof(SetSpectatePlayer))) CancelInvoke(nameof(SetSpectatePlayer));

        if (spectatingPlayer != null)
        {
            spectatingPlayer.GetComponent<Player>().OpenClosePlayerSprite(false);
            spectatingPlayer.GetComponent<FieldOfView>().SetSpactating(false);
        }

        List<GameObject> players = GameObject.FindGameObjectsWithTag("Player").Where(x=> x.GetComponent<PlayerStats>().team == team).ToList();
        if (players.Count <= 0) return;

        if (players.Count > spectatingId + 1)
        {
            spectatingPlayer = players[spectatingId + 1].transform;
            spectatingId = spectatingId + 1;
        }
            
        else
        {
            spectatingPlayer = players[0].transform;
            spectatingId = 0;
        }

        spectatingPlayer.GetComponent<Player>().OpenClosePlayerSprite(true);
        spectatingPlayer.GetComponent<FieldOfView>().SetSpactating(true);
        spectatingPlayerText.text = "-Spectating-\n\n" + spectatingPlayer.GetComponent<PlayerStats>().nickName;
    }
}
