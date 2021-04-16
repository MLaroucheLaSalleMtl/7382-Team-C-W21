using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HideHair : MonoBehaviour
{
    [SerializeField]
    private GameObject hairGameObject;
        
    // When item is equipped
    void OnEnable()
    {
        hairGameObject.SetActive(false);
    }

    // When item is unequipped
    void OnDisable()
    {
        hairGameObject.SetActive(true);
    }
}
