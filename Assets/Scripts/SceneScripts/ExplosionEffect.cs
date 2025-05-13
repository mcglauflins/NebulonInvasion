using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public void DestroyAfterAnimation()
    {
        Destroy(gameObject);
    }
}