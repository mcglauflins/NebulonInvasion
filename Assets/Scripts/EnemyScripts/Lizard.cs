using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class Lizard : MonoBehaviour, IDamageable
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    
    [Header("Combat")]
    public DetectionZone attackZone;
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    
    [Header("Projectile Attack")]
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private Transform mouthPosition;
    [SerializeField] private GameObject projectilePrefab;
    private float attackTimer;
    
    [Header("Edge Detection")]
    [SerializeField] private bool useEdgeDetection = true;
    [SerializeField] private float groundCheckDistance = 1f;
    [SerializeField] private LayerMask groundLayer;
    
    private Rigidbody2D rb;
    private TouchingDirections touchingDirections;
    private Animator animator;
    private bool isDead = false;
    private Transform playerTransform;
    private bool playerInAttackRange = false;
    
    public enum WalkableDirection { Right, Left }
    
    private WalkableDirection _walkDirection;
    private Vector2 walkDirectionVector;
    
    public WalkableDirection WalkDirection
    {
        get { return _walkDirection; }
        set {
            if (_walkDirection != value)
            {
                if (value == WalkableDirection.Right)
                {
                    walkDirectionVector = Vector2.right;
                }
                else if (value == WalkableDirection.Left)
                {
                    walkDirectionVector = Vector2.left;
                }
                
                _walkDirection = value;
                
                float scaleX = Mathf.Abs(transform.localScale.x);
                if (_walkDirection == WalkableDirection.Right)
                {
                    transform.localScale = new Vector3(-scaleX, transform.localScale.y, transform.localScale.z);
                }
                else
                {
                    transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
                }
            }
        }
    }
    
    public bool _hasTarget = false;
    
    public bool HasTarget
    {
        get { return _hasTarget; }
        private set
        {
            _hasTarget = value;
            animator.SetBool(AnimationStrings.hasTarget, value);
        }
    }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        touchingDirections = GetComponent<TouchingDirections>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        
        walkDirectionVector = Vector2.right;
        
        float scaleX = Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(-scaleX, transform.localScale.y, transform.localScale.z);
        
        _walkDirection = WalkableDirection.Right;
    }
    
    void Start()
    {
        attackTimer = attackCooldown;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        animator.SetBool(AnimationStrings.isAlive, true);
        
        EnsureCorrectOrientation();
    }

    private void EnsureCorrectOrientation()
    {
        float scaleX = Mathf.Abs(transform.localScale.x);
        
        if (_walkDirection == WalkableDirection.Right)
        {
            transform.localScale = new Vector3(-scaleX, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
        }
    }
    
    private void ForceMove()
    {
        if (rb != null && !isDead)
        {
            EnsureCorrectOrientation();
            
            float xVelocity = walkSpeed * (WalkDirection == WalkableDirection.Right ? 1 : -1);
            rb.linearVelocity = new Vector2(xVelocity, rb.linearVelocity.y);
        }
    }
    
    void Update()
    {
        if (isDead) return;
        
        if (attackZone != null)
        {
            bool previousHasTarget = HasTarget;
            HasTarget = attackZone.detectedColliders.Count > 0;
        }
        else
        {
            HasTarget = false;
        }
        
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
        
        CheckPlayerAttackRange();
        
        if (HasTarget && playerInAttackRange && attackTimer <= 0)
        {
            if (!IsPlayingShootAnimation())
            {
                Attack();
                attackTimer = attackCooldown;
            }
        }
    }

    private bool IsPlayingShootAnimation()
    {
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName("Shoots") || 
                   stateInfo.IsName("Shoot") || 
                   stateInfo.IsName("Attack") || 
                   (stateInfo.normalizedTime < 1.0f && stateInfo.IsTag("AttackState"));
        }
        return false;
    }
    
    public void OnShootAnimationComplete()
    {
        attackTimer = attackCooldown;
        animator.ResetTrigger("attack");
        animator.SetBool("isAttacking", false);
    }
    
    private void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }
        
        if (HasTarget)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            
            if (playerTransform != null)
            {
                FacePlayer();
            }
            
            return;
        }
        
        Vector2 movementVector = walkDirectionVector;
        
        bool shouldFlip = false;
        
        if (touchingDirections != null)
        {
            shouldFlip = touchingDirections.IsOnWall;
            
            if (useEdgeDetection && touchingDirections.IsGrounded && !IsGroundAhead())
            {
                shouldFlip = true;
            }
        }
        
        if (shouldFlip)
        {
            FlipDirection();
        }
        
        if (rb != null)
        {
            float xVelocity = walkSpeed * movementVector.x;
            rb.linearVelocity = new Vector2(xVelocity, rb.linearVelocity.y);
        }
    }
    
    private void CheckPlayerAttackRange()
    {
        if (playerTransform == null || !HasTarget) 
        {
            playerInAttackRange = false;
            return;
        }
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        playerInAttackRange = distanceToPlayer <= attackRange;
    }
    
    private void FacePlayer()
    {
        if (playerTransform == null) return;
        
        if (playerTransform.position.x > transform.position.x && WalkDirection == WalkableDirection.Left)
        {
            WalkDirection = WalkableDirection.Right;
        }
        else if (playerTransform.position.x < transform.position.x && WalkDirection == WalkableDirection.Right)
        {
            WalkDirection = WalkableDirection.Left;
        }
    }
    
    private void Attack()
    {
        animator.SetTrigger("attack");
        animator.SetBool("isAttacking", true);
    }
    
    public void ShootProjectile()
    {
        if (isDead || projectilePrefab == null || mouthPosition == null || playerTransform == null) return;
        
        GameObject projectile = Instantiate(projectilePrefab, mouthPosition.position, Quaternion.identity);
        LizardProjectile projectileScript = projectile.GetComponent<LizardProjectile>();
        
        if (projectileScript != null)
        {
            Vector2 directionToPlayer = (playerTransform.position - mouthPosition.position);
            
            float maxVerticalAngle = 30f;
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            
            bool angleWasClamped = false;
            if (angle > maxVerticalAngle && angle < 180 - maxVerticalAngle)
            {
                angle = maxVerticalAngle;
                angleWasClamped = true;
            }
            else if (angle < -maxVerticalAngle && angle > -180 + maxVerticalAngle)
            {
                angle = -maxVerticalAngle;
                angleWasClamped = true;
            }
            
            if (angleWasClamped)
            {
                directionToPlayer = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad)
                );
            }
            
            if ((playerTransform.position.x < mouthPosition.position.x && directionToPlayer.x > 0) ||
                (playerTransform.position.x > mouthPosition.position.x && directionToPlayer.x < 0))
            {
                directionToPlayer.x = -directionToPlayer.x;
            }
            
            projectileScript.SetDirection(directionToPlayer);
        }
        
        Invoke("ResetToMoveState", 0.1f);
    }

    private void ResetToMoveState()
    {
        animator.ResetTrigger("attack");
        animator.SetBool("isAttacking", false);
    }
    
    private bool IsGroundAhead()
    {
        if (walkDirectionVector == null)
        {
            return true;
        }
        
        try
        {
            Vector2 rayStart = new Vector2(
                transform.position.x + (walkDirectionVector.x * 0.5f),
                transform.position.y - 0.1f);
            
            RaycastHit2D hit = Physics2D.Raycast(
                rayStart,
                Vector2.down,
                groundCheckDistance,
                groundLayer
            );
            
            return hit.collider != null;
        }
        catch (System.Exception)
        {
            return true;
        }
    }
    
    private float lastFlipTime = 0f;
    private float flipCooldown = 0.5f;

    private void FlipDirection()
    {
        if (Time.time - lastFlipTime < flipCooldown)
        {
            return;
        }
        
        WalkDirection = (WalkDirection == WalkableDirection.Right) ? 
                         WalkableDirection.Left : 
                         WalkableDirection.Right;
        
        EnsureCorrectOrientation();
        
        lastFlipTime = Time.time;
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("hit");
        }
    }
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        rb.linearVelocity = Vector2.zero;
        walkSpeed = 0f;
        
        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().enabled = false;
        }
        
        animator.SetBool(AnimationStrings.isAlive, false);
        
        float destroyDelay = GetDeathAnimationLength();
        
        Destroy(gameObject, destroyDelay);
    }
    
    private float GetDeathAnimationLength()
    {
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        
        if (clipInfo.Length > 0)
        {
            return clipInfo[0].clip.length;
        }
        
        return 1.0f;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}