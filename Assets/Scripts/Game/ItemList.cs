using System.Collections.Generic;
using UnityEngine;

public class ItemList : MonoBehaviour
{
    private List<Item> items = new List<Item>();

    void Awake()
    {
        CreateWeapon("ak47", "AK-47", "Strong and loud.", 30, 90, 60, 0.1f, 20, 35, 15, true, 3000, true);
        CreateWeapon("dragunov", "Dragunov", "", 10, 50, 90, 0.5f, 40, 55, 20, true, 5000, true);
        CreateWeapon("g36", "G36", "", 30, 120, 48, 0.12f, 18, 32, 15, true, 2800, true);
        CreateWeapon("mac10", "Mac10", "", 25, 100, 30, 0.075f, 10, 25, 10, true, 1500, true);
        CreateWeapon("mp5", "MP5", "", 30, 90, 36, 0.1f, 12, 30, 10, true, 1800, true);
        CreateWeapon("sg552", "SG552", "", 25, 100, 66, 0.15f, 25, 40, 15, true, 3400, true);
        CreateWeapon("awp", "AWP", "", 5, 30, 120, 1.5f, 100, 100, 20, true, 5500, true);

        CreateWeapon("usp", "Usp", "Basic weapon.", 12, 48, 24, 0.25f, 10, 20, 5, true, 600, false);
        CreateWeapon("desert_eagle", "Desert Eagle", "", 7, 35, 45, 0.35f, 35, 55, 5, true, 1000, false);
        CreateWeapon("tec9", "Tec9", "", 15, 75, 27, 0.2f, 15, 25, 5, true, 1300, false);

        CreateKnife("knife", "Knife", "Close range weapon.", 0.35f, 0.35f, 50, 50);

        CreateGrenade("grenade", "Grenade", "Throwable bomb.", 0, 1, 30, 1, 150, 150, true, 450, ItemCategories.Grenade);
        CreateGrenade("flash", "Flash Grenade", "It blinds its enemies.", 0, 2, 30, 1, 0, 0, true, 250, ItemCategories.Flash);
        CreateGrenade("smoke", "Smoke Grenade", "It restricts the field of view.", 0, 1, 30, 1, 0, 0, true, 350, ItemCategories.Smoke);

        CreateC4("c4", "C4", "It must be placed in a specific area.", 200, 200);
    }

    private void CreateWeapon(string nickName, string realName, string description,
        int maxAmmoInside, int totalAmmo, float range, float fireRate, int minDamage, int maxDamage, int speedReduction, bool isMarketItem,
        int marketPrice, bool isPrimary)
    {
        ItemCategories category;
        if(isPrimary) category = ItemCategories.PrimaryWeapon;
        else category = ItemCategories.SecondaryWeapon;

        items.Add(new Item(nickName, realName, description, 1, 1, maxAmmoInside, maxAmmoInside, totalAmmo,
            range, 0, fireRate, minDamage, maxDamage, speedReduction, isMarketItem, marketPrice, category));
    }
    private void CreateKnife(string nickName, string realName, string description, float range, float fireRate, int minDamage, int maxDamage)
    {
        items.Add(new Item(nickName, realName, description, 1, 1, 0, 0, 0, range, 0, fireRate, minDamage, maxDamage, 0, false, 0, ItemCategories.Knife));
    }
    private void CreateGrenade(string nickName, string realName, string description, int amount, int maxAmount,
        float range, float fireRate, int minDamage, int maxDamage, bool isMarketItem, int marketPrice, ItemCategories grenadeCategory)
    {
        items.Add(new Item(nickName, realName, description, amount, maxAmount, 0, 0, 0, range, 0, fireRate, minDamage, maxDamage,
            0, isMarketItem, marketPrice, grenadeCategory));
    }
    private void CreateC4(string nickName, string realName, string description, int minDamage, int maxDamage)
    {
        items.Add(new Item(nickName, realName, description, 0, 1, 0, 0, 0, 0, 0, 0, minDamage, maxDamage, 0, false, 0, ItemCategories.C4));
    }

    public Item ReturnNewItem(string nickName)
    {
        var item = items.Find(x => x.nickName == nickName);
        if (item == null) return new Item();

        return new Item(item.nickName, item.realName, item.description, item.amount, item.maxAmount,
            item.ammoInside, item.maxAmmoInside, item.totalAmmo, item.range, item.fireRate, item.maxFireRate,
            item.minDamage, item.maxDamage, item.speedReduction, item.isMarketItem, item.marketPrice, item.category);
    }
    public Item ReturnNewItem(Item item)
    {
        return new Item(item.nickName, item.realName, item.description, item.amount, item.maxAmount,
            item.ammoInside, item.maxAmmoInside, item.totalAmmo, item.range, item.fireRate, item.maxFireRate,
            item.minDamage, item.maxDamage, item.speedReduction, item.isMarketItem, item.marketPrice, item.category);
    }
    public List<Item> ReturnMarketItems()
    {
        var list = new List<Item>();
        foreach (var item in items)
        {
            if (item.isMarketItem) list.Add(item);
        }

        return list;
    }
}
