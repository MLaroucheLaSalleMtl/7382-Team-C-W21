using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NPC : Entity
{
    [Header("NPC Settings")]
    // public string animationName;
    public bool nameBarIsActive;
    public bool uiSlotsIsActive;
    public string npcTitle;
    
    [Header("NPC Type")]
    public bool dialogue;
    public bool trash;
    public bool skylandTeleporter;
    public bool darklandTeleporter;
    public bool startingHelper;

    [Header("NPC Story")]
    public string startingDialogue;
    
    [Header("NPC Dialogue 1")]
    public string dialogue1;

    [Header("NPC Dialogue 2")]
    public string dialogue2;
    
    [Header("Player Button 1")]
    public string button1;
    public bool button1IsActive;
    
    [Header("Player Button 2")]
    public string button2;
    public bool button2IsActive;
    
    [Header("Player Button 3")]
    public string button3;
    //public bool button3IsActive;

    [Header("NPC Store")]
    public List<int> store = new List<int>();
    public List<int> sellingPrices = new List<int>();
    public List<int> buyingPrices = new List<int>();

    void Start()
    {
        nameBar.SetActive(nameBarIsActive);
        nameBarTMP.text = myName;
    }
}
