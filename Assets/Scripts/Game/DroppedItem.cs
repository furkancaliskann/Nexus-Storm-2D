using Mirror;
using UnityEngine;

public class DroppedItem : NetworkBehaviour
{
    [SyncVar]
    public Item item;

    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Item Images/" + item.nickName);
    }
}
