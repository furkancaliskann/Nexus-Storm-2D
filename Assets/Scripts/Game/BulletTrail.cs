using Mirror;
using UnityEngine;

public class BulletTrail : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 targetPosition;

    private float progress;
    private float speed = 40f;

    void Start()
    {
        startPosition = transform.position = new Vector3(transform.position.x, transform.position.y, -1);
        Invoke(nameof(DestroyObject), 1f);
    }
    void Update()
    {
        progress += Time.deltaTime * speed;
        transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
    }
    public void SetTargetPosition(Vector3 targetPosition)
    {
        this.targetPosition = new Vector3(targetPosition.x, targetPosition.y, -1);
    }
    private void DestroyObject()
    {
        Destroy(gameObject);
    }
}
