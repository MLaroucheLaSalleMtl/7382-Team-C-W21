using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Entity : NetworkBehaviour
{
    // Reference to the Animator
    // Every derived class should set their own
    protected Animator animator;
    
    [Header("Entity UI Elements")]
    public GameObject nameBar;
    public TextMeshPro nameBarTMP;

    [Header("Entity Info")]
    public string myName;
    public float nameBarShowDistance = 100f;
    
    [Header("Buffs")] 
    public GameObject spiritualFocusPrefab;
    public GameObject strengthPrefab;

    [Header("Debuffs")] 
    public GameObject poisonPrefab;
    
    // Play Animation on Entity
    public virtual void PlayAnimation(string animationName)
    {
        animator.Play(animationName);
    }

    // Trigger Animation on Entity
    public virtual void TriggerAnimation(string animationName)
    {
        animator.SetTrigger(animationName);
    }

    private void Start()
    {
        if (nameBarTMP != null)
            nameBarTMP.text = myName;
    }
    
    private void Update()
    {
        // Make floating name always face the online player
        nameBar.transform.LookAt(Camera.main.transform);
        
        // Disable Name Bar if local player is far from the npc
        if (nameBar != null)
            nameBar.SetActive(!(Vector3.Distance(transform.position, Camera.main.transform.position) > nameBarShowDistance));
    }
}
