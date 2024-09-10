using Mirror;
using UnityEngine;

public class FieldOfView : NetworkBehaviour
{
    private Transform mapParent;
    [SerializeField] private LayerMask raycastBlockMask;

    private Color darkColor = new Color(0.9f, 0.9f, 0.9f);
    private Color lightColor = new Color(1.00f, 1.00f, 1.00f);

    private bool spectating;

    public override void OnStartLocalPlayer()
    {
        mapParent = GameObject.FindGameObjectWithTag("Map").transform;

        for (int i = 0; i < mapParent.childCount; i++)
        {
            Transform child = mapParent.GetChild(i);
            child.GetComponent<SpriteRenderer>().color = darkColor;
        }
    }
    public void SetSpactating(bool value)
    {
        spectating = value;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isLocalPlayer && !spectating) return;

        if (collision.gameObject.layer != 8 && collision.tag != "Player") return;

        collision.gameObject.layer = 9;
        var hit = Physics2D.Raycast(transform.position, collision.transform.position - transform.position, 100, raycastBlockMask);
        //Debug.DrawRay(transform.position, collision.transform.position - transform.position, Color.green);

        if (collision.tag != "Player")
            collision.gameObject.layer = 8;
        else
            collision.gameObject.layer = 11;

        if (hit.collider != null && hit.collider.gameObject == collision.gameObject)
        {
            if (collision.tag != "Player")
                collision.GetComponent<SpriteRenderer>().color = lightColor;
            else
                collision.GetComponent<SpriteRenderer>().enabled = true;
        }

        else
        {
            if (collision.tag != "Player")
                collision.GetComponent<SpriteRenderer>().color = darkColor;
            else
                collision.GetComponent<SpriteRenderer>().enabled = false;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isLocalPlayer && !spectating) return;
        if (Vector2.Distance(transform.position, collision.transform.position) <= 1.5f) return;

        if (collision.gameObject.layer != 8 && collision.tag != "Player") return;

        if (collision.tag != "Player")
            collision.GetComponent<SpriteRenderer>().color = darkColor;
        else
            collision.GetComponent<SpriteRenderer>().enabled = false;
    }
}
