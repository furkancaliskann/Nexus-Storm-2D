using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private PlayerStats playerStats;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private GameObject canvasObject;

    [SerializeField] private AudioSource inventoryAudioSource;
    [SerializeField] private AudioClip spawnSound;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        SetPlayerColor();
    }
    public override void OnStartLocalPlayer()
    {
        canvasObject.SetActive(true);
        gameObject.layer = 0;
        OpenClosePlayerSprite(true);
    }
    public override void OnStartClient()
    {
        if (isServer) RpcPlaySpawnSound();
    }
    private void SetPlayerColor()
    {
        if(playerStats.team == Teams.Blue) spriteRenderer.color = Color.blue;
        else if (playerStats.team == Teams.Red) spriteRenderer.color = Color.red;
    }

    [ClientRpc]
    private void RpcPlaySpawnSound()
    {
        inventoryAudioSource.PlayOneShot(spawnSound);
    }

    public void OpenClosePlayerSprite(bool value)
    {
        GetComponent<SpriteRenderer>().enabled = value;

        if (value) gameObject.layer = 0;
        else gameObject.layer = 11;
    }
}
