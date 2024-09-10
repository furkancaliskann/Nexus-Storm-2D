using Mirror;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class Weapon : NetworkBehaviour
{
    private AmmoText ammoText;
    private Inventory inventory;
    private LockMovement lockMovement;
    private PlayerMovement playerMovement;
    private PlayerStats playerStats;

    [SerializeField] private Animation muzzleFlashAnim;
    [SerializeField] private AudioSource gunAudioSource;
    [SerializeField] private Transform gunPoint;
    [SerializeField] private GameObject bulletTrailPrefab;

    [SerializeField] private LayerMask weaponMask;

    // Shoot Sounds
    private List<AudioClip> shootSounds = new List<AudioClip>();

    // Knife Sounds
    [SerializeField] private AudioClip knifeSlashSound;
    [SerializeField] private AudioClip knifeSlashDamagedSound;

    // Reload Sound
    [SerializeField] private AudioClip reloadSound;

    // Empty Clip Sound
    [SerializeField] private AudioClip emptyClipSound;

    [SyncVar] public bool reloading;
    private float reloadProgressCounter;
    private float reloadTime = 1.7f;
    [SerializeField] private RectTransform reloadProgressBar;
    [SerializeField] private GameObject reloadPanel;

    public override void OnStartServer()
    {
        ammoText = GetComponent<AmmoText>();
        playerStats = GetComponent<PlayerStats>();
    }
    public override void OnStartClient()
    {
        inventory = GetComponent<Inventory>();
        reloadProgressCounter = 0;
    }
    public override void OnStartLocalPlayer()
    {
        lockMovement = GetComponent<LockMovement>();
        playerMovement = GetComponent<PlayerMovement>();

        shootSounds.AddRange(Resources.LoadAll<AudioClip>("Item Shoot Sounds/"));
    }
    void Update()
    {
        if (isServer)
        {
            CheckFireRate();
            CheckAutoReload();
        }

        CheckReloadProgress();

        if (!isLocalPlayer) return;
        CheckInputs();
    }
    private void CheckInputs()
    {
        if (reloading || lockMovement.lockType == LockTypes.All ||
            playerMovement.freezed || EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButton(0))
        {
            Item item = inventory.selectedItem;

            if ((item.category == ItemCategories.PrimaryWeapon || item.category == ItemCategories.SecondaryWeapon) &&
                item.fireRate <= 0 && item.ammoInside > 0)
            {
                if (!isServer)
                {
                    item.fireRate = item.maxFireRate;
                    item.ammoInside--;
                }

                CmdShoot();
            }

            else if (item.category == ItemCategories.Knife && item.fireRate <= 0)
            {
                if (!isServer)
                {
                    item.fireRate = item.maxFireRate;
                }

                CmdSlashKnife();
            }


            else if ((item.category == ItemCategories.Grenade || item.category == ItemCategories.Flash ||
                item.category == ItemCategories.Smoke) && item.fireRate <= 0 && item.amount > 0)
            {
                if (!isServer)
                {
                    item.fireRate = item.maxFireRate;
                    item.amount--;
                }

                CmdThrowGrenade();
            }

        }
        if (Input.GetMouseButtonDown(0))
        {
            Item item = inventory.selectedItem;

            if ((item.category == ItemCategories.PrimaryWeapon || item.category == ItemCategories.SecondaryWeapon) &&
                item.fireRate <= 0 && item.ammoInside <= 0)
            {
                CmdPlayEmptyClipSound();
            }
        }

        if (lockMovement.lockType == LockTypes.Input) return;
        if (Input.GetKeyDown(KeyCode.F))
        {
            Item item = inventory.selectedItem;

            if (item.category == ItemCategories.C4) CmdPlantBomb();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Item item = inventory.selectedItem;
            if ((item.category == ItemCategories.PrimaryWeapon || item.category == ItemCategories.SecondaryWeapon) &&
                item.ammoInside < item.maxAmmoInside && item.totalAmmo > 0)
            {
                CmdReload();
            }
        }
    }
    private void CheckFireRate()
    {
        Item item = inventory.selectedItem;
        if (item.fireRate > 0)
        {
            item.fireRate -= Time.deltaTime;
            if (item.fireRate <= 0) inventory.SyncSelectedItem();
        }
    }
    private void CheckAutoReload()
    {
        if (reloading) return;

        Item item = inventory.selectedItem;

        if ((item.category != ItemCategories.PrimaryWeapon && item.category != ItemCategories.SecondaryWeapon) ||
            item.ammoInside > 0 || item.totalAmmo <= 0) return;

        Reload();
    }
    private void CheckReloadProgress()
    {
        if (reloading && reloadProgressCounter < 2)
        {
            reloadProgressCounter += Time.deltaTime;

            if (isLocalPlayer)
            {
                reloadProgressBar.transform.localScale = new Vector3(reloadProgressCounter / reloadTime, 1, 1);
            }

        }
        if (reloading && isServer && reloadProgressCounter >= reloadTime)
        {
            FinishReload();
        }
    }

    [Command]
    private void CmdShoot()
    {
        Item item = inventory.selectedItem;

        if ((item.category != ItemCategories.PrimaryWeapon && item.category != ItemCategories.SecondaryWeapon) ||
            item.fireRate > 0 || item.ammoInside <= 0) return;

        item.fireRate = item.maxFireRate;
        RaycastHit2D hit = Physics2D.Raycast(gunPoint.position, transform.up, item.range, weaponMask);
        CheckHit(hit);
        RpcPlayShootSound();
        RpcPlayMuzzleAnim();
        SpawnTrail(hit);

        item.ammoInside--;
        inventory.SyncSelectedItem();
        ammoText.TargetUpdatePanel(item.ammoInside, item.totalAmmo);
    }
    [Server]
    private void CheckHit(RaycastHit2D hit)
    {
        if (hit.transform == null || !hit.transform.root.CompareTag("Player")) return;
        GameObject hitObject = hit.transform.root.gameObject;

        Item item = inventory.selectedItem;
        Injury injury = hitObject.GetComponent<Injury>();

        hitObject.GetComponent<PlayerStats>().DecreaseHealth(Random.Range(item.minDamage, item.maxDamage + 1),
            playerStats.nickName, playerStats.team, item.nickName);
        injury.TargetPlayInjuryEffect();
        if(item.category != ItemCategories.Knife) injury.RpcPlayGetDamageSound();
    }
    [Command]
    private void CmdPlayEmptyClipSound()
    {
        Item item = inventory.selectedItem;

        if ((item.category != ItemCategories.PrimaryWeapon && item.category != ItemCategories.SecondaryWeapon) ||
            item.fireRate > 0 || item.ammoInside > 0) return;

        RpcPlaySound(AudioClips.EmptyClip);
    }
    [Command]
    private void CmdReload()
    {
        Reload();
    }
    [Server]
    private void Reload()
    {
        if (reloading) return;

        Item item = inventory.selectedItem;

        if ((item.category != ItemCategories.PrimaryWeapon && item.category != ItemCategories.SecondaryWeapon) ||
            item.ammoInside == item.maxAmmoInside || item.totalAmmo <= 0) return;

        reloading = true;
        RpcPlaySound(AudioClips.Reload);
        TargetSetReloadPanel(true);
    }
    [Server]
    private void FinishReload()
    {
        Item item = inventory.selectedItem;

        int requiredAmmo = item.maxAmmoInside - item.ammoInside;
        if (item.totalAmmo >= requiredAmmo)
        {
            item.ammoInside = item.maxAmmoInside;
            item.totalAmmo -= requiredAmmo;
        }
        else
        {
            item.ammoInside += item.totalAmmo;
            item.totalAmmo = 0;
        }

        inventory.SyncSelectedItem();
        ammoText.TargetUpdatePanel(item.ammoInside, item.totalAmmo);
        reloading = false;
        reloadProgressCounter = 0;
        TargetSetReloadPanel(false);
    }
    [TargetRpc]
    private void TargetSetReloadPanel(bool value)
    {
        reloadPanel.SetActive(value);
        if (!value) reloadProgressCounter = 0;
    }
    [Command]
    private void CmdSlashKnife()
    {
        Item item = inventory.selectedItem;

        if (item.category != ItemCategories.Knife || item.fireRate > 0) return;

        item.fireRate = item.maxFireRate;
        RaycastHit2D hit = Physics2D.Raycast(gunPoint.position, transform.up, item.range, weaponMask);
        CheckHit(hit);
        bool isHit = false;

        if (hit.transform != null) isHit = hit.transform.CompareTag("Player");

        if (isHit) RpcPlaySound(AudioClips.KnifeSlashDamaged);
        else RpcPlaySound(AudioClips.KnifeSlash);
        inventory.SyncSelectedItem();
    }
    [Command]
    private void CmdThrowGrenade()
    {
        Item item = inventory.selectedItem;

        if ((item.category != ItemCategories.Grenade && item.category != ItemCategories.Flash &&
                item.category != ItemCategories.Smoke) || item.fireRate > 0 && item.amount <= 0) return;

        item.fireRate = item.maxFireRate;
        item.amount--;

        inventory.SyncSelectedItem();
    }
    [Command]
    private void CmdPlantBomb()
    {
        Item item = inventory.selectedItem;
        if (item.category != ItemCategories.C4) return;
    }
    [Server]
    private void SpawnTrail(RaycastHit2D hit)
    {
        Vector3 targetPos;
        if (hit.collider != null)
        {
            targetPos = hit.point;
        }
        else
        {
            targetPos = gunPoint.position + transform.up * inventory.selectedItem.range;
        }

        RpcSpawnTrail(targetPos);
    }
    [ClientRpc]
    private void RpcSpawnTrail(Vector3 targetPos)
    {
        GameObject trail = Instantiate(bulletTrailPrefab, gunPoint.position, transform.rotation);
        BulletTrail bulletTrail = trail.GetComponent<BulletTrail>();
        bulletTrail.SetTargetPosition(targetPos);
    }
    [ClientRpc]
    private void RpcPlayMuzzleAnim()
    {
        muzzleFlashAnim.Play("Muzzle Flash");
    }
    [ClientRpc]
    private void RpcPlayShootSound()
    {
        AudioClip sound = shootSounds.Find(x => x.name == inventory.selectedItem.nickName + "_shoot");
        if (sound == null) return;

        gunAudioSource.PlayOneShot(sound);
    }
    [ClientRpc]
    private void RpcPlaySound(AudioClips audioClip)
    {
        switch(audioClip)
        {
            case AudioClips.Reload: gunAudioSource.PlayOneShot(reloadSound); break;
            case AudioClips.KnifeSlash: gunAudioSource.PlayOneShot(knifeSlashSound); break;
            case AudioClips.KnifeSlashDamaged: gunAudioSource.PlayOneShot(knifeSlashDamagedSound); break;
            case AudioClips.EmptyClip: gunAudioSource.PlayOneShot(emptyClipSound); break;
        }
    }
}

public enum AudioClips
{
    Reload,
    KnifeSlash,
    KnifeSlashDamaged,
    EmptyClip,
}
