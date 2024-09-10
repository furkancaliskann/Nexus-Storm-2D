using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Market : NetworkBehaviour
{
    private Inventory inventory;
    private ItemList itemList;
    private LockMovement lockMovement;
    private Money money;
    private PlayerStats playerStats;
    private ServerScoreboardManager serverScoreboardManager;
    private Weapon weapon;

    public List<Item> marketItems = new List<Item>();
    public List<GameObject> marketItemObjects = new List<GameObject>();
    [SerializeField] private GameObject marketItemPrefab;
    [SerializeField] private GameObject marketPanel;
    [SerializeField] private Transform primaryParent;
    [SerializeField] private Transform secondaryParent;
    [SerializeField] private Transform grenadeParent;

    [SerializeField] private AudioSource inventoryAudioSource;
    [SerializeField] private AudioClip buySound;

    public override void OnStartServer()
    {
        inventory = GetComponent<Inventory>();
        itemList = GameObject.FindGameObjectWithTag("ServerManager").GetComponent<ItemList>();
        playerStats = GetComponent<PlayerStats>();
        serverScoreboardManager = itemList.GetComponent<ServerScoreboardManager>();
        weapon = GetComponent<Weapon>();
        CheckMarketItems();
    }
    public override void OnStartLocalPlayer()
    {
        lockMovement = GetComponent<LockMovement>();
        CmdMarketItemsRequest();
    }
    public override void OnStartClient()
    {
        money = GetComponent<Money>();
    }
    private void Update()
    {
        if (!isLocalPlayer) return;
        CheckInputs();
    }
    private void CheckInputs()
    {
        if(Input.GetKeyDown(KeyCode.B))
        {
            if (marketPanel.activeInHierarchy) ClosePanel();
            else OpenPanel();
        }
    }
    private void OpenPanel()
    {
        if (lockMovement.lockType != LockTypes.None) return;

        marketPanel.SetActive(true);
        lockMovement.Lock(LockTypes.All);
    }
    private void ClosePanel()
    {
        marketPanel.SetActive(false);
        lockMovement.Unlock();
    }
    [Command]
    private void CmdBuyItem(string nickName)
    {
        if (weapon.reloading) return;
        Item item = marketItems.Find(x => x.nickName == nickName);
        if (item == null) return;
        if (money.money < item.marketPrice) return;
        serverScoreboardManager.DecreaseMoney(playerStats.nickName, playerStats.team, item.marketPrice);
        inventory.ServerGetItem(nickName);
        serverScoreboardManager.SetPlayerItem(playerStats.nickName, item.category, item);
        RpcPlayBuySound();
    }

    [Command]
    private void CmdMarketItemsRequest()
    {
        TargetSetMarketItems(marketItems);
    }

    [Server]
    private void CheckMarketItems()
    {
        marketItems = itemList.ReturnMarketItems();
    }

    [TargetRpc]
    private void TargetSetMarketItems(List<Item> marketItems)
    {
        this.marketItems = marketItems;
        for (int i = 0; i < marketItems.Count; i++)
        {
            Transform parent = null;
            if (marketItems[i].category == ItemCategories.PrimaryWeapon) parent = primaryParent;
            else if (marketItems[i].category == ItemCategories.SecondaryWeapon) parent = secondaryParent;
            else if (marketItems[i].category == ItemCategories.Grenade || marketItems[i].category == ItemCategories.Flash ||
                marketItems[i].category == ItemCategories.Smoke) parent = grenadeParent;

            GameObject marketItem = Instantiate(marketItemPrefab, parent);
            marketItem.GetComponentsInChildren<Image>()[1].sprite = Resources.Load<Sprite>("Item Images/" + marketItems[i].nickName);
            marketItem.GetComponentInChildren<Text>().text = marketItems[i].marketPrice + " $";
            string referanceName = marketItems[i].nickName;
            marketItem.GetComponent<Button>().onClick.AddListener(() => { CmdBuyItem(referanceName); });
            marketItemObjects.Add(marketItem);
        }
        
    }

    [ClientRpc]
    private void RpcPlayBuySound()
    {
        inventoryAudioSource.PlayOneShot(buySound);
    }
}
