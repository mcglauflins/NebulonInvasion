using System.Runtime;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem; 

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(PlayerDamagable))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    public float walkSpeed = 5f;
    
    [SerializeField]
    public float runSpeed = 8f;
    
    [SerializeField]
    public float jumpImpulse = 10f;

    [SerializeField]
    public float wallSlideSpeed = 2f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    private bool isRunning = false;

    private bool isShooting = false;
    private float shootRecoveryTime = 0.2f; 
    private float shootTimer = 0f;
    
    UnityEngine.Vector2 moveInput;

    TouchingDirections touchingDirections;
    PlayerDamagable playerDamagable; 
    
    [SerializeField]
    private bool _isMoving = false;
    public bool IsMoving { get 
        {
            return _isMoving && !touchingDirections.IsOnWall; 
        } 
        private set
        {
            _isMoving = value;
            animator.SetBool(AnimationStrings.isMoving, value); 
        }
    }
    
    public bool _isFacingRight = true;
    public bool IsFacingRight { get {return _isFacingRight; } private set {
        if(_isFacingRight != value)
        {
            transform.localScale *= new UnityEngine.Vector2(-1, 1);
        }

        _isFacingRight = value;
    }}

    Rigidbody2D rb;
    Animator animator;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
        playerDamagable = GetComponent<PlayerDamagable>();  
    }
    
    private void FixedUpdate()
    {
        if (playerDamagable != null && playerDamagable.IsDead) return;
        
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        rb.linearVelocity = new UnityEngine.Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);

        animator.SetFloat(AnimationStrings.yVelocity, rb.linearVelocity.y);
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        if (playerDamagable != null && playerDamagable.IsDead) return;
        
        moveInput = context.ReadValue<UnityEngine.Vector2>();
        
        IsMoving = moveInput != UnityEngine.Vector2.zero;

        SetFacingDirection(moveInput);
    }

    private void Update()
    {
        if(isShooting)
        {
            shootTimer -= Time.deltaTime;
            if(shootTimer <= 0)
            {
                isShooting = false;
            }
        }
    }
    
    public void OnRun(InputAction.CallbackContext context)
    {
        if (playerDamagable != null && playerDamagable.IsDead) return;
        
        if (context.performed)
        {
            isRunning = true;
            animator.SetBool(AnimationStrings.isRunning, true);
        }
        else if (context.canceled)
        {
            isRunning = false;
            animator.SetBool(AnimationStrings.isRunning, false);
        }
    }

    private void SetFacingDirection(UnityEngine.Vector2 moveInput)
    {
        if(moveInput.x > 0 && !IsFacingRight)
        {
            IsFacingRight = true;
        }
        else if (moveInput.x < 0 && IsFacingRight)
        {
            IsFacingRight = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (playerDamagable != null && playerDamagable.IsDead) return;
        
        if(context.started && touchingDirections.IsGrounded)
        {
            animator.SetTrigger(AnimationStrings.jump);
            rb.linearVelocity = new UnityEngine.Vector2(rb.linearVelocity.x, jumpImpulse);
        }
    }

    public void OnFire(InputAction.CallbackContext context) 
    {
        if (playerDamagable != null && playerDamagable.IsDead) return;
        
        if(context.started)
        {
            animator.SetTrigger(AnimationStrings.fire);
            isShooting = true;
            shootTimer = shootRecoveryTime;
            
            if (projectilePrefab == null || firePoint == null)
            {
                return;
            }
            
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            
            if (projectileScript != null)
            {
                float direction = IsFacingRight ? 1f : -1f;
                projectileScript.SetDirection(direction);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (playerDamagable != null)
        {
            playerDamagable.TakeDamage(damage);
        }
    }

    public void ResetAllMovement()
{
    moveInput = Vector2.zero;
    
    IsMoving = false;
    
    if (animator != null)
    {
        animator.SetBool(AnimationStrings.isMoving, false);
        animator.SetBool(AnimationStrings.isRunning, false);
    }
    
    if (rb != null)
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
}
    

}