using UnityEngine;

public class Attack : MonoBehaviour
{
    public int attackDamage = 10;
    private void OnTriggerEnter2D(Collider2D collision)
    { 
    PlayerDamagable playerDamagable = collision.GetComponent<PlayerDamagable>();
    if (playerDamagable != null)
    {
        playerDamagable.TakeDamage(attackDamage);
        return;
    }
    
    Damagable damagable = collision.GetComponent<Damagable>();
    if (damagable != null)
    {
        damagable.Hit(attackDamage);
    }
    }
}