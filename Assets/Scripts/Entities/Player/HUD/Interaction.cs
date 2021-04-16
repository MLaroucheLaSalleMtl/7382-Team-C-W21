using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Interaction : MonoBehaviour
{
    public Player player;

    // Player Interaction (Raycast) Variables 
    [SerializeField] private float interactionRange = 60f;
    private float secondsCount;
    private bool picking = false;
    private bool _receivedStartingItems = false;
    private Camera _camera;
    [SerializeField] private Text pressETxt;

    // Interactable items
    private GameObject _interactionGameObject;

    // Animation variables
    private Animator _doorAnimator;

    #region ClosePrefabsFunctions

    public void TimingDoor()
    {
        _interactionGameObject.SetActive(true);
    }

    public void TimingEatableItem()
    {
        _interactionGameObject.SetActive(true);
    }

    public void TimingUI()
    {
        _interactionGameObject.SetActive(true);
    }

    #endregion

    public void OnPick(InputAction.CallbackContext context)
    {
        picking = context.performed;
        Debug.Log(picking);
    }

    private void Awake()
    {
        _camera = GetComponentInParent<Camera>();
    }

    void Start()
    {
        // Set reference to Animator
        _doorAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // Center of the screen
        Vector3 rayOrigin = new Vector3(0.5f, 0.5f, 0f);

        // Interaction Ray
        Ray interactionRay = Camera.main.ViewportPointToRay(rayOrigin);

        // Layer Mask -- Everything but the LocalPlayer
        int layerMask = LayerMask.GetMask("LocalPlayer");
        layerMask = ~layerMask;

        // Debug Ray
        Debug.DrawRay(interactionRay.origin, interactionRay.direction * interactionRange, Color.red);

        // Hide press E text
        pressETxt.gameObject.SetActive(false);

        // Cast Raycast
        if (Physics.Raycast(interactionRay, out RaycastHit hit, interactionRange, layerMask))
        {
            #region Items

            // Checking if it's hitting the right item, if it is this item remove it
            if (hit.collider.CompareTag("Item"))
            {
                pressETxt.gameObject.SetActive(true);
                _interactionGameObject = hit.collider.gameObject;
                pressETxt.text = "Take '" + _interactionGameObject.GetComponent<Item>().itemName + "'";

                // TODO: Move to player input  
                if (Input.GetKeyDown(KeyCode.E))
                {
                    // Change this slotIndex to a stack
                    // And i can call a function that updates the list of empty slots whenever an item is added to or removed from the inventory
                    if (player.inventory.emptySlots.Any())
                    {
                        // Show message first otherwise the first element of the emptySlots will be changed before we print the message.
                        Debug.Log("Picked up: " + _interactionGameObject.GetComponent<Item>().itemId +
                                  ". Trying to place it on slot: " +
                                  player.inventory.emptySlots.First());

                        // Add the item to the inventory
                        player.inventory.AddItem(player.inventory.emptySlots.First(),
                            _interactionGameObject.GetComponent<Item>().itemId);

                        // Hide the item in game
                        _interactionGameObject.SetActive(false);

                        // Call function that respawns the item
                        Invoke(nameof(TimingEatableItem), 2.0f); // Cooldown
                    }
                    else
                        Debug.Log("Inventory is Full");
                }

                return;
            }

            #endregion

            #region NPC

            // NPC Dialogue System idea + example
            if (hit.collider.CompareTag("NPC"))
            {
                // Get all NPC info from the NPC
                _interactionGameObject = hit.collider.gameObject;
                NPC interactionNpc = _interactionGameObject.GetComponent<NPC>();
                MMONetworkManager networkManager = (MMONetworkManager) NetworkManager.singleton;
                pressETxt.text = "Talk to " + interactionNpc.myName;
                pressETxt.gameObject.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (!player.npcDialogueWindow.activeSelf)
                    {
                        player.ToggleNpcWindow();
                    }

                    // Clean buttons
                    player.button1.onClick.RemoveAllListeners();
                    player.button2.onClick.RemoveAllListeners();
                    player.buttonSlot1.onClick.RemoveAllListeners();
                    player.buttonSlot2.onClick.RemoveAllListeners();
                    player.buttonSlot3.onClick.RemoveAllListeners();

                    // SetActive(false) to who needs to be deactivated 
                    player.button1.gameObject.SetActive(interactionNpc.button1IsActive);
                    player.button2.gameObject.SetActive(interactionNpc.button2IsActive);
                    player.button1.GetComponentInChildren<Text>().text = interactionNpc.button1;
                    player.button2.GetComponentInChildren<Text>().text = interactionNpc.button2;
                    player.uiSlots.SetActive(interactionNpc.uiSlotsIsActive);

                    // Setting close button
                    // NPC button 3 == player closeButton :D
                    player.closeButton.GetComponentInChildren<Text>().text = interactionNpc.button3;
                    player.closeButton.onClick.RemoveAllListeners();
                    player.closeButton.onClick.AddListener(delegate { player.ToggleNpcWindow(); });

                    // Load NPC dialogue window info from NPC script
                    player.npcDialogueText.text = interactionNpc.startingDialogue;

                    // Load NPC Title info from NPC script
                    player.npcTitleText.text = interactionNpc.npcTitle;

                    // Setting teleportation NPC
                    if (interactionNpc.skylandTeleporter)
                    {
                        player.button1.onClick.AddListener(delegate { player.TeleportToSkyland(interactionNpc); });
                        player.button1.onClick.AddListener(delegate { player.npcDialogueWindow.SetActive(false); });
                    }

                    // Setting teleportation NPC
                    if (interactionNpc.darklandTeleporter)
                    {
                        player.button1.onClick.AddListener(delegate { player.TeleportToDarkland(interactionNpc); });
                        player.button1.onClick.AddListener(delegate { player.npcDialogueWindow.SetActive(false); });
                    }

                    // Setting UI slots
                    if (interactionNpc.uiSlotsIsActive)
                    {
                        // Impoverished Villagers are different, they do not have button 2 (Because they only sell for free).
                        if (interactionNpc.trash)
                            player.button2.gameObject.SetActive(false);
                        
                        // Add the right sprites to the slots
                        player.itemSlot1.sprite = networkManager.itemsDictionary[interactionNpc.store[0]].itemIcon;
                        player.itemSlot2.sprite = networkManager.itemsDictionary[interactionNpc.store[1]].itemIcon;
                        player.itemSlot3.sprite = networkManager.itemsDictionary[interactionNpc.store[2]].itemIcon;

                        // Set all sprites active
                        player.itemSlot1.gameObject.SetActive(true);
                        player.itemSlot2.gameObject.SetActive(true);
                        player.itemSlot3.gameObject.SetActive(true);

                        // if button 1 (selling button) was clicked
                        player.button1.onClick.AddListener(delegate { Selling(player, interactionNpc); });

                        // if button 2 (buying button) was clicked
                        player.button2.onClick.AddListener(delegate { Buying(player, interactionNpc); });
                    }

                    // Setting NPC that is trype dialogue
                    if (interactionNpc.dialogue)
                    {
                        // Setting button 1
                        if (interactionNpc.button1IsActive)
                        {
                            player.button1.onClick.AddListener(delegate
                            {
                                player.npcDialogueText.text = interactionNpc.dialogue1;
                            });
                            player.button1.onClick.AddListener(delegate
                            {
                                player.button1.gameObject.SetActive(false);
                            });
                            player.button1.onClick.AddListener(delegate
                            {
                                player.button2.gameObject.SetActive(false);
                            });
                        }

                        // Setting button 2
                        if (interactionNpc.button2IsActive)
                        {
                            player.button2.onClick.AddListener(delegate
                            {
                                player.npcDialogueText.text = interactionNpc.dialogue2;
                            });
                            player.button2.onClick.AddListener(delegate
                            {
                                player.button1.gameObject.SetActive(false);
                            });
                            player.button2.onClick.AddListener(delegate
                            {
                                player.button2.gameObject.SetActive(false);
                            });
                        }
                        
                        // Setting helper NPC which is also a dialogue NPC
                        if (interactionNpc.startingHelper)
                        {
                            if (!_receivedStartingItems)
                            {
                                _receivedStartingItems = true;
                                // Initial gold
                                player.inventory.AddGold(250);
                                // Initial Sword
                                player.inventory.AddItem(player.inventory.emptySlots.First(), 601);
                                // Initial potions
                                player.inventory.AddItem(player.inventory.emptySlots.First(), 2);
                                player.inventory.AddItem(player.inventory.emptySlots.First(), 3);
                                player.inventory.AddItem(player.inventory.emptySlots.First(), 3);
                                player.inventory.AddItem(player.inventory.emptySlots.First(), 3);
                            }
                        }
                    }
                }

                return;
            }

            #endregion

            #region EnemyLoot

            if (hit.collider.CompareTag("Lootable"))
            {
                pressETxt.text = "Press E to Loot";
                pressETxt.gameObject.SetActive(true);
                _interactionGameObject = hit.collider.gameObject;
                Enemy interactionEnemy = _interactionGameObject.GetComponent<Enemy>();

                if (Input.GetKeyDown(KeyCode.E))
                {
                    List<Item> loot = interactionEnemy.lootingSystem.GenerateLoot();

                    foreach (Item item in loot)
                    {
                        Debug.Log("Looted item: " + item.itemName);
                        player.inventory.AddItem(player.inventory.emptySlots.First(), item.itemId);
                        player.inventory.AddGold(interactionEnemy.lootingSystem.goldLoot);
                    }

                    if (interactionEnemy.lootingSystem.medusaCoinsIsActive)
                    {
                        player.AddEnemyCoins("Medusa");
                        player.medusaTMP.text = player.medusaCoins.ToString();
                    }

                    if (interactionEnemy.lootingSystem.chestMonsterCoinsIsActive)
                    {
                        player.AddEnemyCoins("ChestMonster");
                        player.chestMonsterTMP.text = player.chestMonsterCoins.ToString();
                    }

                    if (interactionEnemy.lootingSystem.minotaurCoinsIsActive)
                    {
                        player.AddEnemyCoins("Minotaur");
                        player.minotaurTMP.text = player.minotaurCoins.ToString();
                    }

                    if (interactionEnemy.lootingSystem.mushroomCoinsIsActive)
                    {
                        player.AddEnemyCoins("Mushroom");
                        player.mushroomTMP.text = player.mushroomCoins.ToString();
                    }

                    if (interactionEnemy.lootingSystem.rockMonsterCoinsIsActive)
                    {
                        player.AddEnemyCoins("RockMonster");
                        player.rockMonsterTMP.text = player.rockMonsterCoins.ToString();
                    }

                    if (interactionEnemy.lootingSystem.spiderCoinsIsActive)
                    {
                        player.AddEnemyCoins("Spider");
                        player.spiderTMP.text = player.spiderCoins.ToString();
                    }

                    if (interactionEnemy.lootingSystem.trollCoinsIsActive)
                    {
                        player.AddEnemyCoins("Troll");
                        player.trollTMP.text = player.trollCoins.ToString();
                    }

                    _interactionGameObject.tag = "Looted";
                }
            }

            #endregion

            #region Doors

            if (hit.collider.CompareTag("Door"))
            {
                pressETxt.gameObject.SetActive(true);
                pressETxt.text = "Press E to open the door";

                // TODO: Move to player input  
                if (Input.GetKeyDown(KeyCode.E))
                {
                    _interactionGameObject = hit.collider.gameObject;
                    _interactionGameObject.SetActive(false);
                    Debug.Log("You just open a door");
                    Invoke(nameof(TimingDoor), 3.0f); // Cooldown
                }

                return;
            }

            #endregion
        }
    }
    
    // Creating the function that will set Buying Mode
    void Buying(Player thisPlayer, NPC interactionNpc)
    {
        // Clean the Slot buttons after change the store mode
        player.buttonSlot1.onClick.RemoveAllListeners();
        player.buttonSlot2.onClick.RemoveAllListeners();
        player.buttonSlot3.onClick.RemoveAllListeners();
        
        // Set items price // It also could be change to a list(TODO after last demo)
        thisPlayer.itemSlotPriceText1.text = interactionNpc.buyingPrices[0].ToString();
        thisPlayer.itemSlotPriceText2.text = interactionNpc.buyingPrices[1].ToString();
        thisPlayer.itemSlotPriceText3.text = interactionNpc.buyingPrices[2].ToString();

        // Make a list of buttons. So, everything will really be dynamic 
        thisPlayer.buttonSlot1.onClick.AddListener(delegate
        {
            thisPlayer.BuyItem(interactionNpc.store[0], interactionNpc.buyingPrices[0]);
        });
        thisPlayer.buttonSlot2.onClick.AddListener(delegate
        {
            thisPlayer.BuyItem(interactionNpc.store[1], interactionNpc.buyingPrices[1]);
        });
        thisPlayer.buttonSlot3.onClick.AddListener(delegate
        {
            thisPlayer.BuyItem(interactionNpc.store[2], interactionNpc.buyingPrices[2]);
        });
    }

    // Creating the function that will set Selling Mode
    void Selling(Player thisPlayer, NPC interactionNpc)
    {
        // Clean the Slot buttons after change the store mode
        player.buttonSlot1.onClick.RemoveAllListeners();
        player.buttonSlot2.onClick.RemoveAllListeners();
        player.buttonSlot3.onClick.RemoveAllListeners();
        
        // Set items price // It also could be change to a list(TODO after last demo)
        thisPlayer.itemSlotPriceText1.text = interactionNpc.sellingPrices[0].ToString();
        thisPlayer.itemSlotPriceText2.text = interactionNpc.sellingPrices[1].ToString();
        thisPlayer.itemSlotPriceText3.text = interactionNpc.sellingPrices[2].ToString();
        
        thisPlayer.buttonSlot1.onClick.AddListener(delegate
        {
            thisPlayer.SellItem(interactionNpc.store[0], interactionNpc.sellingPrices[0]);
        });
        thisPlayer.buttonSlot2.onClick.AddListener(delegate
        {
            thisPlayer.SellItem(interactionNpc.store[1], interactionNpc.sellingPrices[1]);
        });
        thisPlayer.buttonSlot3.onClick.AddListener(delegate
        {
            thisPlayer.SellItem(interactionNpc.store[2], interactionNpc.sellingPrices[2]);
        });
    }
}
