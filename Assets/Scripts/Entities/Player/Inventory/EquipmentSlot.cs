using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlot : MonoBehaviour
{
    // References
    private Player _player;
    private Inventory _inventory;
    
    public Item item;
    public Image itemIcon;
    public Button itemButton;
    public bool isEmpty = true;
    public bool isButtonReady = false;

    // Update is called once per frame
    void Update()
    {
        if (isEmpty)
        {
            itemIcon.gameObject.SetActive(false);
            itemButton.onClick.RemoveAllListeners();
            return;
        }

        // Show item icon in UI
        itemIcon.gameObject.SetActive(true);
        itemIcon.sprite = item.itemIcon;

        if (!isButtonReady)
        {
            itemButton.onClick.RemoveAllListeners();
            
            Debug.Log("equipment adding listener");
            itemButton.onClick.AddListener(delegate { _inventory.AddItem(_inventory.emptySlots.First(), item.itemId); });

            switch (item.itemType)
            {
                case ItemType.Consumable:
                    break;
                case ItemType.Arms:
                    itemButton.onClick.AddListener(delegate { _player.EquipArmsSlot(null); });
                    itemButton.onClick.AddListener(delegate { _player.equippedArms = null; });
                    break;
                case ItemType.Feet:
                    itemButton.onClick.AddListener(delegate { _player.EquipFeetSlot(null); });
                    itemButton.onClick.AddListener(delegate { _player.equippedFeet = null; });
                    break;
                case ItemType.Hands:
                    itemButton.onClick.AddListener(delegate { _player.EquipHandsSlot(null); });
                    itemButton.onClick.AddListener(delegate { _player.equippedHands = null; });
                    break;
                case ItemType.Head:
                    itemButton.onClick.AddListener(delegate { _player.EquipHeadSlot(null); });
                    itemButton.onClick.AddListener(delegate { _player.equippedHead = null; });
                    break;
                case ItemType.Legs:
                    itemButton.onClick.AddListener(delegate { _player.EquipLegsSlot(null); });
                    itemButton.onClick.AddListener(delegate { _player.equippedLegs = null; });
                    break;
                case ItemType.Shoulders:
                    itemButton.onClick.AddListener(delegate { _player.EquipShouldersSlot(null); });
                    itemButton.onClick.AddListener(delegate { _player.equippedShoulders = null; });
                    break;
                case ItemType.Waist:
                    itemButton.onClick.AddListener(delegate { _player.EquipWaistSlot(null); });
                    itemButton.onClick.AddListener(delegate { _player.equippedWaist = null; });
                    break;
                case ItemType.Weapon:
                    itemButton.onClick.AddListener(delegate { _player.EquipWeaponSlot(null); });
                    itemButton.onClick.AddListener(delegate { _player.equippedWeapon = null; });
                    break;
                case ItemType.Shield:
                    itemButton.onClick.AddListener(delegate { _player.EquipShieldSlot(null); });
                    itemButton.onClick.AddListener(delegate { _player.equippedShield = null; });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            isButtonReady = true;
        }
    }

    private void Awake()
    {
        // Cache the Button component
        itemButton = GetComponent<Button>();
        
        // Set reference to Inventory
        _inventory = GetComponentInParent<Inventory>();
        
        // Set reference to Player
        // TODO: Change to Entity
        _player = GetComponentInParent<Player>();
    }
}
