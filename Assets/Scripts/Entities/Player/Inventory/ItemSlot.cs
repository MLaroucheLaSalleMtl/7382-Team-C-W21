using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    // References
    private Player _player;
    private Inventory _inventory;
    
    // Slot info
    public int slotId;
    public bool isEmpty = false;
    [SerializeField] private GameObject slotItemIcon;
    //[SerializeField] private Tooltip tooltip;
    
    // Item in slot
    public Item item;
    
    // Temp item (for equipment switching)
    private Item tempItem;
    
    private Button _slotItemButton;
    
    // Item icon
    private Image _image;

    public void PrepareButton(bool clearOnly)
    {
        // Clear any action already set to the button
        _slotItemButton.onClick.RemoveAllListeners();

        if (!clearOnly)
        {
            // Add onClick action according to item type
            switch (item.itemType)
            {
                case ItemType.Consumable:
                    _slotItemButton.onClick.AddListener(delegate { _player.health += item.healthRestored; });
                    _slotItemButton.onClick.AddListener(delegate { _player.mana += item.manaRestored; });
                    break;
                
                case ItemType.Arms:
                    // Cache the current equipped item
                    tempItem = _player.equippedArms;
                
                    // Equip new item
                    _slotItemButton.onClick.AddListener(delegate { _player.EquipArmsSlot(item); });
                
                    // Add old item to inventory
                    // We need 1 empty slot to perform the item swap
                    if (tempItem != null && _inventory.emptySlots.Count >= 1)
                        _slotItemButton.onClick.AddListener(delegate { _inventory.AddItem(_inventory.emptySlots.First(), tempItem.itemId); });
                    break;
                
                case ItemType.Feet:
                    // Cache the current equipped item
                    tempItem = _player.equippedFeet;
                    
                    // Equip new item
                    _slotItemButton.onClick.AddListener(delegate { _player.EquipFeetSlot(item); });
                    
                    // Add old item to inventory
                    // We need 1 empty slot to perform the item swap
                    if (tempItem != null && _inventory.emptySlots.Count >= 1)
                        _slotItemButton.onClick.AddListener(delegate { _inventory.AddItem(_inventory.emptySlots.First(), tempItem.itemId); });
                    break;
                
                case ItemType.Hands:
                    // Cache the current equipped item
                    tempItem = _player.equippedHands;
                    
                    // Equip new item
                    _slotItemButton.onClick.AddListener(delegate { _player.EquipHandsSlot(item); });
                    
                    // Add old item to inventory
                    // We need 1 empty slot to perform the item swap
                    if (tempItem != null && _inventory.emptySlots.Count >= 1)
                        _slotItemButton.onClick.AddListener(delegate { _inventory.AddItem(_inventory.emptySlots.First(), tempItem.itemId); });
                    break;
                
                case ItemType.Head:
                    // Cache the current equipped item
                    tempItem = _player.equippedHead;
                    
                    // Equip new item
                    _slotItemButton.onClick.AddListener(delegate { _player.EquipHeadSlot(item); });
                    
                    // Add old item to inventory
                    // We need 1 empty slot to perform the item swap
                    if (tempItem != null && _inventory.emptySlots.Count >= 1)
                        _slotItemButton.onClick.AddListener(delegate { _inventory.AddItem(_inventory.emptySlots.First(), tempItem.itemId); });
                    break;
                
                case ItemType.Legs:
                    // Cache the current equipped item
                    tempItem = _player.equippedLegs;
                    
                    // Equip new item
                    _slotItemButton.onClick.AddListener(delegate { _player.EquipLegsSlot(item); });
                    
                    // Add old item to inventory
                    // We need 1 empty slot to perform the item swap
                    if (tempItem != null && _inventory.emptySlots.Count >= 1)
                        _slotItemButton.onClick.AddListener(delegate { _inventory.AddItem(_inventory.emptySlots.First(), tempItem.itemId); });
                    break;
                
                case ItemType.Shoulders:
                    // Cache the current equipped item
                    tempItem = _player.equippedShoulders;
                    
                    // Equip new item
                    _slotItemButton.onClick.AddListener(delegate { _player.EquipShouldersSlot(item); });
                    
                    // Add old item to inventory
                    // We need 1 empty slot to perform the item swap
                    if (tempItem != null && _inventory.emptySlots.Count >= 1)
                        _slotItemButton.onClick.AddListener(delegate { _inventory.AddItem(_inventory.emptySlots.First(), tempItem.itemId); });
                    break;
                
                case ItemType.Waist:
                    // Cache the current equipped item
                    tempItem = _player.equippedWaist;
                    
                    // Equip new item
                    _slotItemButton.onClick.AddListener(delegate { _player.EquipWaistSlot(item); });
                    
                    // Add old item to inventory
                    // We need 1 empty slot to perform the item swap
                    if (tempItem != null && _inventory.emptySlots.Count >= 1)
                        _slotItemButton.onClick.AddListener(delegate { _inventory.AddItem(_inventory.emptySlots.First(), tempItem.itemId); });
                    break;
                
                case ItemType.Weapon:
                    tempItem = _player.equippedWeapon;
                    
                    // Equip new item
                    _slotItemButton.onClick.AddListener(delegate { _player.EquipWeaponSlot(item); });
                    
                    // Add old item to inventory
                    // We need 1 empty slot to perform the item swap
                    if (tempItem != null && _inventory.emptySlots.Count >= 1)
                        _slotItemButton.onClick.AddListener(delegate { _inventory.AddItem(_inventory.emptySlots.First(), tempItem.itemId); });
                    break;
                
                case ItemType.Shield:
                    // Cache the current equipped item
                    tempItem = _player.equippedShield;
                    
                    // Equip new item
                    _slotItemButton.onClick.AddListener(delegate { _player.EquipShieldSlot(item); });
                    
                    // Add old item to inventory
                    // We need 1 empty slot to perform the item swap
                    if (tempItem != null && _inventory.emptySlots.Count >= 1)
                        _slotItemButton.onClick.AddListener(delegate { _inventory.AddItem(_inventory.emptySlots.First(), tempItem.itemId); });
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            // Add onClick action to remove item from that slot
            _slotItemButton.onClick.AddListener(delegate { _inventory.RemoveItem(slotId); });
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        //tooltip.DisplayInfo(item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //tooltip.HideInfo();
    }

    private void Awake()
    {
        // Set reference to Inventory
        _inventory = GetComponentInParent<Inventory>();
        
        // Set reference to Player
        // TODO: Change to Entity
        _player = GetComponentInParent<Player>();
        
        // Cache the Image component
        if (!isEmpty)
            _image = slotItemIcon.GetComponent<Image>();

        // Cache the Button component
        _slotItemButton = GetComponent<Button>();
    }

    private void Update()
    {
        // If we have an item in this slot
        if (!isEmpty)
        {
            // Update the icon shown in that slot
            _image.sprite = item.itemIcon;
            //Debug.Log("Changing the sprite");

            // Show the icon
            slotItemIcon.SetActive(true);
            
            PrepareButton(false);

            return;
        }

        // Show the icon
        slotItemIcon.SetActive(false);
    }
}