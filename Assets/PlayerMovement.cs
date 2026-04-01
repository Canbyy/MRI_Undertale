using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 5f;
    public int facingDirection = 0;
    public Rigidbody2D rb;
    public Vector2 moveInput;
    public Animator animator;

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

        if (horizontal > 0 && transform.localScale.x < 0 ||
            horizontal < 0 && transform.localScale.x > 0)
        {
            Flip();
        }

        animator.SetFloat("horizontal", Mathf.Abs(horizontal)); // Update the "moveX" parameter in the animator to control horizontal movement animations
        animator.SetFloat("vertical", (vertical)); // Update the "moveY" parameter in the animator to control vertical movement animations


        rb.velocity = new Vector2(horizontal * moveSpeed, vertical * moveSpeed); // Set the velocity of the Rigidbody2D based on input and move speed
    }

    void Flip()
    {
        facingDirection *= 1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

}