using UnityEngine;
using TMPro;
using System.Collections;

public class GuidanceMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float displayTime = 5f;
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float fadeOutTime = 0.5f;
    [SerializeField] private float slideDistance = 100f;
    
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 hiddenPosition;
    private Vector2 visiblePosition;
    private Coroutine displayRoutine;
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        canvasGroup.alpha = 0f;
        
        visiblePosition = rectTransform.anchoredPosition;
        hiddenPosition = new Vector2(rectTransform.anchoredPosition.x, 
                                    rectTransform.anchoredPosition.y - slideDistance);
        
        rectTransform.anchoredPosition = hiddenPosition;
    }
    
    public void ShowMessage(string message)
    {
        Debug.Log("GuidanceMessage.ShowMessage called with message: " + message);
        
        if (displayRoutine != null)
        {
            Debug.Log("Stopping existing display routine");
            StopCoroutine(displayRoutine);
        }
        
        if (messageText != null)
        {
            Debug.Log("Setting message text: " + message);
            messageText.text = message;
        }
        else
        {
            Debug.LogError("messageText is null!");
        }
        
        Debug.Log("Starting display routine");
        displayRoutine = StartCoroutine(DisplayMessageRoutine());
    }
    
    private IEnumerator DisplayMessageRoutine()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInTime)
        {
            float normalizedTime = elapsedTime / fadeInTime;
            canvasGroup.alpha = normalizedTime;
            
            rectTransform.anchoredPosition = Vector2.Lerp(
                hiddenPosition, visiblePosition, normalizedTime);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = visiblePosition;
        
        yield return new WaitForSeconds(displayTime);
        
        elapsedTime = 0f;
        
        while (elapsedTime < fadeOutTime)
        {
            float normalizedTime = elapsedTime / fadeOutTime;
            canvasGroup.alpha = 1f - normalizedTime;
            
            rectTransform.anchoredPosition = Vector2.Lerp(
                visiblePosition, hiddenPosition, normalizedTime);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = hiddenPosition;
        
        displayRoutine = null;
    }
    
    public void ForceShowMessage(string testMessage)
    {
        ShowMessage(testMessage);
    }
}