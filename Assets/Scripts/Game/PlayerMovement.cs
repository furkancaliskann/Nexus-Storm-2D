using Mirror;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private Camera cam;
    private Inventory inventory;
    private LockMovement lockMovement;
    private Rigidbody2D rb;
    
    private float runSpeed = 3.5f;
    private float walkSpeed = 1.5f;
    public Vector2 moveDirection = Vector2.zero;
    private float rotationAngle = 0;

    public bool freezed;
    public MovementTypes movementType;
    public MovementTypes lastSendedMovementType;
    private float sendMovementSensitivity = 0.5f;
    private Vector2 lastSendedPosition;

    void Awake()
    {
        cam = Camera.main;
        inventory = GetComponent<Inventory>();
        lockMovement = GetComponent<LockMovement>();
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        if (!isLocalPlayer) return;
        Move();
        Rotate();
    }
    void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        rb.velocity = moveDirection;
        rb.rotation = rotationAngle;
    }
    void LateUpdate()
    {
        if (!isLocalPlayer) return;
        cam.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }
    private void Move()
    {
        if (lockMovement.lockType == LockTypes.Movement || lockMovement.lockType == LockTypes.All || freezed) 
        {
            moveDirection  = Vector2.zero;
            SendMovementType(MovementTypes.Idle);
            return; 
        }

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(KeyCode.LeftControl))
        {
            moveDirection = new Vector2(x, y).normalized * walkSpeed;
            SendMovementType(MovementTypes.Walk);
        }
        else
        {
            if (inventory.selectedItem.nickName != null && inventory.selectedItem.speedReduction != 0)
            {
                float decreaseAmount = runSpeed * (inventory.selectedItem.speedReduction / 100f);
                moveDirection = new Vector2(x, y).normalized * (runSpeed - decreaseAmount);
            } 
            else
                moveDirection = new Vector2(x, y).normalized * runSpeed;

            if (moveDirection != Vector2.zero && rb.velocity != Vector2.zero)
            {
                SendMovementType(MovementTypes.Run);
            }
            else SendMovementType(MovementTypes.Idle);
        }
    }
    private void Rotate()
    {
        if (lockMovement.lockType == LockTypes.Movement || lockMovement.lockType == LockTypes.All) return;
        Vector3 mousePosition = Input.mousePosition;
        mousePosition = cam.ScreenToWorldPoint(mousePosition);

        Vector2 direction = new Vector2(
            mousePosition.x - transform.position.x,
            mousePosition.y - transform.position.y
        );

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rotationAngle = angle - 90f;

    }
    [TargetRpc]
    public void TargetSetFreezeVariable(bool value)
    {
        freezed = value;
    }
    private void SendMovementType(MovementTypes type)
    {
        if (lastSendedMovementType == type) return;
        if (Vector3.Distance(transform.position, lastSendedPosition) < sendMovementSensitivity  &&
            lastSendedPosition != Vector2.zero && type != MovementTypes.Idle) return;

        CmdSendMovementType(type);
        lastSendedMovementType = type;
        lastSendedPosition = transform.position;
    }
    [Command]
    private void CmdSendMovementType(MovementTypes type)
    {
        RpcSendMovementType(type);
    }
    [ClientRpc]
    private void RpcSendMovementType(MovementTypes type)
    {
        movementType = type;
    }
}

public enum MovementTypes
{
    Idle,
    Walk,
    Run,
}
