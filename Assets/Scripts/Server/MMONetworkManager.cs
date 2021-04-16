using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public enum NetworkState { Offline, Handshake, Lobby, World }

public class MMONetworkManager : NetworkManager
{
    // current network manager state on client
    public NetworkState state = NetworkState.Offline;
    
    // <conn, account> dict for the lobby
    // (people that are still creating or selecting characters)
    public Dictionary<NetworkConnection, string> lobby = new Dictionary<NetworkConnection, string>();

    [Header("Popup Window")]
    public Popup popupWindow;

    [Header("Database")]
    [SerializeField] private DatabaseManager databaseManager;
    public int characterSlots = 5;
    public int characterNameMaxLength = 16;
    public float saveInterval = 60f; // How often we save player data to the database

    [Header("Logout")] 
    public float combatLogoutDelay = 5f;

    [Header("Starting position")] 
    public GameObject startingPosition;
    
    [Header("Main Menu")]
    public GameObject menuCameraPosition;
    public GameObject mainMenu;

    [Header("Character Selection")]
    public int selectedCharacter = -1;
    public Transform[] characterPosition;
    public Transform selectionCameraPosition;
    // List of available prefabs that can be instantiated as a player
    [HideInInspector] public List<Player> playerPrefabs = new List<Player>();
    // Available characters message
    [HideInInspector] public AvailableCharactersMsg availableCharactersMsg;
    
    [Header("NPC Teleport")]
    [SerializeField] public GameObject teleportToSkyland;
    [SerializeField] public GameObject teleportToDarkland;

    [Header("Items")]
    public List<Item> itemPrefabs = new List<Item>();
    public Dictionary<int, Item> itemsDictionary = new Dictionary<int, Item>();

    [Serializable]
    public class ServerInfo
    {
        public string name;
        public string ip;
    }

    public List<ServerInfo> serverList = new List<ServerInfo>()
    {
        new ServerInfo {name = "Home1", ip = "home1.holdrheim.com"}
    };

    // Set available player prefabs
    public List<Player> FindPlayerPrefabs()
    {
        // Filter out all Player prefabs from spawnPrefabs
        // because monsters are also supposed to be on that list
        // since they should also be spawned by the server
        List<Player> playerPrefabList = new List<Player>();
        foreach (GameObject prefab in spawnPrefabs)
        {
            Player player = prefab.GetComponent<Player>();
            if (player != null)
                playerPrefabList.Add(player);
        }
        return playerPrefabList;
    }
    
    // Set available Items
    public Dictionary<int, Item> FindItems()
    {
        itemsDictionary[0] = null;
        // Creates a dictionary out of all item prefabs
        foreach (Item item in itemPrefabs)
        {
            if (item != null)
                itemsDictionary[item.itemId] = item;
        }
        return itemsDictionary;
    }
    
    // Check name for not allowed characters
    public bool IsAllowedCharacterName(string characterName)
    {
        // not too long?
        // only contains letters, number and underscore and not empty (+)?
        // (important for database safety etc.)
        return characterName.Length <= 16 &&
               Regex.IsMatch(characterName, @"^[a-zA-Z0-9_]+$");
    }
    
    // Server Error Messages
    public void ServerSendError(NetworkConnection conn, string error, bool disconnect)
    {
        conn.Send(new ErrorMsg{text=error, causesDisconnect=disconnect});
    }

    // Client Error Messages
    void OnClientError(NetworkConnection conn, ErrorMsg message)
    {
        // Use print() so this is printed only in Unity Editor and not on server
        print("OnClientError: " + message.text);
        
        // Show a popup error message on client
        popupWindow.Show(message.text);

        // disconnect if it was an important network error
        // (this is needed because the login failure message doesn't disconnect
        //  the client immediately (only after timeout))
        if (message.causesDisconnect)
        {
            // Change network state back to offline so user can log in again
            state = NetworkState.Offline;
            
            // Disconnect the player
            conn.Disconnect();

            // also stop the host if running as host
            // (host shouldn't start server but disconnect client for invalid
            //  login, which would be pointless)
            if (NetworkServer.active) StopHost();
        }
    }
    
    public override void OnStartClient()
    {
        // Setup handlers
        NetworkClient.RegisterHandler<ErrorMsg>(OnClientError, false);
        NetworkClient.RegisterHandler<AvailableCharactersMsg>(OnClientCharactersAvailable);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        // connect to database
        DatabaseManager.singleton.Connect();

        NetworkServer.RegisterHandler<CreateCharacterMsg>(OnServerCreateCharacter);
        NetworkServer.RegisterHandler<SelectCharacterMsg>(OnServerSelectCharacter);
        NetworkServer.RegisterHandler<DeleteCharacterMsg>(OnServerDeleteCharacter);
        
        // Save player data every saveInterval
        InvokeRepeating(nameof(SavePlayers), saveInterval, saveInterval);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        // Do NOT call base function, otherwise client becomes "ready"
        // and we only want the client set as "ready" after selecting a character
        // otherwise the client can see online events
        //base.OnClientConnect(conn);
    }
    
    // called on the server if a client connects after successful auth
    public override void OnServerConnect(NetworkConnection conn)
    {
        // grab the account from the lobby
        string username = lobby[conn];

        // send necessary data to client
        conn.Send(MakeCharactersAvailableMessage(username));
    }
    
    // Called on the server when a client disconnects
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        // Players shouldn't be able to log out instantly to avoid combat
        float delay = 0;
        if (conn.identity != null)
        {
            Player player = conn.identity.GetComponent<Player>();
            delay = (float)player.remainingLogoutTime;
        }

        StartCoroutine(DoServerDisconnect(conn, delay));
    }
    
    // Called on the client if he disconnects
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        print("OnClientDisconnect called");

        // take the camera out of the local player so it doesn't get destroyed
        Camera mainCamera = Camera.main;
        if (mainCamera.transform.parent != null)
            mainCamera.transform.SetParent(null);

        // Show a disconnect popup
        popupWindow.Show("Disconnected.", "Quit", delegate { Quit(); });

        // call base function to guarantee proper functionality
        base.OnClientDisconnect(conn);

        // set state
        state = NetworkState.Offline;
    }
    
    // Client authentication: login
    public bool IsConnecting()
    {
        return NetworkClient.active && !ClientScene.ready;
    }

    IEnumerator<WaitForSeconds> DoServerDisconnect(NetworkConnection conn, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Save player if it is online
        // There is nothing to save it we're on character selection screen
        if (conn.identity != null)
        {
            conn.identity.GetComponent<Player>().position = conn.identity.GetComponent<Player>().transform.position;
            DatabaseManager.singleton.SaveCharacter(conn.identity.GetComponent<Player>(), false);
            print("Saved player: " + conn.identity.name);
        }
        
        // Remove logged in account
        lobby.Remove(conn);
        
        // Removes the player from the connection
        base.OnServerDisconnect(conn);
    }
    
    // The default OnClientSceneChanged sets the client as ready, we don't want that
    public override void OnClientSceneChanged(NetworkConnection conn) {}

    // helper function to make a CharactersAvailableMsg from all characters in
    // an account
    AvailableCharactersMsg MakeCharactersAvailableMessage(string username)
    {
        // load from database
        List<Player> characters = new List<Player>();
        foreach (string characterName in DatabaseManager.singleton.CharactersForUsername(username))
        {
            // Load character data
            DatabaseManager.characters row = DatabaseManager.singleton.LoadCharacter(characterName);
                
            // generate prefab name. in the future we can introduce new races if we want.
            string genderString = "Human " + char.ToUpper(row.gender[0]) + row.gender.Substring(1);
	            
            // instantiate based on the gender, always using first letter as uppercase. like: Male
            Player prefab = playerPrefabs.Find(p => p.name == genderString);
            
            GameObject go = Instantiate(prefab.gameObject);
            Player player = go.GetComponent<Player>();
            
            go.name             = row.name;
            player.myName       = row.name;
            player.username     = row.username;
            player.gender       = row.gender;
            player.position     = new Vector3(row.x, row.y, row.z);
            player.level        = row.level;
            player.experience   = row.experience;

            player.isNewPlayer = true;
            player.health = row.health;
            player.mana = row.mana;

            player.medusaCoins = row.medusaCoins;
            player.chestMonsterCoins = row.chestCoins;
            player.minotaurCoins = row.minotaurCoins;
            player.mushroomCoins = row.mushroomCoins;
            player.rockMonsterCoins = row.rockMonsterCoins;
            player.spiderCoins = row.spiderCoins;
            player.trollCoins = row.trollCoins;

            player.bonusHealth = row.bonusHealth;
            player.bonusMana = row.bonusMana;
            player.bonusAgility = row.bonusAgility;
            player.bonusIntelligence = row.bonusIntelligence;
            player.bonusStrength = row.bonusStrength;
            player.bonusAttack = row.bonusAttack;

            player.equippedArms           = (row.arms == 0 ? null : itemsDictionary[row.arms]);
            player.equippedFeet           = (row.feet == 0 ? null : itemsDictionary[row.feet]);
            player.equippedHands          = (row.hands == 0 ? null : itemsDictionary[row.hands]);
            player.equippedHead           = (row.head == 0 ? null : itemsDictionary[row.head]);
            player.equippedLegs           = (row.legs == 0 ? null : itemsDictionary[row.legs]);
            player.equippedShoulders      = (row.shoulders == 0 ? null : itemsDictionary[row.shoulders]);
            player.equippedWaist          = (row.waist == 0 ? null : itemsDictionary[row.waist]);
            player.equippedWeapon         = (row.weapon == 0 ? null : itemsDictionary[row.weapon]);
            player.equippedShield         = (row.shield == 0 ? null : itemsDictionary[row.shield]);

            player.activeArmsSynced       = row.arms == 0 ? 0 : player.equippedArms.equipmentDisplayID;
            player.activeFeetSynced       = row.feet == 0 ? 0 : player.equippedFeet.equipmentDisplayID;
            player.activeHandsSynced      = row.hands == 0 ? 0 : player.equippedHands.equipmentDisplayID;
            player.activeHeadSynced       = row.head == 0 ? 0 : player.equippedHead.equipmentDisplayID;
            player.activeLegsSynced       = row.legs == 0 ? 0 : player.equippedLegs.equipmentDisplayID;
            player.activeShouldersSynced  = row.shoulders == 0 ? 0 : player.equippedShoulders.equipmentDisplayID;
            player.activeWaistSynced      = row.waist == 0 ? 0 : player.equippedWaist.equipmentDisplayID;
            player.activeWeaponSynced     = row.weapon == 0 ? 0 : player.equippedWeapon.equipmentDisplayID;
            player.activeShieldSynced     = row.shield == 0 ? 0 : player.equippedShield.equipmentDisplayID;
            player.inventory.InitializeEmptyInventory();

            characters.Add(player);
        }

        // construct the message
        AvailableCharactersMsg message = new AvailableCharactersMsg();
        message.Load(characters);

        // destroy the temporary players again and return the result
        characters.ForEach(player => Destroy(player.gameObject));
        return message;
    }
    
    /////////////////////////
    // Character Selection //
    /////////////////////////
    void LoadPreview(GameObject prefab, Transform location, int selectionIndex, AvailableCharactersMsg.CharacterPreview character)
    {
        // instantiate the prefab
        GameObject preview = Instantiate(prefab.gameObject, location.position, location.rotation);
        preview.transform.parent = location;
        Player player = preview.GetComponent<Player>();

        // assign basic preview values like name and equipment
        player.myName = character.name;
        player.gender = character.gender;
        player.activeArmsSynced = character.arms;
        player.activeFeetSynced = character.feet;
        player.activeHandsSynced = character.hands;
        player.activeHeadSynced = character.head;
        player.activeLegsSynced = character.legs;
        player.activeShouldersSynced = character.shoulders;
        player.activeWaistSynced = character.waist;
        player.activeWeaponSynced = character.weapon;
        player.activeShieldSynced = character.shield;
        
        // Show floating player info
        player.nameBar.SetActive(true);
        player.nameBarTMP.text = player.myName;

        // add selection script
        preview.AddComponent<SelectableCharacter>();
        preview.GetComponent<SelectableCharacter>().index = selectionIndex;
    }

    public void ClearPreviews()
    {
        selectedCharacter = -1;
        
        foreach (Transform location in characterPosition)
        {
            if (location.childCount > 0)
                Destroy(location.GetChild(0).gameObject);
        }
    }

    void OnClientCharactersAvailable(NetworkConnection conn, AvailableCharactersMsg message)
    {
        availableCharactersMsg = message;
        print("Available characters:" + availableCharactersMsg.characters.Length);

        // set state
        state = NetworkState.Lobby;

        // clear previous previews in any case
        ClearPreviews();

        // load previews for 3D character selection
        for (int i = 0; i < availableCharactersMsg.characters.Length; ++i)
        {
            AvailableCharactersMsg.CharacterPreview character = availableCharactersMsg.characters[i];
            
            // generate prefab name. in the future we can introduce new races if we want.
            string genderString = "Human " + char.ToUpper(character.gender[0]) + character.gender.Substring(1);
            // instantiate based on the gender, always using first letter as uppercase. like: Male
            Player prefab = playerPrefabs.Find(p => p.name == genderString);
            
            if (prefab != null)
                LoadPreview(prefab.gameObject, characterPosition[i], i, character);
            else
            {
                print("WARNING: Character Selection: couldn't find prefab " + genderString);
            }
        }

        // Move camera to Character Selection position
        Camera.main.transform.position = selectionCameraPosition.position;
        Camera.main.transform.rotation = selectionCameraPosition.rotation;
    }

    void OnServerCreateCharacter(NetworkConnection conn, CreateCharacterMsg message)
    {
        // While we're in lobby (character selection screen)
        if (lobby.ContainsKey(conn))
        {
            // Is the character name allowed? (not too long or with unwanted characters)
            if (IsAllowedCharacterName(message.name))
            {
                // If a character with that name doesn't exist yet
                string username = lobby[conn];
                if (!DatabaseManager.singleton.CharacterExists(message.name))
                {
                    // Have we exceeded the amount of characterSlots?
                    if (DatabaseManager.singleton.CharactersForUsername(username).Count < characterSlots)
                    {
                        // Valid gender?
                        if (message.gender == "Male" || message.gender == "Female")
                        {
                            // generate prefab name. in the future we can introduce new races if we want.
                            string genderString = "Human " + message.gender;
                            Player prefab = playerPrefabs.Find(p => p.name == genderString);
                            Player player = CreateCharacter(prefab.gameObject, message.name, message.gender, username);

                            // save the player
                            DatabaseManager.singleton.SaveCharacter(player, false, false);
                            Destroy(player.gameObject);
                            
                            // send available characters list again, causing
                            // the client to switch to the character
                            // selection scene again
                            conn.Send(MakeCharactersAvailableMessage(username));
                        }
                        else
                        {
                            print("Invalid character gender: " + message.gender);
                            ServerSendError(conn, "Invalid character gender.", false);
                        }
                    }
                    else
                    {
                        print("Character limit reached: " + message.name);
                        ServerSendError(conn, "Character limit reached.", false);
                    }
                }
                else
                {
                    print("Character name already exists: " + message.name);
                    ServerSendError(conn, "Character with that name already exists.", false);
                }
            }
            else
            {
                print("Character name not allowed: " + message.name);
                ServerSendError(conn, "Name contains invalid characters. (a-Z and spaces)", false);
            }
        }
        else
        {
            Debug.Log("Create Character Error: not in lobby"); //<- don't show on live server
            ServerSendError(conn, "Create Character Error: not in lobby", true);
        }
    }

    Player CreateCharacter(GameObject genderPrefab, string characterName, string gender, string username)
    {
        Player player = Instantiate(genderPrefab).GetComponent<Player>();
        player.name = characterName;
        player.myName = characterName;
        player.username = username;
        player.gender = gender;
        player.level = 1;
        player.experience = 0;
        player.health = 100;
        player.mana = 50;
        player.position = startingPosition.transform.position;
        player.isNewPlayer = true;

        return player;
    }

    void OnServerSelectCharacter(NetworkConnection conn, SelectCharacterMsg message)
    {
        // While we're in lobby (character selection screen)
        if (lobby.ContainsKey(conn))
        {
            // Read the username from lobby
            string username = lobby[conn];
            List<string> characters = DatabaseManager.singleton.CharactersForUsername(username);
            
            // Validate character index
            if (0 <= message.index && message.index < characters.Count)
            {
                // Load character data
                DatabaseManager.characters row = DatabaseManager.singleton.LoadCharacter(characters[message.index]);
                
                // generate prefab name. in the future we can introduce new races if we want.
	            string genderString = "Human " + char.ToUpper(row.gender[0]) + row.gender.Substring(1);
	            
                // instantiate based on the gender, always using first letter as uppercase. like: Male
                Player prefab = playerPrefabs.Find(p => p.name == genderString);
            
                GameObject go = Instantiate(prefab.gameObject);
                Player player = go.GetComponent<Player>();

                go.name             = row.name;
                player.myName       = row.name;
                player.username     = row.username;
                player.gender       = row.gender;
                player.position     = new Vector3(row.x, row.y, row.z);
                player.level        = row.level;
                player.experience   = row.experience;

                player.health = row.health;
                player.mana = row.mana;
                
                player.isNewPlayer = true;

                player.medusaCoins = row.medusaCoins;
                player.chestMonsterCoins = row.chestCoins;
                player.minotaurCoins = row.minotaurCoins;
                player.mushroomCoins = row.mushroomCoins;
                player.rockMonsterCoins = row.rockMonsterCoins;
                player.spiderCoins = row.spiderCoins;
                player.trollCoins = row.trollCoins;

                player.bonusHealth = row.bonusHealth;
                player.bonusMana = row.bonusMana;
                player.bonusAgility = row.bonusAgility;
                player.bonusIntelligence = row.bonusIntelligence;
                player.bonusStrength = row.bonusStrength;
                player.bonusAttack = row.bonusAttack;

                player.equippedArms           = (row.arms == 0 ? null : itemsDictionary[row.arms]);
                player.equippedFeet           = (row.feet == 0 ? null : itemsDictionary[row.feet]);
                player.equippedHands          = (row.hands == 0 ? null : itemsDictionary[row.hands]);
                player.equippedHead           = (row.head == 0 ? null : itemsDictionary[row.head]);
                player.equippedLegs           = (row.legs == 0 ? null : itemsDictionary[row.legs]);
                player.equippedShoulders      = (row.shoulders == 0 ? null : itemsDictionary[row.shoulders]);
                player.equippedWaist          = (row.waist == 0 ? null : itemsDictionary[row.waist]);
                player.equippedWeapon         = (row.weapon == 0 ? null : itemsDictionary[row.weapon]);
                player.equippedShield         = (row.shield == 0 ? null : itemsDictionary[row.shield]);

                player.activeArmsSynced       = row.arms == 0 ? 0 : player.equippedArms.equipmentDisplayID;
                player.activeFeetSynced       = row.feet == 0 ? 0 : player.equippedFeet.equipmentDisplayID;
                player.activeHandsSynced      = row.hands == 0 ? 0 : player.equippedHands.equipmentDisplayID;
                player.activeHeadSynced       = row.head == 0 ? 0 : player.equippedHead.equipmentDisplayID;
                player.activeLegsSynced       = row.legs == 0 ? 0 : player.equippedLegs.equipmentDisplayID;
                player.activeShouldersSynced  = row.shoulders == 0 ? 0 : player.equippedShoulders.equipmentDisplayID;
                player.activeWaistSynced      = row.waist == 0 ? 0 : player.equippedWaist.equipmentDisplayID;
                player.activeWeaponSynced     = row.weapon == 0 ? 0 : player.equippedWeapon.equipmentDisplayID;
                player.activeShieldSynced     = row.shield == 0 ? 0 : player.equippedShield.equipmentDisplayID;
                
                player.gold = row.gold;
                player.inventoryItems.Add(row.slot1);
                player.inventoryItems.Add(row.slot2);
                player.inventoryItems.Add(row.slot3);
                player.inventoryItems.Add(row.slot4);
                player.inventoryItems.Add(row.slot5);
                player.inventoryItems.Add(row.slot6);
                player.inventoryItems.Add(row.slot7);
                player.inventoryItems.Add(row.slot8);
                player.inventoryItems.Add(row.slot9);
                player.inventoryItems.Add(row.slot10);
                player.inventoryItems.Add(row.slot11);
                player.inventoryItems.Add(row.slot12);
                player.inventoryItems.Add(row.slot13);
                player.inventoryItems.Add(row.slot14);
                player.inventoryItems.Add(row.slot15);
                player.inventoryItems.Add(row.slot16);
                player.inventoryItems.Add(row.slot17);
                player.inventoryItems.Add(row.slot18);
                player.inventoryItems.Add(row.slot19);
                player.inventoryItems.Add(row.slot20);
                player.inventoryItems.Add(row.slot21);
                player.inventoryItems.Add(row.slot22);
                player.inventoryItems.Add(row.slot23);
                player.inventoryItems.Add(row.slot24);

                go.transform.position = startingPosition.transform.position;
                //go.transform.position = player.position;

                // Add to client
                NetworkServer.AddPlayerForConnection(conn, go);

                // Remove player from lobby
                lobby.Remove(conn);
            }
            else
            {
                print("Invalid character index: " + message.index + " for username " + username);
                ServerSendError(conn, "Invalid character index", false);
            }
        }
        else
        {
            print("Selecting a character that is not in lobby " + conn);
            ServerSendError(conn, "Selecting a character that is not in lobby", false);
        }
    }

    void OnServerDeleteCharacter(NetworkConnection conn, DeleteCharacterMsg message)
    {
        // While we're in lobby (character selection screen)
        if (lobby.ContainsKey(conn))
        {
            string username = lobby[conn];
            List<string> characters = DatabaseManager.singleton.CharactersForUsername(username);
            
            // Validate character index
            if (0 <= message.index && message.index < characters.Count)
            {
                // Delete the character
                print("Deleting character: " + characters[message.index]);
                DatabaseManager.singleton.DeleteCharacter(characters[message.index]);
                
                // Send the new character list to the client
                conn.Send(MakeCharactersAvailableMessage(username));
            }
            else
            {
                print("Invalid character index: " + message.index + " for username " + username);
                ServerSendError(conn, "Invalid character index", false);
            }
        }
        else
        {
            print("Deleting a character that is not in lobby " + conn);
            ServerSendError(conn, "Deleting a character that is not in lobby", false);
        }
    }

    // Player saving
    void SavePlayers()
    {
        DatabaseManager.singleton.SaveManyCharacters(Player.onlinePlayers.Values);
        if (Player.onlinePlayers.Count > 0)
        {
            print("Saved " + Player.onlinePlayers.Count + " players.");
        }
    }
    
    // Quit function
    public static void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else  
        Application.Quit();
#endif
    }

    public override void Awake()
    {
        base.Awake();
        
        // Cache list of player prefabs
        playerPrefabs = FindPlayerPrefabs();
        
        // Cache list of item prefabs
        itemsDictionary = FindItems();
    }
}
