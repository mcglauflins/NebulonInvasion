using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class Skeleton : MonoBehaviour, IDamageable
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    
    [Header("Combat")]
    public DetectionZone attackZone;
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    
    [Header("Edge Detection")]
    [SerializeField] private bool useEdgeDetection = true;
    [SerializeField] private float groundCheckDistance = 1f;
    [SerializeField] private LayerMask groundLayer;
    
    Rigidbody2D rb;
    TouchingDirections touchingDirections;
    Animator animator;
    
    public enum WalkableDirection { Right, Left }
    
    private WalkableDirection _walkDirection;
    private Vector2 walkDirectionVector = Vector2.right;
    
    public WalkableDirection WalkDirection
    {
        get { return _walkDirection; }
        set {
            if (_walkDirection != value)
            {
                gameObject.transform.localScale = new Vector2(gameObject.transform.localScale.x * -1, gameObject.transform.localScale.y);
                
                if (value == WalkableDirection.Right)
                {
                    walkDirectionVector = Vector2.right;
                }
                else if (value == WalkableDirection.Left)
                {
                    walkDirectionVector = Vector2.left;
                }
            }
            
            _walkDirection = value;
        }
    }
    
    public bool _hasTarget = false;
    private bool isDead = false;
    
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
        
        WalkDirection = WalkableDirection.Right;
    }
    
    void Start()
    {
        animator.SetBool(AnimationStrings.isAlive, true);
    }
    
    void Update()
    {
        HasTarget = attackZone.detectedColliders.Count > 0;
        
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            transform.rotation = Quaternion.identity;
        }
    }
    
    private void FixedUpdate()
    {
        if (isDead) return;
        
        bool shouldFlip = touchingDirections.IsOnWall;
        
        if (useEdgeDetection && touchingDirections.IsGrounded && !IsGroundAhead())
        {
            shouldFlip = true;
        }
        
        if (shouldFlip)
        {
            FlipDirection();
        }
        
        rb.linearVelocity = new Vector2(walkSpeed * walkDirectionVector.x, rb.linearVelocity.y);
    }
    
    private bool IsGroundAhead()
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
    
    private void FlipDirection()
    {
        if (WalkDirection == WalkableDirection.Right)
        {
            WalkDirection = WalkableDirection.Left;
        }
        else
        {
            WalkDirection = WalkableDirection.Right;
        }
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
}