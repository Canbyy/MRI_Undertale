using UnityEngine;
using UnityEngine.InputSystem;

public class AutomatedDoor : MonoBehaviour
{
    private Animator animator;

    bool TriggerOpen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D col) {
        PlayerInput playerInput = col.GetComponent<PlayerInput>();

        if (playerInput) {
            this.TriggerOpen = true;
            animator.SetBool("TriggerOpen", TriggerOpen);
            // Open the door here (e.g., play an animation, disable the collider, etc.)
            Debug.Log("Door opened!");
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        PlayerInput playerInput = col.GetComponent<PlayerInput>();
        
        if (playerInput) {
            TriggerOpen = false;
            animator.SetBool("TriggerOpen", TriggerOpen);
            // Close the door here (e.g., play an animation, enable the collider, etc.)
            Debug.Log("Door closed!");
        }
    }
}
