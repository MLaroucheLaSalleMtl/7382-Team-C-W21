using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostOrLogin : MonoBehaviour
{
    [SerializeField] private GameObject loginButton;
    [SerializeField] private GameObject hostButton;
    
    // Start is called before the first frame update
    void Start()
    {
        // Enable Host Button 
        if (Application.isEditor)
            hostButton.SetActive(true);
        else
            loginButton.SetActive(true);
    }
}
