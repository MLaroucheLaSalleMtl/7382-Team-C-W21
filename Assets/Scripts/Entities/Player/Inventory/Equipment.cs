using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    
    
    public EquipmentSlot arms;
    public EquipmentSlot feet;
    public EquipmentSlot hands;
    public EquipmentSlot head;
    public EquipmentSlot legs;
    public EquipmentSlot shoulders;
    public EquipmentSlot waist;
    public EquipmentSlot weapon;
    public EquipmentSlot shield;

    public void UpdateSlot(EquipmentSlot equipmentSlot, Item item)
    {
        if (item != null)
        {
            equipmentSlot.item = item;
            equipmentSlot.isEmpty = false;
            equipmentSlot.isButtonReady = false;
            //UpdateSlotButton(equipmentSlot, item);
        }
        else
            equipmentSlot.isEmpty = true;
    }

    // public void UpdateSlotButton(EquipmentSlot equipmentSlot, Item item)
    // {
    //     if (item == null) return;
    //     
    //     equipmentSlot.itemButton.onClick.AddListener(delegate { _inventory.AddItem(_inventory.emptySlots.First(), item.itemId); });
    //     equipmentSlot.itemButton.onClick.AddListener(delegate { UpdateSlot(equipmentSlot, null); });
    // }

    private void Awake()
    {
        
    }
}
