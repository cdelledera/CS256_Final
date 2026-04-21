using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movementInput;

    void Start()
    {
        // Automatically grab the Rigidbody attached to the player
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // --- THE NEW SAFETY CHECK ---
        // If a menu is open, zero out our speed and stop reading the keyboard!
        if (GameManager.Instance.isUIActive)
        {
            movementInput = Vector2.zero;
            return;
        }

        // 1. CAPTURE INPUT (Runs only if menus are closed)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        movementInput = new Vector2(moveX, moveY).normalized;
    }

    void FixedUpdate()
    {
        // 2. APPLY MOVEMENT (Runs in sync with the physics engine)
        // Move the physical body to its current position + our input direction * speed
        rb.MovePosition(rb.position + movementInput * moveSpeed * Time.fixedDeltaTime);
    }
}