using System.Collections.Generic;
using UnityEngine;

public class DeathNoticeVariables : MonoBehaviour
{
    public GameObject deathNoticePrefab;
    public Transform deathNoticeParent;
    public List<Sprite> weaponImages = new List<Sprite>();

    void Awake()
    {
        weaponImages.AddRange(Resources.LoadAll<Sprite>("Item Images/"));
    }
}
