using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : NetworkBehaviour
{
    private Death death;
    private ServerManager serverManager;
    private ServerScoreboardManager serverScoreboardManager;

    [SyncVar]
    public string nickName;
    [SyncVar(hook = nameof(UpdateHealthText))]
    public int health;
    [SyncVar]
    public Teams team;

    [SerializeField] private Text healthText;

    public void SetVariables(string nickName, Teams team, int health)
    {
        this.nickName = nickName;
        this.team = team;
        this.health = health;
    }
    public override void OnStartServer()
    {
        death = GetComponent<Death>();
        serverManager = GameObject.FindGameObjectWithTag("ServerManager").GetComponent<ServerManager>();
        serverScoreboardManager = serverManager.GetComponent<ServerScoreboardManager>();
    }

    private void UpdateHealthText(int oldHealth, int newHealth)
    {
        healthText.text = newHealth.ToString();
    }

    [Server]
    public void DecreaseHealth(int amount, string attacker, Teams attackerTeam, string weaponName)
    {
        if(health <= amount)
            serverScoreboardManager.AddDamage(attacker, health, attackerTeam, team);
        else
            serverScoreboardManager.AddDamage(attacker, amount, attackerTeam, team);

        health -= amount;

        if(health <= 0)
        {
            health = 0;
            serverScoreboardManager.AddKillPoint(attacker, attackerTeam, nickName, team);
            death.StartDeathProcess(team);
            serverManager.SendDeathNoticeToAllPlayers(attacker, attackerTeam, weaponName, nickName, team);
        }
    } 
}
