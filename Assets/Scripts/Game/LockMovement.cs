using UnityEngine;

public class LockMovement : MonoBehaviour
{
    private PlayerStats playerStats;
    public LockTypes lockType;


    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }
    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("PlayerInfoDisplay");

        foreach (GameObject player in players)
        {
            if(playerStats.nickName == player.GetComponent<Scoreboard>().nickName)
            {
                player.GetComponent<Chat>().lockMovement = this;
                return;
            }
        }
    }

    public void Lock(LockTypes lockType)
    {
        this.lockType = lockType;
    }
    public void Unlock()
    {
        lockType = LockTypes.None;
    }
}

public enum LockTypes
{
    None,
    All,
    Movement,
    Input
}
