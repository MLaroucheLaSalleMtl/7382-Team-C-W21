using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Sword : MonoBehaviour
{
    public Player player;
    private BoxCollider _swordCollider;
    [HideInInspector] public int attackDamage;
    
    void OnTriggerEnter(Collider collision)
    {
        // Have we collided with an enemy? (debugging)
        if (collision.CompareTag("Enemy"))
        {
            player.IsNotAttacking();
            _swordCollider.enabled = false;
            collision.GetComponent<Enemy>().ReceiveAttack(attackDamage);

            if (collision.GetComponent<Enemy>().health <= 0)
                player.experience += collision.GetComponent<Enemy>().givenExperience;
            
            Debug.Log("Hit an enemy, disabled weapon collider!");
        }
    }

    void Awake()
    {
        player = GetComponentInParent<Player>();
        _swordCollider = GetComponent<BoxCollider>();
        
        // Initialize collider sizes
        _swordCollider.center = new Vector3(0f, 0f, 0.6f);
        _swordCollider.size = new Vector3(0.1f, 0.1f, 1.2f);
        
        // Set Weapon tag
        gameObject.tag = "Weapon";
    }

    void Update()
    {
        attackDamage = player.attackValue;
        _swordCollider.enabled = player.isAttacking;
    }
}