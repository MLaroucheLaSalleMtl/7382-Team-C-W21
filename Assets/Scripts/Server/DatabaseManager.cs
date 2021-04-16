using System.Collections.Generic;
using System.Data;
using System.IO;
using Mirror;
using SQLite; // from: https://github.com/praeclarum/sqlite-net
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
	// Singleton for ease of access
	public static DatabaseManager singleton;

	// Database file name
	[Header("Database file name")]
	public string databaseFile = "Skyland.sqlite";

	// SQL Connection
	private SQLiteConnection _connection;

	// Accounts table
	// Reference: https://github.com/praeclarum/sqlite-net/wiki/GettingStarted
	class accounts
	{
		[PrimaryKey] public string username { get; set; }
		public string password { get; set; }
	}
	
	public class characters
	{
		// [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Player' and 'player' as characters]
		[PrimaryKey] [Collation("NOCASE")] public string name { get; set; }
		
		[AutoIncrement] public int id { get; }
		
		// add index on username to avoid full scans when loading characters
		[Indexed] public string username { get; set; }
		
		public string gender { get; set; }
		public float x { get; set; }
		public float y { get; set; }
		public float z { get; set; }
		public int level { get; set; }
		public long experience { get; set; }
		public int health { get; set; }
		public int mana { get; set; }
		public int bonusHealth { get; set; }
		public int bonusMana { get; set; }
		public int bonusAgility { get; set; }
		public int bonusIntelligence { get; set; }
		public int bonusStrength { get; set; }
		public int bonusAttack { get; set; }
		public bool online { get; set; }
		public bool deleted { get; set; }
		
		public int medusaCoins { get; set; }
		public int chestCoins { get; set; }
		public int minotaurCoins { get; set; }
		public int mushroomCoins { get; set; }
		public int rockMonsterCoins { get; set; }
		public int spiderCoins { get; set; }
		public int trollCoins { get; set; }
		
		public int arms { get; set; }
		public int feet { get; set; }
		public int hands { get; set; }
		public int head { get; set; }
		public int legs { get; set; }
		public int shoulders { get; set; }
		public int waist { get; set; }
		public int weapon { get; set; }
		public int shield { get; set; }
		
		public int gold { get; set; }
		public int slot1 { get; set; }
		public int slot2 { get; set; }
		public int slot3 { get; set; }
		public int slot4 { get; set; }
		public int slot5 { get; set; }
		public int slot6 { get; set; }
		public int slot7 { get; set; }
		public int slot8 { get; set; }
		public int slot9 { get; set; }
		public int slot10 { get; set; }
		public int slot11 { get; set; }
		public int slot12 { get; set; }
		public int slot13 { get; set; }
		public int slot14 { get; set; }
		public int slot15 { get; set; }
		public int slot16 { get; set; }
		public int slot17 { get; set; }
		public int slot18 { get; set; }
		public int slot19 { get; set; }
		public int slot20 { get; set; }
		public int slot21 { get; set; }
		public int slot22 { get; set; }
		public int slot23 { get; set; }
		public int slot24 { get; set; }
	}

	public void Connect()
	{
		// If we're on Editor, we don't want the .sqlite file inside the Assets folder
		// because it is not an asset, lol
#if UNITY_EDITOR
		string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, databaseFile);
#else
        string path = Path.Combine(Application.dataPath, databaseFile);
#endif

		// Open SQLite connection
		// Database file is created automatically if it doesn't exist
		_connection = new SQLiteConnection(path);

		// Create tables if it doesn't exist yet
		_connection.CreateTable<accounts>();
		_connection.CreateTable<characters>();

		print("Connected to database!");
	}

	void Awake()
	{
		// Initialize singleton
		if (singleton == null) singleton = this;
	}

	// Close connection when Unity closes to prevent the database from locking
	void OnApplicationQuit()
	{
		// Close connection
		_connection?.Close();
	}

	// Add new entry to Accounts table
	public bool Login(string username, string password)
	{
		// If username and password are not empty
		if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
		{
			// For demo purposes: create an account if it doesn't exist
			if (_connection.FindWithQuery<accounts>("SELECT * FROM accounts WHERE username = ?", username) == null)
				_connection.Insert(new accounts {username = username, password = password});

			// Check username and password
			if (_connection.FindWithQuery<accounts>("SELECT * FROM accounts WHERE username = ? AND password = ?",
				username, password) != null)
			{
				// Login succeeded
				return true;
			}
		}

		// Login failed
		return false;
	}
	
	// return tru
	public bool CharacterExists(string characterName)
	{
		return _connection.FindWithQuery<characters>("SELECT * FROM characters WHERE name = ?", characterName) != null;
	}

	// soft delete the character so it can be restored later
	public void DeleteCharacter(string characterName)
	{
		_connection.Execute("UPDATE characters SET deleted = 1 WHERE name = ?", characterName);
	}

	// Returns the list of character names for that username
	public List<string> CharactersForUsername(string username)
	{
		List<string> result = new List<string>();
		foreach (characters character in _connection.Query<characters>("SELECT * FROM characters WHERE username = ? AND deleted = 0", username))
			result.Add(character.name);
		return result;
	}

	// Loads character information from database
	public characters LoadCharacter(string characterName)
    {
	    MMONetworkManager networkManager = (MMONetworkManager) NetworkManager.singleton;
        characters row = _connection.FindWithQuery<characters>("SELECT * FROM characters WHERE name = ? AND deleted = 0", characterName);
        if (row != null)
	        return row;
        /*if (row != null)
        {
	        // generate prefab name. in the future we can introduce new races if we want.
	        string genderString = "Human " + char.ToUpper(row.gender[0]) + row.gender.Substring(1);
	        
            // instantiate based on the gender, always using first letter as uppercase. like: Male
            Player prefab = prefabs.Find(p => p.name == genderString);
            
            if (prefab != null)
            {
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

                player.isPreview = isPreview;
                
                player.equippedArms           = (row.arms == 0 ? null : networkManager.itemsDictionary[row.arms]);
                player.equippedFeet           = (row.feet == 0 ? null : networkManager.itemsDictionary[row.feet]);
                player.equippedHands          = (row.hands == 0 ? null : networkManager.itemsDictionary[row.hands]);
                player.equippedHead           = (row.head == 0 ? null : networkManager.itemsDictionary[row.head]);
                player.equippedLegs           = (row.legs == 0 ? null : networkManager.itemsDictionary[row.legs]);
                player.equippedShoulders      = (row.shoulders == 0 ? null : networkManager.itemsDictionary[row.shoulders]);
                player.equippedWaist          = (row.waist == 0 ? null : networkManager.itemsDictionary[row.waist]);
                player.equippedWeapon         = (row.weapon == 0 ? null : networkManager.itemsDictionary[row.weapon]);
                player.equippedShield         = (row.shield == 0 ? null : networkManager.itemsDictionary[row.shield]);

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
                
                player.inventory.AddGold(row.gold);
                player.inventory.AddItem(0, row.slot1);
                player.inventory.AddItem(1, row.slot2);
                player.inventory.AddItem(2, row.slot3);
                player.inventory.AddItem(3, row.slot4);
                player.inventory.AddItem(4, row.slot5);
                player.inventory.AddItem(5, row.slot6);
                player.inventory.AddItem(6, row.slot7);
                player.inventory.AddItem(7, row.slot8);
                player.inventory.AddItem(8, row.slot9);
                player.inventory.AddItem(9, row.slot10);
                player.inventory.AddItem(10, row.slot11);
                player.inventory.AddItem(11, row.slot12);
                player.inventory.AddItem(12, row.slot13);
                player.inventory.AddItem(13, row.slot14);
                player.inventory.AddItem(14, row.slot15);
                player.inventory.AddItem(15, row.slot16);
                player.inventory.AddItem(16, row.slot17);
                player.inventory.AddItem(17, row.slot18);
                player.inventory.AddItem(18, row.slot19);
                player.inventory.AddItem(19, row.slot20);
                player.inventory.AddItem(20, row.slot21);
                player.inventory.AddItem(21, row.slot22);
                player.inventory.AddItem(22, row.slot23);
                player.inventory.AddItem(23, row.slot24);

                //player.TeleportTo(player.position);
                go.transform.position = player.position;

                // If we're not just showing the character on Character Selection screen,
                // set the player as online and load it's modules
                if (!isPreview)
                {
	                // Set player as online
                    _connection.Execute("UPDATE characters SET online = 1 WHERE name = ?", characterName);
                }

                return go;
            }
            else 
	            print("No prefab named: " + genderString + " found.");
    }*/
        print("FATAL ERROR on LoadCharacter");
        return null;
    }

	// adds or overwrites character data in the database
	public void SaveCharacter(Player player, bool online, bool useTransaction = true)
	{
		// only use a transaction if not called within SaveMany transaction
		if (useTransaction) 
			_connection.BeginTransaction();

		_connection.InsertOrReplace(new characters{
			name = char.ToUpper(player.myName[0]) + player.myName.Substring(1),
			username = player.username,
			gender = player.gender,
			x = player.position.x,
			y = player.position.y,
			z = player.position.z,
			level = player.level,
			experience = player.experience,

			health = player.health,
			mana = player.mana,
			
			medusaCoins = player.medusaCoins,
			chestCoins = player.chestMonsterCoins,
			minotaurCoins = player.minotaurCoins,
			mushroomCoins = player.mushroomCoins,
			rockMonsterCoins = player.rockMonsterCoins,
			spiderCoins = player.spiderCoins,
			trollCoins = player.trollCoins,

			bonusHealth = player.bonusHealth,
			bonusMana = player.bonusMana,
			bonusAgility = player.bonusAgility,
			bonusIntelligence = player.bonusIntelligence,
			bonusStrength = player.bonusStrength,
			bonusAttack = player.bonusAttack,
			
			arms = player.equippedArms == null ? 0 : player.equippedArms.itemId,
			feet = player.equippedFeet == null ? 0 : player.equippedFeet.itemId,
			hands = player.equippedHands == null ? 0 : player.equippedHands.itemId,
			head = player.equippedHead == null ? 0 : player.equippedHead.itemId,
			legs = player.equippedLegs == null ? 0 : player.equippedLegs.itemId,
			shield = player.equippedShield == null ? 0 : player.equippedShield.itemId,
			shoulders = player.equippedShoulders == null ? 0 : player.equippedShoulders.itemId,
			waist = player.equippedWaist == null ? 0 : player.equippedWaist.itemId,
			weapon = player.equippedWeapon == null ? 0 : player.equippedWeapon.itemId,
			
			gold           = player.inventory.gold,
			slot1          = player.inventory.GetItemID(0),
			slot2          = player.inventory.GetItemID(1),
			slot3          = player.inventory.GetItemID(2),
			slot4          = player.inventory.GetItemID(3),
			slot5          = player.inventory.GetItemID(4),
			slot6          = player.inventory.GetItemID(5),
			slot7          = player.inventory.GetItemID(6),
			slot8          = player.inventory.GetItemID(7),
			slot9          = player.inventory.GetItemID(8),
			slot10         = player.inventory.GetItemID(9),
			slot11         = player.inventory.GetItemID(10),
			slot12         = player.inventory.GetItemID(11),
			slot13         = player.inventory.GetItemID(12),
			slot14         = player.inventory.GetItemID(13),
			slot15         = player.inventory.GetItemID(14),
			slot16         = player.inventory.GetItemID(15),
			slot17         = player.inventory.GetItemID(16),
			slot18         = player.inventory.GetItemID(17),
			slot19         = player.inventory.GetItemID(18),
			slot20         = player.inventory.GetItemID(19),
			slot21         = player.inventory.GetItemID(20),
			slot22         = player.inventory.GetItemID(21),
			slot23         = player.inventory.GetItemID(22),
			slot24         = player.inventory.GetItemID(23),
			
			online = online,
		});

		if (useTransaction)
			_connection.Commit();
	}
	
	public void SaveManyCharacters(IEnumerable<Player> players, bool online = true)
	{
		// Transaction for performance
		_connection.BeginTransaction();

		// Save all online players
		foreach (Player player in players)
			SaveCharacter(player, online, false);

		// End transaction
		_connection.Commit();
	}
}