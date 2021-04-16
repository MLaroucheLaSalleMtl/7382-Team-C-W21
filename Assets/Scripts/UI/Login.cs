using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    public MMONetworkAuthenticator authenticator;
    [SerializeField] private MMONetworkManager manager;
    public Popup popupManager;

    // Inputs
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    // Login Menu
    [SerializeField] private GameObject loginMenu;

    // Character Selection Screen
    [SerializeField] private GameObject characterSelectionMenu;

    void Update()
    {
        // Only show the login menu if we're offline or authenticating
        if (manager.state == NetworkState.Offline || manager.state == NetworkState.Handshake)
        {
            loginMenu.SetActive(true);
            
            EnterUsername(usernameInput.text);
            EnterPassword(passwordInput.text);
            
            // Authentication status
            if (manager.IsConnecting())
            {
                popupManager.Show("Connecting...");
            }

            if (manager.state == NetworkState.Handshake)
                popupManager.Show("Authenticating...", "Cancel", () => popupManager.Hide());
        }
        else
        {
            // Hide login menu
            loginMenu.SetActive(false);
            
            // Hide popup window
            popupManager.Hide();
            
            // Show character selection screen
            characterSelectionMenu.SetActive(true);
        }
    }
    
    public void EnterUsername(string username)
    {
        // Sends the input to the authenticator
        authenticator.loginUsername = username;
    }

    public void EnterPassword(string password)
    {
        // Sends the input to the authenticator
        authenticator.loginPassword = password;
    }
}
