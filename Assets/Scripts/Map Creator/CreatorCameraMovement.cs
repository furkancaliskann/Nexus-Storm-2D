using UnityEngine;

public class CreatorCameraMovement : MonoBehaviour
{
    private Camera cam;
    private CreatorBuilding creatorBuilding;

    private Vector3 originalCameraPosition;
    private float originalCameraSize;

    private float moveSpeed = 25f;
    private float zoomSpeed = 1f;

    private float minCameraSize = 2f;
    private float maxCameraSize = 25f;

    private float minX = -200;
    private float maxX = 200;
    private float minY = -200;
    private float maxY = 200;

    void Awake()
    {
        cam = GetComponent<Camera>();
        creatorBuilding = GetComponent<CreatorBuilding>();
        originalCameraPosition = Camera.main.transform.position;
        originalCameraSize = Camera.main.orthographicSize;
    }
    void Update()
    {
        CheckInputs();
    }
    private void CheckInputs()
    {
        if (creatorBuilding.IsInputfieldFocused()) return;

        if (Input.GetKey(KeyCode.W)) Move(new Vector2(0f, moveSpeed) * Time.deltaTime);
        if (Input.GetKey(KeyCode.S)) Move(new Vector2(0f, -moveSpeed) * Time.deltaTime);
        if (Input.GetKey(KeyCode.A)) Move(new Vector2(-moveSpeed, 0f) * Time.deltaTime);
        if (Input.GetKey(KeyCode.D)) Move(new Vector2(moveSpeed, 0f) * Time.deltaTime);

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput > 0) Zoom(-zoomSpeed);
        else if (scrollInput < 0) Zoom(zoomSpeed);
    }
    private void Move(Vector2 pos)
    {
        if(pos.x > 0 && transform.position.x + pos.x > maxX)
        {
            transform.position = new Vector3(maxX, transform.position.y, transform.position.z);
            return;
        }
        if (pos.x < 0 && transform.position.x + pos.x < minX)
        {
            transform.position = new Vector3(minX, transform.position.y, transform.position.z);
            return;
        }
        if (pos.y > 0 && transform.position.y + pos.y > maxY)
        {
            transform.position = new Vector3(transform.position.x, maxY, transform.position.z);
            return;
        }
        if (pos.y < 0 && transform.position.y + pos.y < minY)
        {
            transform.position = new Vector3(transform.position.x, minY, transform.position.z);
            return;
        }

        transform.position += (Vector3)pos;
    }
    private void Zoom(float value)
    {
        if(cam.orthographicSize + value < minCameraSize)
        {
            cam.orthographicSize = minCameraSize;
            return;
        }
        else if (cam.orthographicSize + value > maxCameraSize)
        {
            cam.orthographicSize = maxCameraSize;
            return;
        }

        cam.orthographicSize += value;
    }
    public void ResetCameraSettings()
    {
        cam.transform.position = originalCameraPosition;
        cam.orthographicSize = originalCameraSize;
    }
}
