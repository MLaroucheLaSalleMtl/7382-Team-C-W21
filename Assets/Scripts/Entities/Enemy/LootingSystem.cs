using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class LootingSystem : MonoBehaviour
{
    [SerializeField] private List<int> DropableItems = new List<int>();
    [SerializeField] private List<int> ItemChance = new List<int>();
    private int luck;
    public int goldLoot;
    public bool medusaCoinsIsActive;
    public bool chestMonsterCoinsIsActive;
    public bool minotaurCoinsIsActive;
    public bool mushroomCoinsIsActive;
    public bool rockMonsterCoinsIsActive;
    public bool spiderCoinsIsActive;
    public bool trollCoinsIsActive;

    public void GenerateLuck()
    {
        luck = Random.Range(0, 101);
    }

    public List<Item> GenerateLoot()
    {
        List<Item> DroppedItems = new List<Item>();
        MMONetworkManager networkManager = (MMONetworkManager) NetworkManager.singleton;

        for (int i = 0; i < ItemChance.Count - 1; i++)
        {
            GenerateLuck();
            if (luck <= ItemChance[i])
            {
                Debug.Log("Adding Item" + DropableItems[i]);
                Debug.Log("Random Number: " + luck);
                DroppedItems.Add(networkManager.itemsDictionary[DropableItems[i]]);
            }
        }
        return DroppedItems;
    }

    // Link the items list to the list of chances X
    // Match luck/chance X
    // generate a list of drop items
}
