using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Cinemachine;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Player : Entity
{
    // cache players to save lots of computations
    // (otherwise we'd have to iterate NetworkServer.objects all the time)
    // => on server: all online players
    // => on client: all observed players
    public static Dictionary<string, Player> onlinePlayers = new Dictionary<string, Player>();
    
    // Time of last combat
    [SyncVar] public double lastCombatTime;
    public double allowedLogoutTime => lastCombatTime + ((MMONetworkManager) NetworkManager.singleton).combatLogoutDelay;
    public double remainingLogoutTime => NetworkTime.time < allowedLogoutTime ? (allowedLogoutTime - NetworkTime.time) : 0;

    [Header("Modules")]
    public Interaction interaction;
    public Inventory inventory;
    public Equipment equipment;
    public PlayerMovement playerMovement;
    
    [Header("Player Info and Stats")]
    [HideInInspector] public string username = "";
    public string gender;
    public int level;
    public long experience;
    public int maxHealth;
    public int maxMana;
    public int health;
    public int mana;
    public int agility;
    public int intelligence;
    public int strength;
    public int gold;
    public int medusaCoins;
    public int chestMonsterCoins;
    public int minotaurCoins;
    public int mushroomCoins;
    public int rockMonsterCoins;
    public int spiderCoins;
    public int trollCoins;
    [HideInInspector] public int bonusHealth;
    [HideInInspector] public int bonusMana;
    [HideInInspector] public int bonusStrength;
    [HideInInspector] public int bonusIntelligence;
    [HideInInspector] public int bonusAgility;
    [HideInInspector] public int bonusAttack;
    public Vector3 position;
    public int attackValue;
    public int blockValue;
    public CinemachineFreeLook freeLookCamera;
    public static Player localPlayer; // localPlayer singleton because there should only be one local player.
    private bool _isNew = true;

    [Header("States")]
    [SerializeField] private bool isCursorVisible = false;
    public bool isAttacking;
    public bool isDead;
    public bool isPreview;
    public bool isNewPlayer = true;
    
    // Timers
    private float _attackTimer = 0;
    private float _regenerationTimer = 0;

    [Header("Starting Stats")]
    public int startingHealth;
    public int startingMana;
    public int startingAgility;
    public int startingIntelligence;
    public int startingStrength;

    [Header("Stats per level")] 
    public int experiencePerLevel;
    [SerializeField] private int healthPerLevel;
    [SerializeField] private int manaPerLevel;
    [SerializeField] private int agilityPerLevel;
    [SerializeField] private int intelligencePerLevel;
    [SerializeField] private int strengthPerLevel;

    [Header("UI Elements")]
    public Canvas canvas;
    [SerializeField] private TextMeshProUGUI agilityTMP;
    [SerializeField] private TextMeshProUGUI intelligenceTMP;
    [SerializeField] private TextMeshProUGUI strengthTMP;
    [SerializeField] private TextMeshProUGUI attackTMP;
    [SerializeField] private TextMeshProUGUI defenseTMP;
    [SerializeField] public TextMeshProUGUI goldTMP;
    
    [Header("UI Windows")]
    [SerializeField] private GameObject storyWindow;
    [SerializeField] private GameObject statusBarWindow;
    [SerializeField] private GameObject minimapWindow;
    [SerializeField] private GameObject inventoryWindow;
    [SerializeField] private GameObject equipmentWindow;
    [SerializeField] private GameObject deathWindow;
    [SerializeField] private GameObject menuWindow;
    public GameObject npcDialogueWindow;
    [SerializeField] private GameObject masterBookWindow;
    [SerializeField] private GameObject tutorialWindow;
    [SerializeField] private GameObject optionsWindow;
    public GameObject winWindow;
    [SerializeField] public GameObject keyMapping;

    [Header("Inventory")]
    public List<int> inventoryItems;

    [Header("NPC Dialogue")]
    public Button button1;
    public Button button2;
    public Button closeButton;
    public TextMeshProUGUI npcDialogueText;
    public TextMeshProUGUI npcTitleText;
    
    [Header("NPC Store")]
    public Button buttonSlot1;
    public Button buttonSlot2;
    public Button buttonSlot3;
    public Image itemSlot1;
    public Image itemSlot2;
    public Image itemSlot3;
    public GameObject uiSlots;
    public TextMeshProUGUI itemSlotPriceText1;
    public TextMeshProUGUI itemSlotPriceText2;
    public TextMeshProUGUI itemSlotPriceText3;

    [Header("Master Book")]
    public TextMeshProUGUI medusaTMP;
    public TextMeshProUGUI chestMonsterTMP;
    public TextMeshProUGUI minotaurTMP;
    public TextMeshProUGUI mushroomTMP;
    public TextMeshProUGUI rockMonsterTMP;
    public TextMeshProUGUI spiderTMP;
    public TextMeshProUGUI trollTMP;

    [Header("Minimap Camera")]
    // Minimap Camera
    [SerializeField] private Camera minimapCamera;
    
    [Header("Equipment Variables")]
    #region EquipmentVariables
    
    // Arms Slot variables
    [SyncVar(hook = nameof(OnArmsChanged))]
    public int activeArmsSynced = 0;
    private int _selectedArmsLocal = 0;
    public Item equippedArms;
    public GameObject[] armsArray;

    // Feet Slot variables
    [SyncVar(hook = nameof(OnFeetChanged))]
    public int activeFeetSynced = 0;
    private int _selectedFeetLocal = 0;
    public Item equippedFeet;
    public GameObject[] feetArray;

    // Hands Slot variables
    [SyncVar(hook = nameof(OnHandsChanged))]
    public int activeHandsSynced = 0;
    private int _selectedHandsLocal = 0;
    public Item equippedHands;
    public GameObject[] handsArray;

    // Head Slot variables
    [SyncVar(hook = nameof(OnHeadChanged))]
    public int activeHeadSynced = 0;
    private int _selectedHeadLocal = 0;
    public Item equippedHead;
    public GameObject[] headArray;

    // Legs Slot variables
    [SyncVar(hook = nameof(OnLegsChanged))]
    public int activeLegsSynced = 0;
    private int _selectedLegsLocal = 0;
    public Item equippedLegs;
    public GameObject[] legsArray;

    // Shoulders Slot variables
    [SyncVar(hook = nameof(OnShouldersChanged))]
    public int activeShouldersSynced = 0;
    private int _selectedShouldersLocal = 0;
    public Item equippedShoulders;
    public GameObject[] shouldersArray;

    // Waist Slot variables
    [SyncVar(hook = nameof(OnWaistChanged))]
    public int activeWaistSynced = 0;
    private int _selectedWaistLocal = 0;
    public Item equippedWaist;
    public GameObject[] waistArray;

    // Weapon Slot variables
    [SyncVar(hook = nameof(OnWeaponChanged))]
    public int activeWeaponSynced = 0;
    private int _selectedWeaponLocal = 0;
    public Item equippedWeapon;
    public GameObject[] weaponArray;
    
    // Shield Slot variables
    [SyncVar(hook = nameof(OnShieldChanged))]
    public int activeShieldSynced = 0;
    private int _selectedShieldLocal = 0;
    public Item equippedShield;
    public GameObject[] shieldArray;

    #endregion

    #region Methods

    // Win close button
    public void CloseButtonWin()
    {
        winWindow.SetActive(false);
        inventory.AddGold(1000);
    }
    // Teleport NPC actions
    public void TeleportToSkyland(NPC interactionNpc)
    {
        TeleportTo(((MMONetworkManager) NetworkManager.singleton).teleportToSkyland.transform.position);
        inventory.RemoveGold(200);
        // Debug.Log("Money Depois: " + inventory.gold);
    }
    public void TeleportToDarkland(NPC interactionNpc)
    {
        TeleportTo(((MMONetworkManager) NetworkManager.singleton).teleportToDarkland.transform.position);
        inventory.RemoveGold(100);
        // Debug.Log("Money Depois: " + inventory.gold);
    }
    
    // Store actions
    public void SellItem(int itemId, int itemSellingPrice)
    {
        if (inventory.GetSlotIndex(itemId) != -1)
        {
            inventory.AddGold(itemSellingPrice);
            inventory.RemoveItem(inventory.GetSlotIndex(itemId));
        }
    }

    public void BuyItem(int itemId, int itemBuyingPrice)
    {
        if (inventory.gold >= itemBuyingPrice)
        {
            inventory.AddItem(inventory.emptySlots.First(),itemId);
            inventory.RemoveGold(itemBuyingPrice);
        }
    }

    public void IsNotAttacking()
    {
        isAttacking = false;
    }

    // Decrease health
    public void DecreaseHealth(int damage)
    {
        health -= (damage - blockValue);
        TriggerAnimation("gotHit");
    }
    
    // Death
    private void Die()
    {
        // Trigger death animation
        TriggerAnimation("die");
        
        // Update tag
        gameObject.tag = "PlayerDead";
        
        // Set dead state
        isDead = true;
        
        // Disable movement
        playerMovement.canMove = false;
        
        // Enable death window
        deathWindow.SetActive(true);
        
        // Enable Cursor
        Cursor.visible = true;
    }

    private void EnableMovement()
    {
        playerMovement.canMove = true;
    }

    public void Logout()
    {
        MMONetworkManager manager = ((MMONetworkManager) NetworkManager.singleton);
        manager.state = NetworkState.Offline;
        
        if (NetworkServer.active)
        {
            // take the camera out of the local player so it doesn't get destroyed
            Camera mainCamera = Camera.main;
            if (mainCamera.transform.parent != null)
                mainCamera.transform.SetParent(null);

            mainCamera.transform.position = manager.menuCameraPosition.transform.position;
            mainCamera.transform.rotation = manager.menuCameraPosition.transform.rotation;

            manager.StopHost();
            
            manager.mainMenu.SetActive(true);
        }
        else
        {
            // take the camera out of the local player so it doesn't get destroyed
            Camera mainCamera = Camera.main;
            if (mainCamera.transform.parent != null)
                mainCamera.transform.SetParent(null);
            
            mainCamera.transform.position = manager.menuCameraPosition.transform.position;
            mainCamera.transform.rotation = manager.menuCameraPosition.transform.rotation;
            
            manager.StopClient();
            
            manager.mainMenu.SetActive(true);
        }
    }
    
    // Respawn
    public void Respawn()
    {
        // Teleport player to starting position
        TeleportTo(((MMONetworkManager) NetworkManager.singleton).startingPosition.transform.position);
        
        // Disable death window
        deathWindow.SetActive(false);
        
        // Set player tag
        gameObject.tag = "Player";
        
        // Unset dead state
        isDead = false;
        
        // Play "waking up" animation
        TriggerAnimation("idleBreak");
        
        // Enable player movement after animation is over
        Invoke(nameof(EnableMovement), 5f);
        
        // Reset health ana mana
        health = maxHealth;
        mana = maxMana;
        
        // Disable cursor
        Cursor.visible = false;
    }

    public bool IsUIOpen()
    {
        return equipmentWindow.activeSelf || inventoryWindow.activeSelf || npcDialogueWindow.activeSelf || 
               menuWindow.activeSelf || masterBookWindow.activeSelf || tutorialWindow.activeSelf || 
               winWindow.activeSelf || optionsWindow.activeSelf;
    }

    public void EscMenu()
    {
        // If we have any ui open
        if (IsUIOpen())
        {
            equipmentWindow.SetActive(false);
            inventoryWindow.SetActive(false);
            npcDialogueWindow.SetActive(false);
            menuWindow.SetActive(false);
            masterBookWindow.SetActive(false);
            tutorialWindow.SetActive(false);
            winWindow.SetActive(false);
            optionsWindow.SetActive(false);
            Cursor.visible = false;
            
            return;
        }
        
        Cursor.visible = true;
        menuWindow.SetActive(true);
    }
    
    // Toggle NPC dialogue window
    public void ToggleNpcWindow()
    {
        Cursor.visible = !isCursorVisible;
        
        npcDialogueWindow.SetActive(!npcDialogueWindow.activeSelf);
    }
    
    // Toggle Equipment Window
    public void ToggleEquipment()
    {
        Cursor.visible = !isCursorVisible;

        equipmentWindow.SetActive(!equipmentWindow.activeSelf);
    }
    
    // Toggle Inventory Window
    public void ToggleInventory()
    {
        Cursor.visible = !isCursorVisible;

        //equipmentWindow.SetActive(!equipmentWindow.activeSelf);
        inventoryWindow.SetActive(!inventoryWindow.activeSelf);
    }
	
    // Toggle MasterBook Window
    public void ToggleMasterBook()
    {
        Cursor.visible = !isCursorVisible;
        masterBookWindow.SetActive(!masterBookWindow.activeSelf);
    }

    // Toggle TutorialBook Window
    public void ToggleTutorialBook()
    {
        Cursor.visible = !isCursorVisible;
        tutorialWindow.SetActive(!tutorialWindow.activeSelf);
    }

    // Set the initial state of the UI
    // In the future we can store which windows the player had open
    // and re-open it once player logs in
    public void InitializeUI()
    {
        Debug.Log("Initializing UI...");
        
        canvas.gameObject.SetActive(true);
        
        storyWindow.SetActive(false);
        inventoryWindow.SetActive(false);
        equipmentWindow.SetActive(false);
        deathWindow.SetActive(false);
        npcDialogueWindow.SetActive(false);
        menuWindow.SetActive(false);
        ToggleTutorialBook();
        
        minimapWindow.SetActive(true);
        statusBarWindow.SetActive(true);
        keyMapping.SetActive(true);
    }
    
    // All clients need to know the new player position
    [ClientRpc]
    public void RpcTeleportTo(Vector3 destination)
    {
        // Set new position
        transform.position = destination;
    }
    
    public void TeleportTo(Vector3 destination)
    {
        // Set new position
        transform.position = destination;

        // Are we an object in the world?
        if (isServer)
        {
            // Let the clients know I moved
            RpcTeleportTo(destination);
        }
    }

    // Methods triggered when a SyncVar is modified

    #region EquipmentMethodsSyncVars

    // Change equipped Arms Slot function
    public void OnArmsChanged(int oldArms, int newArms)
    {
        // Disable old equipped item if new ID exists
        if (oldArms < armsArray.Length && armsArray[oldArms] != null)
        {
            armsArray[oldArms].SetActive(false);
        }

        // Enable new equipped item if new ID exists
        if (newArms < armsArray.Length && armsArray[newArms] != null)
        {
            armsArray[newArms].SetActive(true);
            
            // Update Equipment Window item
            equipment.UpdateSlot(equipment.arms, armsArray[newArms].GetComponent<Item>());
        }
    }

    // Change equipped Feet Slot function
    void OnFeetChanged(int oldFeet, int newFeet)
    {
        // Disable old equipped item if new ID exists
        if (oldFeet < feetArray.Length && feetArray[oldFeet] != null)
        {
            feetArray[oldFeet].SetActive(false);
        }

        // Enable new equipped item if new ID exists
        if (newFeet < feetArray.Length && feetArray[newFeet] != null)
        {
            feetArray[newFeet].SetActive(true);
            
            // Update Equipment Window item
            equipment.UpdateSlot(equipment.feet, feetArray[newFeet].GetComponent<Item>());
        }
    }

    // Change equipped Hands Slot function
    void OnHandsChanged(int oldHands, int newHands)
    {
        // Disable old equipped item if new ID exists
        if (oldHands < handsArray.Length && handsArray[oldHands] != null)
        {
            handsArray[oldHands].SetActive(false);
        }

        // Enable new equipped item if new ID exists
        if (newHands < handsArray.Length && handsArray[newHands] != null)
        {
            handsArray[newHands].SetActive(true);
            
            // Update Equipment Window item
            equipment.UpdateSlot(equipment.hands, handsArray[newHands].GetComponent<Item>());
        }
    }

    // Change equipped Head Slot function
    void OnHeadChanged(int oldHead, int newHead)
    {
        // Disable old equipped item if new ID exists
        if (oldHead < headArray.Length && headArray[oldHead] != null)
        {
            headArray[oldHead].SetActive(false);
        }

        // Enable new equipped item if new ID exists
        if (newHead < headArray.Length && headArray[newHead] != null)
        {
            headArray[newHead].SetActive(true);
            
            // Update Equipment Window item
            equipment.UpdateSlot(equipment.head, headArray[newHead].GetComponent<Item>());
        }
    }

    // Change equipped Legs Slot function
    void OnLegsChanged(int oldLegs, int newLegs)
    {
        // Disable old equipped item if new ID exists
        if (oldLegs < legsArray.Length && legsArray[oldLegs] != null)
        {
            legsArray[oldLegs].SetActive(false);
        }

        // Enable new equipped item if new ID exists
        if (newLegs < legsArray.Length && legsArray[newLegs] != null)
        {
            legsArray[newLegs].SetActive(true);
            
            // Update Equipment Window item
            equipment.UpdateSlot(equipment.legs, legsArray[newLegs].GetComponent<Item>());
        }
    }

    // Change equipped Shoulders Slot function
    void OnShouldersChanged(int oldShoulders, int newShoulders)
    {
        // Disable old equipped item if new ID exists
        if (oldShoulders < shouldersArray.Length && shouldersArray[oldShoulders] != null)
        {
            shouldersArray[oldShoulders].SetActive(false);
        }

        // Enable new equipped item if new ID exists
        if (newShoulders < shouldersArray.Length && shouldersArray[newShoulders] != null)
        {
            shouldersArray[newShoulders].SetActive(true);
            
            // Update Equipment Window item
            equipment.UpdateSlot(equipment.shoulders, shouldersArray[newShoulders].GetComponent<Item>());
        }
    }

    // Change equipped Waist Slot function
    void OnWaistChanged(int oldWaist, int newWaist)
    {
        // Disable old equipped item if new ID exists
        if (oldWaist < waistArray.Length && waistArray[oldWaist] != null)
        {
            waistArray[oldWaist].SetActive(false);
        }

        // Enable new equipped item if new ID exists
        if (newWaist < waistArray.Length && waistArray[newWaist] != null)
        {
            waistArray[newWaist].SetActive(true);
            
            // Update Equipment Window item
            equipment.UpdateSlot(equipment.waist, waistArray[newWaist].GetComponent<Item>());
        }
    }

    // Change equipped Weapon function
    void OnWeaponChanged(int oldWeapon, int newWeapon)
    {
        // Disable old equipped weapon if new ID exists
        if (oldWeapon < weaponArray.Length && weaponArray[oldWeapon] != null)
        {
            weaponArray[oldWeapon].SetActive(false);
        }

        // Enable new equipped weapon if new iD exists
        if (newWeapon < weaponArray.Length && weaponArray[newWeapon] != null)
        {
            weaponArray[newWeapon].SetActive(true);
            
            // Update Equipment Window item
            equipment.UpdateSlot(equipment.weapon, weaponArray[newWeapon].GetComponent<Item>());
        }
    }
    
    // Change equipped Shield function
    void OnShieldChanged(int oldShield, int newShield)
    {
        // Disable old equipped weapon if new ID exists
        if (oldShield < shieldArray.Length && shieldArray[oldShield] != null)
        {
            shieldArray[oldShield].SetActive(false);
        }

        // Enable new equipped weapon if new iD exists
        if (newShield < shieldArray.Length && shieldArray[newShield] != null)
        {
            shieldArray[newShield].SetActive(true);
            
            // Update Equipment Window item
            equipment.UpdateSlot(equipment.shield, shieldArray[newShield].GetComponent<Item>());
        }
    }

    #endregion

    // Methods to change equipped item

    #region EquipmentMethods

    // Equip new item on Arms Slot
    public void EquipArmsSlot(Item newItem)
    {
        int newItemID = 0;
            
        if (newItem != null)
            newItemID = newItem.equipmentDisplayID;
        
        // If the item doesn't have a valid Display ID, show no item
        if ((newItemID < 0) || (newItemID > armsArray.Length - 1))
        {
            Debug.LogError("equipmentDisplayID invalid for " + newItem.itemName);
            newItemID = 0;
            newItem = null;
        }
        
        // Load and apply bonus stats
        LoadItemBonusStats(equippedArms, newItem);
        
        // Update Equipment Window item
        equipment.UpdateSlot(equipment.arms, newItem);
        
        // Cache the equipped item
        equippedArms = newItem;

        // Update equipped item locally
        _selectedArmsLocal = newItemID;
        
        // Recalculate Stats
        RecalculateStats();

        // Notify the server to update equipped item
        CmdChangeActiveArms(newItemID);
    }

    // Equip new item on Feet Slot
    public void EquipFeetSlot(Item newItem)
    {
        int newItemID = 0;
            
        if (newItem != null)
            newItemID = newItem.equipmentDisplayID;
        
        // If the item doesn't have a valid Display ID, show no item
        if ((newItemID < 0) || (newItemID > feetArray.Length - 1))
        {
            Debug.LogError("equipmentDisplayID invalid for " + newItem.itemName);
            newItemID = 0;
            newItem = null;
        }
        
        // Load and apply bonus stats
        LoadItemBonusStats(equippedFeet, newItem);
        
        // Update Equipment Window item
        equipment.UpdateSlot(equipment.feet, newItem);
        
        // Cache the equipped item
        equippedFeet = newItem;

        // Update equipped item locally
        _selectedFeetLocal = newItemID;
        
        // Recalculate Stats
        RecalculateStats();

        // Notify the server to update equipped item
        CmdChangeActiveFeet(newItemID);
    }

    // Equip new item on Hands Slot
    public void EquipHandsSlot(Item newItem)
    {
        int newItemID = 0;
            
        if (newItem != null)
            newItemID = newItem.equipmentDisplayID;
        
        // If the item doesn't have a valid Display ID, show no item
        if ((newItemID < 0) || (newItemID > handsArray.Length - 1))
        {
            Debug.LogError("equipmentDisplayID invalid for " + newItem.itemName);
            newItemID = 0;
            newItem = null;
        }
        
        // Load and apply bonus stats
        LoadItemBonusStats(equippedHands, newItem);
        
        // Update Equipment Window item
        equipment.UpdateSlot(equipment.hands, newItem);
        
        // Cache the equipped item
        equippedHands = newItem;

        // Update equipped item locally
        _selectedHandsLocal = newItemID;
        
        // Recalculate Stats
        RecalculateStats();

        // Notify the server to update equipped item
        CmdChangeActiveHands(newItemID);
    }

    // Equip new item on Head Slot
    public void EquipHeadSlot(Item newItem)
    {
        int newItemID = 0;
            
        if (newItem != null)
            newItemID = newItem.equipmentDisplayID;
        
        // If the item doesn't have a valid Display ID, show no item
        if ((newItemID < 0) || (newItemID > headArray.Length - 1))
        {
            Debug.LogError("equipmentDisplayID invalid for " + newItem.itemName);
            newItemID = 0;
            newItem = null;
        }
        
        // Load and apply bonus stats
        LoadItemBonusStats(equippedHead, newItem);
        
        // Update Equipment Window item
        equipment.UpdateSlot(equipment.head, newItem);
        
        // Cache the equipped item
        equippedHead = newItem;

        // Update equipped item locally
        _selectedHeadLocal = newItemID;
        
        // Recalculate Stats
        RecalculateStats();

        // Notify the server to update equipped item
        CmdChangeActiveHead(newItemID);
    }

    // Equip new item on Legs Slot
    public void EquipLegsSlot(Item newItem)
    {
        int newItemID = 0;
            
        if (newItem != null)
            newItemID = newItem.equipmentDisplayID;
        
        // If the item doesn't have a valid Display ID, show no item
        if ((newItemID < 0) || (newItemID > legsArray.Length - 1))
        {
            Debug.LogError("equipmentDisplayID invalid for " + newItem.itemName);
            newItemID = 0;
            newItem = null;
        }
        
        // Load and apply bonus stats
        LoadItemBonusStats(equippedLegs, newItem);
        
        // Update Equipment Window item
        equipment.UpdateSlot(equipment.legs, newItem);
        
        // Cache the equipped item
        equippedLegs = newItem;

        // Update equipped item locally
        _selectedLegsLocal = newItemID;
        
        // Recalculate Stats
        RecalculateStats();

        // Notify the server to update equipped item
        CmdChangeActiveLegs(newItemID);
    }

    // Equip new item on Shoulders Slot
    public void EquipShouldersSlot(Item newItem)
    {
        int newItemID = 0;
            
        if (newItem != null)
            newItemID = newItem.equipmentDisplayID;
        
        // If the item doesn't have a valid Display ID, show no item
        if ((newItemID < 0) || (newItemID > shouldersArray.Length - 1))
        {
            Debug.LogError("equipmentDisplayID invalid for " + newItem.itemName);
            newItemID = 0;
            newItem = null;
        }
        
        // Load and apply bonus stats
        LoadItemBonusStats(equippedShoulders, newItem);
        
        // Update Equipment Window item
        equipment.UpdateSlot(equipment.shoulders, newItem);
        
        // Cache the equipped item
        equippedShoulders = newItem;

        // Update equipped item locally
        _selectedShouldersLocal = newItemID;
        
        // Recalculate Stats
        RecalculateStats();

        // Notify the server to update equipped item
        CmdChangeActiveShoulders(newItemID);
    }

    // Equip new item on Waist Slot
    public void EquipWaistSlot(Item newItem)
    {
        int newItemID = 0;
            
        if (newItem != null)
            newItemID = newItem.equipmentDisplayID;
        
        // If the item doesn't have a valid Display ID, show no item
        if ((newItemID < 0) || (newItemID > waistArray.Length - 1))
        {
            Debug.LogError("equipmentDisplayID invalid for " + newItem.itemName);
            newItemID = 0;
            newItem = null;
        }
        
        // Load and apply bonus stats
        LoadItemBonusStats(equippedWaist, newItem);
        
        // Update Equipment Window item
        equipment.UpdateSlot(equipment.waist, newItem);
        
        // Cache the equipped item
        equippedWaist = newItem;

        // Update equipped item locally
        _selectedWaistLocal = newItemID;
        
        // Recalculate Stats
        RecalculateStats();

        // Notify the server to update equipped item
        CmdChangeActiveWaist(newItemID);
    }

    // Equip new item on Weapon Slot
    public void EquipWeaponSlot(Item newItem)
    {
        int newItemID = 0;
            
        if (newItem != null)
            newItemID = newItem.equipmentDisplayID;
        
        // If the item doesn't have a valid Display ID, show no item
        if ((newItemID < 0) || (newItemID > weaponArray.Length - 1))
        {
            Debug.LogError("equipmentDisplayID invalid for " + newItem.itemName);
            newItemID = 0;
            newItem = null;
        }
        
        // Load and apply bonus stats
        LoadItemBonusStats(equippedWeapon, newItem);
        
        // Update Equipment Window item
        equipment.UpdateSlot(equipment.weapon, newItem);
        
        // Cache the equipped item
        equippedWeapon = newItem;

        // Update equipped item locally
        _selectedWeaponLocal = newItemID;
        
        // Recalculate Stats
        RecalculateStats();

        // Notify the server to update equipped item
        CmdChangeActiveWeapon(newItemID);
    }
    
    // Equip new item on Shield Slot
    public void EquipShieldSlot(Item newItem)
    {
        int newItemID = 0;
            
        if (newItem != null)
            newItemID = newItem.equipmentDisplayID;
        
        // If the item doesn't have a valid Display ID, show no item
        if ((newItemID < 0) || (newItemID > shieldArray.Length - 1))
        {
            Debug.LogError("equipmentDisplayID invalid for " + newItem.itemName);
            newItemID = 0;
            newItem = null;
        }
        
        // Load and apply bonus stats
        LoadItemBonusStats(equippedShield, newItem);
        
        // Update Equipment Window item
        equipment.UpdateSlot(equipment.shield, newItem);
        
        // Cache the equipped item
        equippedShield = newItem;

        // Update equipped item locally
        _selectedShieldLocal = newItemID;
        
        // Recalculate Stats
        RecalculateStats();

        // Notify the server to update equipped item
        CmdChangeActiveShield(newItemID);
    }

    #endregion
    
    // Methods related to Player Stats (health, mana, strength, attack...)

    #region StatsMethods

    public void LoadItemBonusStats(Item oldItem, Item newItem)
    {
        // Remove bonus stats from last item
        if (oldItem != null)
        {
            bonusHealth -= oldItem.bonusHealth;
            
            bonusAgility -= oldItem.bonusAgility;
            bonusIntelligence -= oldItem.bonusIntellect;
            bonusStrength -= oldItem.bonusStrength;
            
            bonusAttack -= oldItem.bonusAttack;
            blockValue -= oldItem.blockValue;
        }
        
        // Add bonus stats from item
        if (newItem != null)
        {
            bonusHealth += newItem.bonusHealth;
            
            bonusAgility += newItem.bonusAgility;
            bonusIntelligence += newItem.bonusIntellect;
            bonusStrength += newItem.bonusStrength;
            
            bonusAttack += newItem.bonusAttack;
            blockValue += newItem.blockValue;
        }
    }

    public void RecalculateStats()
    {
        // Stats
        strength = startingStrength + bonusStrength + (strengthPerLevel * (level - 1));
        intelligence = startingIntelligence + bonusIntelligence + (intelligencePerLevel * (level - 1));
        agility = startingAgility + bonusAgility + (agilityPerLevel * (level - 1));
        
        // Stats impact
        attackValue = strength + bonusAttack;
        bonusMana = intelligence * 5;
        
        // Health and Mana
        maxHealth = startingHealth + bonusHealth + (healthPerLevel * (level - 1));
        maxMana = startingMana + bonusMana + (manaPerLevel * (level - 1));
    }

    #endregion
    
    // Methods related to Spells, Buffs and Debuffs

    #region SpellsMethods

    // Buffs
    public void EndSpiritualFocus()
    {
        spiritualFocusPrefab.SetActive(false);
    }

    public void EndStrength()
    {
        strengthPrefab.SetActive(false);
    }
    
    // Debuffs
    public void EndPoison()
    {
        poisonPrefab.SetActive(false);
    }

    #endregion

    // Load player on Client
    public override void OnStartLocalPlayer()
    {
        // set localPlayer singleton
        localPlayer = this;

        // Change player layer (for Interaction)
        gameObject.layer = LayerMask.NameToLayer("LocalPlayer");

        // Disable the mouse
        Cursor.visible = false;
        
        // Load UI initial state
        InitializeUI();
        
        // Activate free look camera
        freeLookCamera.gameObject.SetActive(true);
        
        // Activate minimap camera
        minimapCamera.gameObject.SetActive(true);
        
        // Set camera position. TODO: Implement Camera Zoom with mouse wheel.
        Camera.main.transform.SetParent(transform);
        Camera.main.transform.localPosition = new Vector3(0, 2f, -2.5f);
        Camera.main.transform.localRotation = Quaternion.Euler(15, 0, 0);

        // Recalculate stats
        RecalculateStats();

        // Enable movement
        EnableMovement();
        
        // Initialize inventory
        inventory.InitializeEmptyInventory();
        inventory.AddGold(gold);
        
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            inventory.AddItem(i, inventoryItems[i]);
        }

    }
    
    #endregion

    #region Commands

    #region EquipmentCommands

    // Arms Slot
    [Command]
    public void CmdChangeActiveArms(int newIndex)
    {
        activeArmsSynced = newIndex;
    }

    // Feet Slot
    [Command]
    public void CmdChangeActiveFeet(int newIndex)
    {
        activeFeetSynced = newIndex;
    }

    // Hands Slot
    [Command]
    public void CmdChangeActiveHands(int newIndex)
    {
        activeHandsSynced = newIndex;
    }

    // Head Slot
    [Command]
    public void CmdChangeActiveHead(int newIndex)
    {
        activeHeadSynced = newIndex;
    }

    // Legs Slot
    [Command]
    public void CmdChangeActiveLegs(int newIndex)
    {
        activeLegsSynced = newIndex;
    }

    // Shoulders Slot
    [Command]
    public void CmdChangeActiveShoulders(int newIndex)
    {
        activeShouldersSynced = newIndex;
    }

    // Waist Slot
    [Command]
    public void CmdChangeActiveWaist(int newIndex)
    {
        activeWaistSynced = newIndex;
    }

    // Weapon Slot
    [Command]
    public void CmdChangeActiveWeapon(int newIndex)
    {
        // New selected weapon ID sent to server.
        // Server updates sync vars which handles it on all clients.
        activeWeaponSynced = newIndex;
    }
    
    // Shield Slot
    [Command]
    public void CmdChangeActiveShield(int newIndex)
    {
        // New selected weapon ID sent to server.
        // Server updates sync vars which handles it on all clients.
        activeShieldSynced = newIndex;
    }

    #endregion

    #endregion

    private void Awake()
    {
        // Make sure we don't have a player level 0
        if (level == 0)
            level = 1;
        
        // Set Animator reference
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        // Do nothing if not spawned
        if (!isServer && !isClient) 
            return;
        
        // Add player to lobby
        onlinePlayers[myName] = this;
    }
    
    public void AddEnemyCoins(string whichEnemy)
    {
        switch (whichEnemy)
        {
            case "Medusa":
                medusaCoins += 1;
                break;
            case "ChestMonster":
                chestMonsterCoins += 1;
                break;
            case "Minotaur":
                minotaurCoins += 1;
                break;
            case "Mushroom":
                mushroomCoins += 1;
                break;
            case "RockMonster":
                rockMonsterCoins += 1;
                break;
            case "Spider":
                spiderCoins += 1;
                break;
            case "Troll":
                trollCoins += 1;
                break;
        }
    }

    private void Update()
    {
        if (_isNew && isLocalPlayer)
        {
            TeleportTo(((MMONetworkManager) NetworkManager.singleton).startingPosition.transform.position);
            _isNew = false;
        }
        
        // If we're not local player (means its another player connected to the server)
        if (!isLocalPlayer)
        {
            freeLookCamera.gameObject.SetActive(false);
            minimapCamera.gameObject.SetActive(false);
            nameBar.SetActive(true);
            return;
        }

        // Hide floating info for own character. TODO: Move to settings
        //nameBar.SetActive(false);
        
        // Update Equipment UI stats
        agilityTMP.text = "Agility: " + agility;
        intelligenceTMP.text = "Intelligence: " + intelligence;
        strengthTMP.text = "Strength: " + strength;
        attackTMP.text = "Attack: " + attackValue;
        defenseTMP.text = "Defense: " + blockValue;
        
        // Update attack timer (to avoid spamming attacks)
        _attackTimer += Time.deltaTime;
        
        // Regeneration timer
        // TODO: Apple adds regeneration
        _regenerationTimer += Time.deltaTime;
        
        // Health > MaxHealth?????
        if (health > maxHealth)
            health = maxHealth;
        
        // Mana > MaxMana????
        if (mana > maxMana)
            mana = maxMana;
        
        // Mana < 0???
        if (mana < 0)
            mana = 0;

        // Death
        if (health < 0)
        {
            health = 0;
            
            if(!isDead)
                Die();
        }
        
        // Regeneration
        if (_regenerationTimer > 3f)
        {
            // Health Regen
            if (health < maxHealth)
                health += 2;

            // Mana Regen
            if (mana < maxMana)
                mana += 3;

            // Reset timer
            _regenerationTimer = 0;
        }
        
        // Levels
        if (experience >= experiencePerLevel)
        {
            level += 1;
            RecalculateStats();
            health = maxHealth;
            mana = maxMana;
            experience -= experiencePerLevel;
        }
        
        // Update gold UI in inventory
        goldTMP.text = inventory.gold.ToString();

        // Press ESC to open menu
        // Strength
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EscMenu();
        }
        
        // Base Attack
        // Pressing Fire1 (Left Click) will use the basic attack.
        // TODO: cant attack without a weapon
        if (Input.GetButtonDown("Fire1"))
        {
            if (_attackTimer > 1.2f && !IsUIOpen() && !isDead && mana >= 4)
            {
                //lastCombatTime = NetworkTime.time;
                isAttacking = true;
                TriggerAnimation("attack1Right");
                mana -= 4;
                _attackTimer = 0;
                Invoke(nameof(IsNotAttacking), 1.2f);
            }
        }

        // Toggle MasterBook
        if (Input.GetKeyDown(KeyCode.B))
            ToggleMasterBook();

            // Toggle TutorialBook
        if (Input.GetKeyDown(KeyCode.T))
            ToggleTutorialBook();

        // Toggle Inventory
        if (Input.GetKeyDown(KeyCode.I))
            ToggleInventory();
        
        // Toggle Equipment
        if (Input.GetKeyDown(KeyCode.P))
            ToggleEquipment();
        
        // Win case
        if (medusaCoins == 20 && chestMonsterCoins == 4 && minotaurCoins == 15 &&
            mushroomCoins == 50 && rockMonsterCoins == 20 && spiderCoins == 25 &&
            trollCoins == 15)
        {
            winWindow.SetActive(true);
            medusaCoins = 0;
            chestMonsterCoins = 0;
            minotaurCoins = 0;
            mushroomCoins = 0;
            rockMonsterCoins = 0;
            spiderCoins = 0;
            trollCoins = 0;
        }
    }

    private void LateUpdate()
    {
        // no need for animations on the server
        if (isClient)
        {
        }

        // update overlays in any case, except on server-only mode
        // (also update for character selection previews etc. then)
        if (!isServerOnly)
        {
            if (nameBarTMP != null)
            {
                nameBarTMP.text = myName;

                // find local player (null while in character selection)
                if (localPlayer != null)
                {
                    nameBar.SetActive(false);
                    //nameBarTMP.color = Color.green;
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Try to remove Player from onlinePlayers
        if (onlinePlayers.TryGetValue(base.myName, out Player entry) && entry == this)
            onlinePlayers.Remove(base.myName);
        
        // Do nothing if not spawned
        if (!isServer && !isClient) 
            return;
        
        // Unset the localPlayer
        if (isLocalPlayer)
            localPlayer = null;
    }
}
