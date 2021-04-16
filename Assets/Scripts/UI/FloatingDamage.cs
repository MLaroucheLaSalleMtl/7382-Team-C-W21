using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingDamage : MonoBehaviour
{
    // Disappear the FloatingText
    [SerializeField] private float secondToDestroy = 1f;
    [SerializeField] private TextMeshPro damage;
    
    void Start()
    {
        Destroy(gameObject, secondToDestroy);
        
    }
}
