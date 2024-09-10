using System;

[Serializable]
public class Item
{
    public string nickName;
    public string realName;
    public string description;
    public int amount;
    public int maxAmount;
    public int ammoInside;
    public int maxAmmoInside;
    public int totalAmmo;
    public float range;
    public float fireRate;
    public float maxFireRate;
    public int minDamage;
    public int maxDamage;
    public int speedReduction;
    public bool isMarketItem;
    public int marketPrice;
    public ItemCategories category;

    public Item(string nickName, string realName, string description, int amount, int maxAmount, int ammoInside, int maxAmmoInside,
        int totalAmmo, float range, float fireRate, float maxFireRate, int minDamage, int maxDamage, int speedReduction,
        bool isMarketItem, int marketPrice,
        ItemCategories category)
    {
        this.nickName = nickName;
        this.realName = realName;
        this.description = description;
        this.amount = amount;
        this.maxAmount = maxAmount;
        this.ammoInside = ammoInside;
        this.maxAmmoInside = maxAmmoInside;
        this.totalAmmo = totalAmmo;
        this.range = range;
        this.fireRate = fireRate;
        this.maxFireRate = maxFireRate;
        this.minDamage = minDamage;
        this.maxDamage = maxDamage;
        this.speedReduction = speedReduction;
        this.isMarketItem = isMarketItem;
        this.marketPrice = marketPrice;
        this.category = category;
    }

    public Item() { }
}

public enum ItemCategories
{
    Empty,
    PrimaryWeapon,
    SecondaryWeapon,
    Knife,
    Grenade,
    Flash,
    Smoke,
    C4
}
