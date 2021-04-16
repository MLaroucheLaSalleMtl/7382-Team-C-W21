using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tag to execute script inside the Editor
[ExecuteInEditMode]
public class HideUnderwear : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> piecesToHide;
        
    // When item is equipped
    void OnEnable()
    {
        foreach (var piece in piecesToHide)
        {
            piece.SetActive(false);
        }
    }

    // When item is unequipped
    void OnDisable()
    {
        foreach (var piece in piecesToHide)
        {
            piece.SetActive(true);
        }
    }
}
