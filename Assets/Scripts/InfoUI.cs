using UnityEngine;
using UnityEngine.EventSystems;

public class InfoUI : MonoBehaviour, IPointerDownHandler
{
    // 터치 감지 
    public void OnPointerDown(PointerEventData eventData)
    {
        // 비활성
        gameObject.SetActive(false);
    }

}