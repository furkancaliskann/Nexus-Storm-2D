using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : NetworkBehaviour
{
    private AmmoText ammoText;
    private ItemList itemList;
    private LockMovement lockMovement;
    private PlayerStats playerStats;
    private ServerScoreboardManager serverScoreboardManager;
    private Weapon weapon;

    [SyncVar] public Item primary;
    [SyncVar] public Item secondary;
    [SyncVar] public Item knife;
    [SyncVar] public Item grenade;
    [SyncVar] public Item flash;
    [SyncVar] public Item smoke;
    [SyncVar] public Item c4;

    [SyncVar] public Item selectedItem;

    [SerializeField] private RectTransform selectedBackground;
    private List<Sprite> itemImages = new List<Sprite>();
    [SerializeField] private Image primaryWeaponImage;
    [SerializeField] private Image secondaryWeaponImage;

    [SerializeField] private GameObject droppedItemPrefab;
    private GameObject currentCollision;
    private bool canGetItem;

    [SerializeField] private AudioClip getItemSound;
    [SerializeField] private AudioClip dropItemSound;
    [SerializeField] private AudioSource inventoryAudioSource;

    [SerializeField] private GameObject getItemPanel;
    [SerializeField] private Text getItemText;

    public override void OnStartServer()
    {
        ammoText = GetComponent<AmmoText>();
        itemList = GameObject.FindGameObjectWithTag("ServerManager").GetComponent<ItemList>();
        playerStats = GetComponent<PlayerStats>();
        serverScoreboardManager = GameObject.FindGameObjectWithTag("ServerManager").GetComponent<ServerScoreboardManager>();
    }
    public override void OnStartLocalPlayer()
    {
        itemImages = Camera.main.GetComponent<DeathNoticeVariables>().weaponImages;
        lockMovement = GetComponent<LockMovement>();

        CmdGetMyItems();
    }
    public override void OnStartClient()
    {
        weapon = GetComponent<Weapon>();
    }
    void Update()
    {
        if (!isLocalPlayer) return;
        CheckInputs();
    }
    private void CheckInputs()
    {
        if (weapon.reloading || lockMovement.lockType == LockTypes.All || lockMovement.lockType == LockTypes.Input) return;

        if(Input.GetKeyDown(KeyCode.Alpha1)) CmdSelectSlot(1);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) CmdSelectSlot(2);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) CmdSelectSlot(3);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) CmdSelectSlot(4);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) CmdSelectSlot(5);

        if (Input.GetKeyDown(KeyCode.G))
        {
            if(selectedItem.category != ItemCategories.Knife && selectedItem.category != ItemCategories.Empty) CmdDropItem();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (canGetItem) CmdGetItem();
        }
    }
    [Server]
    public void AddItem(string nickName)
    {
        Item item = itemList.ReturnNewItem(nickName);
        if (item == null) return;

        switch(item.category)
        {
            case ItemCategories.PrimaryWeapon: primary = item; break;
            case ItemCategories.SecondaryWeapon: secondary = item; break;
            case ItemCategories.Grenade: IncreaseAmount(grenade); break;
            case ItemCategories.Flash: IncreaseAmount(flash); break;
            case ItemCategories.Smoke: IncreaseAmount(smoke); break;
            case ItemCategories.C4: IncreaseAmount(c4); break;
            default: return;
        }

        TargetUpdateSlotImage(item.category, nickName);
    }
    [Server]
    private void AddItem(Item item)
    {
        if (item == null) return;

        switch (item.category)
        {
            case ItemCategories.PrimaryWeapon: primary = item; break;
            case ItemCategories.SecondaryWeapon: secondary = item; break;
            case ItemCategories.Knife: knife = item; break;
            case ItemCategories.Grenade: IncreaseAmount(grenade); break;
            case ItemCategories.Flash: IncreaseAmount(flash); break;
            case ItemCategories.Smoke: IncreaseAmount(smoke); break;
            case ItemCategories.C4: IncreaseAmount(c4); break;
            default: return;
        }

        TargetUpdateSlotImage(item.category, item.nickName);
    }
    [Command]
    private void CmdGetMyItems()
    {
        ScoreboardPlayer player = serverScoreboardManager.FindPlayerScore(playerStats.nickName);
        AddItem(player.primary);
        AddItem(player.secondary);
        AddItem(player.knife);
        AddItem(player.grenade);
        AddItem(player.flash);
        AddItem(player.smoke);
        AddItem(player.c4);

        ServerAutoSelectSlot();
    }
    [TargetRpc]
    private void TargetUpdateSlotImage(ItemCategories category, string nickName)
    {
        Image image = null;

        if (category == ItemCategories.PrimaryWeapon) image = primaryWeaponImage;
        else if (category == ItemCategories.SecondaryWeapon) image = secondaryWeaponImage;
        else return;

        var sprite = itemImages.Find(x => x.name == nickName);
        image.sprite = sprite;

        if (sprite != null && !image.enabled) image.enabled = true;
        if (sprite == null && image.enabled) image.enabled = false;
    }
    [TargetRpc]
    private void TargetClearSlotImage(ItemCategories category)
    {
        if (category == ItemCategories.PrimaryWeapon && primaryWeaponImage.enabled) primaryWeaponImage.enabled = false;
        else if (category == ItemCategories.SecondaryWeapon && secondaryWeaponImage.enabled) secondaryWeaponImage.enabled = false;
    }
    [Server]
    private void IncreaseAmount(Item item)
    {
        if (item.amount + 1 > item.maxAmount) item.amount = item.maxAmount;
        else item.amount++;

        SyncItem(item);
    }
    [Command]
    private void CmdSelectSlot(int slotNo)
    {
        ServerSelectSlot(slotNo);
    }
    [Server]
    private void ServerSelectSlot(int slotNo)
    {
        if (weapon.reloading) return;
        ItemCategories result = ItemCategories.Empty;

        switch (slotNo)
        {
            case 1: if (primary.nickName == "") return; selectedItem = primary; result = ItemCategories.PrimaryWeapon; break;

            case 2:
                if (secondary.nickName == "") return;
                selectedItem = secondary; 
                result = ItemCategories.SecondaryWeapon; break;

            case 3: selectedItem = knife; result = ItemCategories.Knife; break;

            case 4:
                if (selectedItem.nickName == grenade.nickName)
                {
                    if (flash.amount > 0) { selectedItem = flash; result = ItemCategories.Flash; break; }
                    else if (smoke.amount > 0) { selectedItem = smoke; result = ItemCategories.Smoke; break; }
                    else if (grenade.amount > 0) { selectedItem = grenade; result = ItemCategories.Grenade; break; }
                }
                else if (selectedItem.nickName == flash.nickName)
                {
                    if (smoke.amount > 0) { selectedItem = smoke; result = ItemCategories.Smoke; break; }
                    else if (grenade.amount > 0) { selectedItem = grenade; result = ItemCategories.Grenade; break; }
                    else if (flash.amount > 0) { selectedItem = flash; result = ItemCategories.Flash; break; }
                }
                else
                {
                    if (grenade.amount > 0) { selectedItem = grenade; result = ItemCategories.Grenade; break; }
                    else if (flash.amount > 0) { selectedItem = flash; result = ItemCategories.Flash; break; }
                    else if (smoke.amount > 0) { selectedItem = smoke; result = ItemCategories.Smoke; break; }
                }
                selectedItem = knife; result = ItemCategories.Knife; break;

            case 5: if (c4.amount == 0) return; selectedItem = c4; result = ItemCategories.C4; break;
        }

        SyncSelectedItem();
        TargetUpdateSelectedBackground(result);

        if ((result == ItemCategories.PrimaryWeapon || result == ItemCategories.SecondaryWeapon) && selectedItem.nickName != "")
            ammoText.TargetUpdatePanel(selectedItem.ammoInside, selectedItem.totalAmmo);
        else
            ammoText.TargetClosePanel();
    }
    [Server]
    private void ServerAutoSelectSlot()
    {
        if (primary.nickName != "") ServerSelectSlot(1);
        else if (secondary.nickName != "") ServerSelectSlot(2);
        else ServerSelectSlot(3);
    }
    [TargetRpc]
    private void TargetUpdateSelectedBackground(ItemCategories category)
    {
        switch (category)
        {
            case ItemCategories.PrimaryWeapon: selectedBackground.anchoredPosition = new Vector3(-50, 50, 0); break;
            case ItemCategories.SecondaryWeapon: selectedBackground.anchoredPosition = new Vector3(0, 50, 0); break;
            case ItemCategories.Knife: selectedBackground.anchoredPosition = new Vector3(50, 50, 0); break;
            case ItemCategories.Grenade: selectedBackground.anchoredPosition = new Vector3(-50, 0, 0); break;
            case ItemCategories.Flash: selectedBackground.anchoredPosition = new Vector3(0, 0, 0); break;
            case ItemCategories.Smoke: selectedBackground.anchoredPosition = new Vector3(50, 0, 0); break;
        }
    }
    [Command]
    private void CmdDropItem()
    {
        ServerDropItem(true, selectedItem);
    }
    [Server]
    private void ServerDropItem(bool selected, Item item)
    {
        if (item.category == ItemCategories.Knife ||
            item.category == ItemCategories.Empty) return;

        GameObject droppedItem = Instantiate(droppedItemPrefab, transform.position, Quaternion.identity);
        droppedItem.GetComponent<DroppedItem>().item = item;
        TargetClearSlotImage(item.category);

        if(selected)
        {
            RemoveSelectedItem();
            ServerAutoSelectSlot();
            RpcPlayDropItemSound();
        }     

        NetworkServer.Spawn(droppedItem);
    }
    [Command]
    private void CmdGetItem()
    {
        ServerGetItem(null);
    }
    [Server]
    public void ServerGetItem(string itemName)
    {
        if ((!canGetItem && itemName == null) || (currentCollision == null && itemName == null)) return;

        Item item;

        if (itemName == null)
            item = currentCollision.GetComponent<DroppedItem>().item;
        else
            item = itemList.ReturnNewItem(itemName);

        if (item.category == ItemCategories.PrimaryWeapon && primary.nickName != "") ServerDropItem(false, primary);
        else if (item.category == ItemCategories.SecondaryWeapon && secondary.nickName != "") ServerDropItem(false, secondary);

        AddItem(item);
        RpcPlayGetItemSound();
        if (item.category == ItemCategories.PrimaryWeapon) ServerSelectSlot(1);
        if (item.category == ItemCategories.SecondaryWeapon && (selectedItem.category == ItemCategories.Knife ||
            selectedItem.category == ItemCategories.SecondaryWeapon)) ServerSelectSlot(2);

        if(itemName == null)
        NetworkServer.Destroy(currentCollision);
    }
    [ClientRpc]
    private void RpcPlayGetItemSound()
    {
        inventoryAudioSource.PlayOneShot(getItemSound);
    }
    [ClientRpc]
    private void RpcPlayDropItemSound()
    {
        inventoryAudioSource.PlayOneShot(dropItemSound);
    }
    [TargetRpc]
    private void TargetSetCanGetItem(bool value)
    {
        canGetItem = value;
    }
    [TargetRpc]
    private void TargetGetItemText(string text)
    {
        if (text == "") getItemPanel.SetActive(false);
        else getItemPanel.SetActive(true);

        getItemText.text = text;
    }
    [Server]
    public void SyncSelectedItem()
    {
        var newItem = itemList.ReturnNewItem(selectedItem);
        if (newItem == null) return;

        if (newItem.nickName == primary.nickName) primary = newItem;
        else if (newItem.nickName == secondary.nickName) secondary = newItem;
        else if (newItem.nickName == knife.nickName) knife = newItem;
        else if (newItem.nickName == grenade.nickName) grenade = newItem;
        else if (newItem.nickName == flash.nickName) flash = newItem;
        else if (newItem.nickName == smoke.nickName) smoke = newItem;
        else if (newItem.nickName == c4.nickName) c4 = newItem;

        selectedItem = newItem;
    }
    [Server]
    public void RemoveSelectedItem()
    {
        if (selectedItem.nickName == primary.nickName) primary = new Item();
        else if (selectedItem.nickName == secondary.nickName) secondary = new Item();
        else if (selectedItem.nickName == knife.nickName) knife = new Item();
        else if (selectedItem.nickName == grenade.nickName) grenade = new Item();
        else if (selectedItem.nickName == flash.nickName) flash = new Item();
        else if (selectedItem.nickName == smoke.nickName) smoke = new Item();
        else if (selectedItem.nickName == c4.nickName) c4 = new Item();

        selectedItem = new Item();
    }
    [Server]
    private void SyncItem(Item item)
    {
        var newItem = itemList.ReturnNewItem(item);
        if (newItem == null) return;

        item = newItem;
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isServer) return;
        if(collision.CompareTag("DroppedItem"))
        {
            currentCollision = collision.gameObject;
            canGetItem = true;
            TargetSetCanGetItem(true);
            TargetGetItemText("Press 'E' to get " + currentCollision.GetComponent<DroppedItem>().item.realName);
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (!isServer) return;
        if (collision.CompareTag("DroppedItem"))
        {
            currentCollision = null;
            TargetSetCanGetItem(false);
            TargetGetItemText("");
            canGetItem = false;
        }
    }
}
