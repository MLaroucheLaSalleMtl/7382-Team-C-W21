using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Mirror;
using TMPro;

public class CreateCharacter : MonoBehaviour
{
    [SerializeField] private MMONetworkManager manager;
    [SerializeField] private GameObject characterCreationMenu;
    [SerializeField] private GameObject selectCharacterMenu;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_Dropdown genderDropdown;
    [SerializeField] private Button createButton;


    // Update is called once per frame
    void Update()
    {
        // Only update if this menu is active
        if (IsVisible())
        {
            // Are we in lobby?
            if (manager.state == NetworkState.Lobby)
            {
                // Show create character menu
                Show();
                
                // Send create character message
                createButton.interactable = manager.IsAllowedCharacterName(nameInput.text);
                createButton.onClick.SetListener(() => {
                    CreateCharacterMsg message = new CreateCharacterMsg {
                        name = nameInput.text,
                        gender = genderDropdown.options[genderDropdown.value].text,
                    };
                    NetworkClient.Send(message);
                    
                    Debug.Log("Name: " + nameInput.text + " Gender: " + genderDropdown.options[genderDropdown.value].text);
                    
                    // Message sent, hide menu
                    Hide();
                });

                // Show character selection menu
                selectCharacterMenu.SetActive(true);
            }
        }
    }
    
    public void Show()
    {
        characterCreationMenu.SetActive(true);
    }
    
    public void Hide()
    {
        characterCreationMenu.SetActive(false);
    }

    public bool IsVisible()
    {
        return characterCreationMenu.activeSelf;
    }
}
