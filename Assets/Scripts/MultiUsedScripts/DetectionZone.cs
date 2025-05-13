using UnityEngine;
using System.Collections.Generic;

public class DetectionZone : MonoBehaviour
{
    public List<Collider2D> detectedColliders = new List<Collider2D>();
    [SerializeField] private string targetTag = "Player"; 
    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            detectedColliders.Add(collision);
            Debug.Log($"DetectionZone: Detected {collision.name} with tag {targetTag}");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (detectedColliders.Contains(collision))
        {
            detectedColliders.Remove(collision);
            Debug.Log($"DetectionZone: {collision.name} exited");
        }
    }
}