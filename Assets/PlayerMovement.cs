using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    [System.Obsolete]
    void FixedUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // Get horizontal input (A/D or Left/Right)
        float vertical = Input.GetAxisRaw("Vertical"); // Get vertical input (W/S or Up/Down)

        animator.SetFloat("horizontal", Mathf.Abs(horizontal); // Update the "moveX" parameter in the animator to control horizontal movement animations
        animator.SetFloat("vertical", Mathf.Abs(vertical)); // Update the "moveY" parameter in the animator to control vertical movement animations


        rb.velocity = new Vector2(horizontal * moveSpeed, vertical * moveSpeed); // Set the velocity of the Rigidbody2D based on input and move speed
    }


    public void Move(InputAction.CallbackContext context)
    {
        animator.SetBool("isWalking", true); // Set the "isMoving" parameter in the animator based on whether the player is moving

        if (context.canceled)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
        moveInput = context.ReadValue<Vector2>(); // Read the movement input from the player
        animator.SetFloat("InputX", moveInput.x); // Set the "moveX" parameter in the animator to control horizontal movement animations
        animator.SetFloat("InputY", moveInput.y); // Set the "moveY" parameter in the animator to control vertical movement animations
    }

}
