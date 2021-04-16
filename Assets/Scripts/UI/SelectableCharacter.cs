using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SelectableCharacter : MonoBehaviour
{
    // Set by Network Manager when adding this script to the player prefab
    public int index = -1;
    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    void Update()
    {
        // If we're clicking, cast a raycast and see if we clicked on a player
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.collider == _collider)
                {
                    // Set selected character index
                    ((MMONetworkManager) NetworkManager.singleton).selectedCharacter = index;

                    // Change player name color
                    GetComponent<Player>().nameBarTMP.color = Color.blue;
                
                    Debug.Log("Clicked on character. index: " + index);
                }
            }
        }

        // Change player name color back to white if not selected anymore
        if (((MMONetworkManager) NetworkManager.singleton).selectedCharacter != index)
        {
            GetComponent<Player>().nameBarTMP.color = Color.white;
        }
    }
}
