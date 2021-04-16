using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
/*
public class Tooltip : MonoBehaviour
{
    [SerializeField] private RectTransform popupObject;
    [SerializeField] private GameObject tooltipPopupGameObject;
    [SerializeField] private GameObject equipmentPopup;
    [SerializeField] private GameObject consumablePopup;
    [SerializeField] private TextMeshProUGUI attackTMP;
    [SerializeField] private TextMeshProUGUI defenceTMP;
    [SerializeField] private TextMeshProUGUI healthTMP;
    [SerializeField] private TextMeshProUGUI strengthTMP;
    [SerializeField] private TextMeshProUGUI intellectTMP;
    [SerializeField] private TextMeshProUGUI agilityTMP;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float padding;

    private Canvas popupCanvas;

    private void Awake()
    {
        popupCanvas = tooltipPopupGameObject.GetComponent<Canvas>();
    }

    private void Update()
    {
        FollowCursor();
    }

    private void FollowCursor()
    {
        if (!tooltipPopupGameObject.activeSelf)
            return;

        Vector3 newPos = Input.mousePosition + offset;
        newPos.z = 0f;
        float rightEdgeToScreenEdgeDistance = Screen.width - (newPos.x + popupObject.rect.width * popupCanvas.scaleFactor / 2) - padding;
        float leftEdgeToScreenEdgeDistance = 0 - (newPos.x - popupObject.rect.width * popupCanvas.scaleFactor / 2) + padding;
        float topEdgeToScreenEdgeDistance = Screen.height - (newPos.y + popupObject.rect.height * popupCanvas.scaleFactor) - padding;

        if (rightEdgeToScreenEdgeDistance < 0)
        {
            newPos.x += rightEdgeToScreenEdgeDistance;
        }

        if (leftEdgeToScreenEdgeDistance > 0)
        {
            newPos.x += leftEdgeToScreenEdgeDistance;
        }

        if (topEdgeToScreenEdgeDistance < 0)
        {
            newPos.y += topEdgeToScreenEdgeDistance;
        }

        popupObject.transform.position = newPos;
    }

    // Show the tooltip
    public void DisplayInfo(Item item)
    {
        tooltipPopupGameObject.SetActive(true);
        
        // Show a different tooltip based on item type
        if (item.itemType == ItemType.Consumable)
            consumablePopup.SetActive(true);
        else
            equipmentPopup.SetActive(true);
        
        // Update the displayed info
        
        /*
         itemName (title)
         bonusAttack
         blockValue
         bonusHealth
         
         bonusStrength
         bonusIntellect
         bonusAgility
        */
        /*
         itemName (title)
         healthRestored
         manaRestored 
        */
       /* 
    }

    // Hide tooltip
    public void HideInfo()
    {
        tooltipPopupGameObject.SetActive(false);
        consumablePopup.SetActive(false);
        equipmentPopup.SetActive(false);
    }
}
*/