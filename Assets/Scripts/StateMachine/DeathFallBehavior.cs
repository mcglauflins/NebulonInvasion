// using UnityEngine;

// public class DeathFallBehavior : StateMachineBehaviour
// {
//     [Header("Death Settings")]
//     [SerializeField] private float deathGravityScale = 1f;
//     [SerializeField] private float deathDrag = 0.5f;
//     [SerializeField] private bool disableCollisions = false;
    
//     // Store original values
//     private float originalGravityScale;
//     private float originalDrag;
//     private bool originalColliderEnabled;
    
//     // Called when entering the death state
//     override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
//     {
//         Rigidbody2D rb = animator.GetComponent<Rigidbody2D>();
//         Collider2D col = animator.GetComponent<Collider2D>();
        
//         if (rb != null)
//         {
//             // Store original values
//             originalGravityScale = rb.gravityScale;
//             originalDrag = rb.linearDamping;
            
//             // Apply death physics
//             rb.gravityScale = deathGravityScale;
//             rb.linearDamping = deathDrag;
            
//             // Stop horizontal movement but keep vertical
//             rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
//         }
        
//         if (col != null && disableCollisions)
//         {
//             originalColliderEnabled = col.enabled;
//             col.enabled = false;
//         }
        
//         // Disable player controls if they exist
//         PlayerController controller = animator.GetComponent<PlayerController>();
//         if (controller != null)
//         {
//             controller.enabled = false;
//         }
//     }
    
//     // Called every frame during the death state
//     override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
//     {
//         // Keep horizontal movement at 0 during death
//         Rigidbody2D rb = animator.GetComponent<Rigidbody2D>();
//         if (rb != null)
//         {
//             rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
//         }
        
//         // Optional: Add rotation during fall
//         // animator.transform.Rotate(0, 0, 180 * Time.deltaTime);
//     }
    
//     // Called when exiting the death state (respawn)
//     override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
//     {
//         Rigidbody2D rb = animator.GetComponent<Rigidbody2D>();
//         Collider2D col = animator.GetComponent<Collider2D>();
        
//         if (rb != null)
//         {
//             // Restore original physics values
//             rb.gravityScale = originalGravityScale;
//             rb.linearDamping = originalDrag;
//             rb.linearVelocity = Vector2.zero; // Stop all movement
//         }
        
//         if (col != null && disableCollisions)
//         {
//             col.enabled = originalColliderEnabled;
//         }
        
//         // Re-enable player controls
//         PlayerController controller = animator.GetComponent<PlayerController>();
//         if (controller != null)
//         {
//             controller.enabled = true;
//         }
//     }
// }