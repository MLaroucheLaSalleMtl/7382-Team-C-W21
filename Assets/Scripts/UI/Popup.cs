using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    [SerializeField] private GameObject popupWindow;
    [SerializeField] private TMP_Text messageTMP;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text closeButtonText;
    
    public void Show(string message)
    {
        // Shows the popup window with entered message
        popupWindow.SetActive(true);
        messageTMP.text = message;

        closeButtonText.text = "Close";
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.SetListener(Hide);
    }
    
    public void Show(string message, string buttonText, UnityAction onConfirm)
    {
        popupWindow.SetActive(true);
        messageTMP.text = message;

        closeButtonText.text = buttonText;
        closeButton.onClick.SetListener(onConfirm);
    }
    
    public void Hide()
    {
        popupWindow.SetActive(false);
    }

    public bool IsVisible()
    {
        return popupWindow.activeSelf;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Hide();
    }
}
