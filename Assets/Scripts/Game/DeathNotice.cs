using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathNotice : NetworkBehaviour
{
    private DeathNoticeVariables variables;

    private GameObject deathNoticePrefab;
    private Transform deathNoticeParent;
    private List<Sprite> weaponImages = new List<Sprite>();

    private List<GameObject> deathNotices = new List<GameObject>();
    private float removeCounter = 5f;
    private float removeCounterMax = 5f;

    public override void OnStartAuthority()
    {
        variables = Camera.main.GetComponent<DeathNoticeVariables>();
        deathNoticePrefab = variables.deathNoticePrefab;
        deathNoticeParent = variables.deathNoticeParent;
        weaponImages = variables.weaponImages;
    }
    void Update()
    {
        if (!isOwned) return;
        CheckList();
    }

    [TargetRpc]
    public void TargetAddNotice(string killer, Teams killerTeam, string weaponName, string killed, Teams killedTeam)
    {
        GameObject notice = Instantiate(deathNoticePrefab, deathNoticeParent);

        Text killerText = notice.GetComponentsInChildren<Text>()[0];
        killerText.text = killer;
        killerText.color = killerTeam == Teams.Blue ? Color.blue : Color.red; 

        notice.GetComponentsInChildren<Image>()[1].sprite = weaponImages.Find(x => x.name == weaponName);

        Text killedText = notice.GetComponentsInChildren<Text>()[1];
        killedText.text = killed;
        killedText.color = killedTeam == Teams.Blue ? Color.blue : Color.red;
        deathNotices.Add(notice);
    }

    private void CheckList()
    {
        if (deathNotices.Count > 0) removeCounter -= Time.deltaTime;
        if(removeCounter <= 0)
        {
            Destroy(deathNotices[0]);
            deathNotices.RemoveAt(0);
            removeCounter = removeCounterMax;
        }
    }
}
