using UnityEngine;

public class GuidanceTrigger : MonoBehaviour
{
    [SerializeField] private string guidanceMessage = "Press SPACE to jump";
    [SerializeField] private bool showOnce = true;
    [SerializeField] private float delayBeforeShow = 0.5f;
    [SerializeField] private GuidanceMessage guidanceUI;
    
    private bool hasTriggered = false;
    
    private void Start()
    {
        Debug.Log("GuidanceTrigger initialized with message: " + guidanceMessage);
        Debug.Log("GuidanceUI reference: " + (guidanceUI != null ? guidanceUI.name : "NULL"));
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger entered by: " + other.name + " with tag: " + other.tag);
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player tag detected!");
            
            if (!showOnce || !hasTriggered)
            {
                Debug.Log("Showing message: " + guidanceMessage);
                hasTriggered = true;
                
                if (guidanceUI != null)
                {
                    Debug.Log("GuidanceUI found, showing message with delay: " + delayBeforeShow);
                    if (delayBeforeShow > 0)
                        Invoke("ShowMessage", delayBeforeShow);
                    else
                        ShowMessage();
                }
                else
                {
                    Debug.LogError("GuidanceUI is null!");
                }
            }
            else
            {
                Debug.Log("Not showing message as it was already shown once");
            }
        }
        else
        {
            Debug.Log("Object does not have Player tag");
        }
    }
    
    private void ShowMessage()
    {
        Debug.Log("ShowMessage called");
        if (guidanceUI != null)
        {
            guidanceUI.ShowMessage(guidanceMessage);
        }
        else
        {
            Debug.LogError("GuidanceUI is null in ShowMessage!");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0.7f, 1f, 0.3f);
        Gizmos.DrawCube(transform.position, transform.lossyScale);
        
        Gizmos.color = new Color(0, 0.7f, 1f, 1f);
        Gizmos.DrawWireCube(transform.position, transform.lossyScale);
    }
}