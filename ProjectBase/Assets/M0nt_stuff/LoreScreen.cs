using UnityEngine;
using UnityEngine.EventSystems;
public class LoreScreen : MonoBehaviour, IPointerClickHandler
{
    
    public void OnPointerClick(PointerEventData eventData)
    {
       // Debug.Log("Panel clicked!");
        this.gameObject.SetActive(false);
    }
}
