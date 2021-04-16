// Contains all the network messages that we use

using System.Collections.Generic;
using Mirror;

// Client to Server
public class LoginMsg : NetworkMessage
{
    public string username;
    public string password;
    public string version;
}

public class CreateCharacterMsg : NetworkMessage
{
    public string name;
    public string gender;
}

public class SelectCharacterMsg : NetworkMessage
{
    public int index;
}

public class DeleteCharacterMsg : NetworkMessage
{
    public int index;
}


// Server to Client
public class ErrorMsg : NetworkMessage
{
    public string text;
    public bool causesDisconnect;
}

public class LoginSuccessMsg : NetworkMessage
{
}

public class AvailableCharactersMsg : NetworkMessage
{
    public struct CharacterPreview
    {
        public string name;
        public string gender;
        // TODO: Add character customization
        // Equipment
        public int arms;
        public int feet;
        public int hands;
        public int head;
        public int legs;
        public int shoulders;
        public int waist;
        public int weapon;
        public int shield;
    }
    public CharacterPreview[] characters;
    
    public void Load(List<Player> players)
    {
        // We only need name, gender and equipment for our UI
        characters = new CharacterPreview[players.Count];
        for (int i = 0; i < players.Count; ++i)
        {
            Player player = players[i];
            characters[i] = new CharacterPreview
            {
                name = player.myName,
                gender = player.gender,
                arms = player.activeArmsSynced,
                feet = player.activeFeetSynced,
                hands = player.activeHandsSynced,
                head = player.activeHeadSynced,
                legs = player.activeLegsSynced,
                shoulders = player.activeShouldersSynced,
                waist = player.activeWaistSynced,
                weapon = player.activeWeaponSynced,
                shield = player.activeShieldSynced
            };
        }
    }
}

