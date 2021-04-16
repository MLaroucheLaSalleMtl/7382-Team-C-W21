using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEditor;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // Network Manager reference
    private readonly MMONetworkManager _networkManager = (MMONetworkManager) NetworkManager.singleton;

    public int gold;

    // List of slots inside main backpack
    public ItemSlot[] itemSlots;
    
    // List of empty slots
    [HideInInspector] public List<int> emptySlots = new List<int>();

    public void InitializeEmptyInventory()
    {
        // Initialize the inventory with empty items
        foreach (ItemSlot slot in itemSlots)
        {
            slot.gameObject.SetActive(true);
            slot.item = null;
            slot.isEmpty = true;
        }

        UpdateAvailableSlots();
    }

    private void UpdateAvailableSlots()
    {
        foreach (ItemSlot slot in itemSlots)
        {
            emptySlots.Remove(slot.slotId);
            
            if (slot.isEmpty)
                emptySlots.Add(slot.slotId);
        }
        
        // Sort the list so that we can always pick the first available slot to put an item in
        emptySlots.Sort();
    }
    
    // Function that will add an item to the mainBackpack
    public void AddItem(int slotIndex, int itemID)
    {
        if (itemID == 0)
        {
            itemSlots[slotIndex].item = null;
            itemSlots[slotIndex].isEmpty = true;
            UpdateAvailableSlots();

            return;
        }
        
        if (itemSlots[slotIndex].isEmpty)
        {
            // Set which item is inside that slot
            itemSlots[slotIndex].item = GetItemFromID(itemID);

            // Set the slot to NOT empty
            itemSlots[slotIndex].isEmpty = false;

            // Update the empty slots
            UpdateAvailableSlots();

            Debug.Log("Added an item to the slot " + slotIndex);
        }
        else
        {
            Debug.Log("ERROR: Slot " + slotIndex + " not empty.");
        }
    }

    public Item GetItem(int slotIndex)
    {
        return itemSlots[slotIndex].item == null ? null : itemSlots[slotIndex].item;
    }
    
    public int GetItemID(int slotIndex)
    {
        if (itemSlots[slotIndex].item == null)
            return 0;

        return itemSlots[slotIndex].item.itemId;
    }

    public Item GetItemFromID(int itemID)
    {
        return itemID == 0 ? null : _networkManager.itemsDictionary[itemID];
    }
    
    // Function that will remove an item to the mainBackpack
    public void RemoveItem(int slotIndex)
    {
        // item could not be found
        if (slotIndex == -1)
        {
            //transactionDone = false;
            return;
        }

        // Cache name of removed item
        string removedItemName = itemSlots[slotIndex].item.itemName;
        
        // Remove any item inside that slot
        itemSlots[slotIndex].item = null;
        
        // Set the slot to empty
        itemSlots[slotIndex].isEmpty = true;
        
        // Remove item slot button action
        itemSlots[slotIndex].PrepareButton(true);

        //transactionDone = true;
        
        // Update the empty slots
        UpdateAvailableSlots();
        
        Debug.Log("Removed: "+ removedItemName + "Slot: " + slotIndex);
    }
    
    // Function that will find the slotIndex by the ItemId
    public int GetSlotIndex(int itemId)
    {
        int slotIndex = -1;

        for (int i = 0; i < itemSlots.Length; i++)
        {
            var itemSlot = itemSlots[i];

            if (!itemSlot.isEmpty && itemSlot.item.itemId == itemId)
            {
                slotIndex = i;
            }
            // slotIndex = -1;
        }
        return slotIndex;
    }

    public void AddGold(int amount)
    {
        Debug.Log("Gold before: " + gold);
        gold += amount;
        Debug.Log("Added " + amount + " gold. Total: " + gold);
    }

    public bool RemoveGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            Debug.Log("Removed " + amount + " gold " + gold);
            return true;
        }
        Debug.Log("ERROR: gold < 0| Gold: " + gold);
        return false;
    }
    
    
}
