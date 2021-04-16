using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ItemType
{
    Consumable,
    Arms,
    Feet,
    Hands,
    Head,
    Legs,
    Shoulders,
    Waist,
    Weapon,
    Shield
}

public class Item : MonoBehaviour//, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Items Variables
    [Header("Item Info")]
    public int itemId;
    public string itemName;
    public ItemType itemType;
    public int itemLevel;
    public int levelRequirement;
    public Sprite itemIcon;

    [Header("Equipment Info")]
    public int equipmentDisplayID;
    public int bonusStrength;
    public int bonusIntellect;
    public int bonusAgility;

    [Header("Weapon Info")]
    public int bonusAttack;

    [Header("Shield Info")]
    public int bonusHealth;
    public int blockValue;

    [Header("Consumable Info")]
    public int healthRestored;
    public int manaRestored;

    [Header("Sell Info")]
    public float price;
}
