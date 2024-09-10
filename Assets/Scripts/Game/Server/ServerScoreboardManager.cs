using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;

public class ServerScoreboardManager : NetworkBehaviour
{
    private ItemList itemList;
    private ServerRoundManager serverRoundManager;

    public List<ScoreboardPlayer> blueTeamPlayers = new List<ScoreboardPlayer>();
    public List<ScoreboardPlayer> redTeamPlayers = new List<ScoreboardPlayer>();

    private int startingMoney = 800;
    private int winnerTeamRoundMoney = 2500;
    private int loserTeamRoundMoney = 1750;
    private int maxMoney = 15000;

    public override void OnStartServer()
    {
        itemList = GetComponent<ItemList>();
        serverRoundManager = GetComponent<ServerRoundManager>();
    }
    public bool IsNameUsable(string nickName)
    {
        for (int i = 0; i < blueTeamPlayers.Count; i++)
        {
            if (blueTeamPlayers[i].nickName == nickName) return false;
        }
        for (int i = 0; i < redTeamPlayers.Count; i++)
        {
            if (redTeamPlayers[i].nickName == nickName) return false;
        }
        return true;
    }
    public void AddNewPlayer(string nickName, Teams team)
    {
        ScoreboardPlayer player = new ScoreboardPlayer();
        player.nickName = nickName;
        player.money = 800;

        if (team == Teams.Blue) blueTeamPlayers.Add(player);
        else if (team == Teams.Red) redTeamPlayers.Add(player);

        ResetPlayerItems(nickName);
        SendScoreToAllPlayers(false);
        SendMoney(player.nickName, true, 0, player.money);
    }
    public void RefreshRanks()
    {
        blueTeamPlayers = blueTeamPlayers.OrderByDescending(x => x.damage)
                                 .ThenByDescending(x => x.kill)
                                 .ToList();

        redTeamPlayers = redTeamPlayers.OrderByDescending(x => x.damage)
                                 .ThenByDescending(x => x.kill)
                                 .ToList();
    }
    public void RefreshPing(string nickName, Teams team, int ping)
    {
        if(team == Teams.Blue)
        {
            var player = blueTeamPlayers.Find(x => x.nickName == nickName);
            if(player != null) player.ping = ping;
        }
        else if (team == Teams.Red)
        {
            var player = redTeamPlayers.Find(x => x.nickName == nickName);
            if (player != null) player.ping = ping;
        }

        SendScoreToAllPlayers(false);
    }
    public void RemovePlayer(string nickName, Teams team)
    {
        if (team == Teams.Blue) blueTeamPlayers.RemoveAt(blueTeamPlayers.FindIndex(x => x.nickName == nickName));
        else if (team == Teams.Red) redTeamPlayers.RemoveAt(redTeamPlayers.FindIndex(x => x.nickName == nickName));

        SendScoreToAllPlayers(false);
    }
    public void AddKillPoint(string killer, Teams killerTeam, string killed, Teams killedTeam)
    {
        int score = killerTeam == killedTeam ? -1 : 1;
        int money = killerTeam == killedTeam ? -500 : 500;

        if (killerTeam == Teams.Blue)
        {
            var killerPlayer = blueTeamPlayers.Find(x => x.nickName == killer);
            if (killerPlayer.nickName != "")
            {
                killerPlayer.kill += score;
                int oldMoney = killerPlayer.money;
                killerPlayer.money += money;
                if(killerPlayer.money > maxMoney) killerPlayer.money = maxMoney;
                SendMoney(killer, true, oldMoney, killerPlayer.money);
            }
        }
            
        else if (killerTeam == Teams.Red)
        {
            var killerPlayer = redTeamPlayers.Find(x => x.nickName == killer);
            if (killerPlayer.nickName != "")
            {
                killerPlayer.kill += score;
                int oldMoney = killerPlayer.money;
                killerPlayer.money += money;
                if (killerPlayer.money > maxMoney) killerPlayer.money = maxMoney;
                SendMoney(killer, true, oldMoney, killerPlayer.money);
            }
        }

        if (killedTeam == Teams.Blue)
        {
            var killedPlayer = blueTeamPlayers.Find(x => x.nickName == killed);
            if (killedPlayer.nickName != "") killedPlayer.death++;

        }
        else if (killedTeam == Teams.Red)
        {
            var killedPlayer = redTeamPlayers.Find(x => x.nickName == killed);
            if (killedPlayer.nickName != "") killedPlayer.death++;
        }

        ResetPlayerItems(killed);
        SendScoreToAllPlayers(false);
        StartCoroutine(serverRoundManager.CheckRound());
    }
    public void AddDamage(string playerNickName, int damage, Teams playerTeam, Teams damagedPlayerTeam)
    {
        if (playerTeam == damagedPlayerTeam) return;

        if (playerTeam == Teams.Blue)
        {
            var player = blueTeamPlayers.Find(x => x.nickName == playerNickName);
            player.damage += damage;
        }
        else if (playerTeam == Teams.Red)
        {
            var player = redTeamPlayers.Find(x => x.nickName == playerNickName);
            player.damage += damage;
        }

        SendScoreToAllPlayers(false);
    }
    public void SendScoreToAllPlayers(bool gameFinished)
    {
        RefreshRanks();

        List<GameObject> players = GameObject.FindGameObjectsWithTag("PlayerInfoDisplay").ToList();

        for (int i = 0; i < players.Count; i++)
        {
            Scoreboard scoreboard = players[i].GetComponent<Scoreboard>();

            if (!gameFinished)
            scoreboard.TargetUpdateScores(blueTeamPlayers, redTeamPlayers, serverRoundManager.blueTeamWinCount,
                serverRoundManager.redTeamWinCount);
            else
            {
                scoreboard.TargetScoreSceneWithScores();
            }  
        }
    }
    public void ResetScores()
    {
        for (int i = 0; i < blueTeamPlayers.Count; i++)
        {
            blueTeamPlayers[i].kill = 0;
            blueTeamPlayers[i].death = 0;
            blueTeamPlayers[i].damage = 0;
        }

        for (int i = 0; i < redTeamPlayers.Count; i++)
        {
            redTeamPlayers[i].kill = 0;
            redTeamPlayers[i].death = 0;
            redTeamPlayers[i].damage = 0;
        }

        SendScoreToAllPlayers(false);
    }
    public ScoreboardPlayer FindPlayerScore(string nickName)
    {
        for (int i = 0; i < blueTeamPlayers.Count; i++)
        {
            if (blueTeamPlayers[i].nickName == nickName) return blueTeamPlayers[i];
        }

        for (int i = 0; i < redTeamPlayers.Count; i++)
        {
            if (redTeamPlayers[i].nickName == nickName) return redTeamPlayers[i];
        }

        return null;
    }
    private Teams FindPlayerTeam(string nickName)
    {
        for (int i = 0; i < blueTeamPlayers.Count; i++)
        {
            if (blueTeamPlayers[i].nickName == nickName) return Teams.Blue;
        }

        for (int i = 0; i < redTeamPlayers.Count; i++)
        {
            if (redTeamPlayers[i].nickName == nickName) return Teams.Red;
        }

        return Teams.Spectator;
    }
    private void SendMoney(string nickName, bool moneyChanged, int oldMoney, int newMoney)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponent<PlayerStats>().nickName == nickName)
            {
                players[i].GetComponent<Money>().UpdateMoney(moneyChanged, oldMoney, newMoney);
                return;
            }
        }
    }
    public void GiveStartingMoneyToAllPlayers()
    {
        for (int i = 0; i < blueTeamPlayers.Count; i++)
        {
            blueTeamPlayers[i].money = startingMoney;
        }

        for (int i = 0; i < redTeamPlayers.Count; i++)
        {
            redTeamPlayers[i].money = startingMoney;
        }

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < players.Length; i++)
        {
            PlayerStats playerStats = players[i].GetComponent<PlayerStats>();
            var playerScore = FindPlayerScore(playerStats.nickName);
            players[i].GetComponent<Money>().UpdateMoney(true, 0, playerScore.money);
        }
    }
    public void DecreaseMoney(string nickName, Teams team, int value)
    {
        int oldMoney = 0;

        if (team == Teams.Blue)
        {
            var player = blueTeamPlayers.Find(x => x.nickName == nickName);
            oldMoney = player.money;
            player.money -= value;

            SendMoney(nickName, true, oldMoney, player.money);
        } 
            
        else if (team == Teams.Red)
        {
            var player = redTeamPlayers.Find(x => x.nickName == nickName);
            oldMoney = player.money;
            player.money -= value;

            SendMoney(nickName, true, oldMoney, player.money);
        }
            
            
    }
    public IEnumerator GiveRoundMoneyToAllPlayers(Teams winnerTeam)
    {
        yield return null;

        for (int i = 0; i < blueTeamPlayers.Count; i++)
        {
            int oldMoney = blueTeamPlayers[i].money;

            if (winnerTeam == Teams.Blue) blueTeamPlayers[i].money += winnerTeamRoundMoney;
            else blueTeamPlayers[i].money += loserTeamRoundMoney;

            if (blueTeamPlayers[i].money > maxMoney) blueTeamPlayers[i].money = maxMoney;

            SendMoney(blueTeamPlayers[i].nickName, true, oldMoney, blueTeamPlayers[i].money);
        }

        for (int i = 0; i < redTeamPlayers.Count; i++)
        {
            int oldMoney = redTeamPlayers[i].money;

            if (winnerTeam == Teams.Red) redTeamPlayers[i].money += winnerTeamRoundMoney;
            else redTeamPlayers[i].money += loserTeamRoundMoney;

            if (redTeamPlayers[i].money > maxMoney) redTeamPlayers[i].money = maxMoney;

            SendMoney(redTeamPlayers[i].nickName, true, oldMoney, redTeamPlayers[i].money);
        }
    }
    public void BalanceTeams()
    {
        if (blueTeamPlayers.Count == redTeamPlayers.Count || ((blueTeamPlayers.Count == redTeamPlayers.Count - 1 ||
            blueTeamPlayers.Count == redTeamPlayers.Count + 1) && blueTeamPlayers.Count > 0 && redTeamPlayers.Count > 0)) return;

        int balancedPlayerCount;

        List<ScoreboardPlayer> bigTeam;
        List<ScoreboardPlayer> smallTeam;

        if (blueTeamPlayers.Count > redTeamPlayers.Count)
        {
            bigTeam = blueTeamPlayers;
            smallTeam = redTeamPlayers;
            balancedPlayerCount = (blueTeamPlayers.Count - redTeamPlayers.Count) / 2;
        }
        else
        {
            bigTeam = redTeamPlayers;
            smallTeam = blueTeamPlayers;
            balancedPlayerCount = (redTeamPlayers.Count - blueTeamPlayers.Count) / 2;
        }

        for (int i = 0; i < balancedPlayerCount; i++)
        {
            ScoreboardPlayer player = bigTeam[UnityEngine.Random.Range(0, bigTeam.Count)];
            bigTeam.Remove(player);
            smallTeam.Add(player);
        }

        GameObject[] infoDisplays = GameObject.FindGameObjectsWithTag("PlayerInfoDisplay");

        for (int i = 0; i < infoDisplays.Length; i++)
        {
            Scoreboard scoreBoard = infoDisplays[i].GetComponent<Scoreboard>();
            scoreBoard.team = FindPlayerTeam(scoreBoard.nickName);
        }
    }
    public void SetPlayerItem(string nickName, ItemCategories category, Item item)
    {
        ScoreboardPlayer player = FindPlayerScore(nickName);
        if (player == null) return;

        switch (category)
        {
            case ItemCategories.PrimaryWeapon: player.primary = item; break;
            case ItemCategories.SecondaryWeapon: player.secondary = item; break;
            case ItemCategories.Knife: player.knife = item; break;
            case ItemCategories.Grenade: player.grenade = item; break;
            case ItemCategories.Flash: player.flash = item; break;
            case ItemCategories.Smoke: player.smoke = item; break;
            case ItemCategories.C4: player.c4 = item; break;
        }
    }
    public void ResetPlayerItems(string nickName)
    {
        ScoreboardPlayer player = FindPlayerScore(nickName);
        if (player == null) return;

        player.primary = null;
        player.secondary = itemList.ReturnNewItem("usp");
        player.knife = itemList.ReturnNewItem("knife");
        player.grenade = itemList.ReturnNewItem("grenade");
        player.flash = itemList.ReturnNewItem("flash");
        player.smoke = itemList.ReturnNewItem("smoke");
        player.c4 = itemList.ReturnNewItem("c4");
    }
}

[Serializable]
public class ScoreboardPlayer
{
    public string nickName;
    public int ping;
    public int kill;
    public int death;
    public int damage;
    public int money;

    public Item primary;
    public Item secondary;
    public Item knife;
    public Item grenade;
    public Item flash;
    public Item smoke;
    public Item c4;
}
