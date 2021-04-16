using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debuff : MonoBehaviour
{
    [SerializeField] private Player _player;
    private ParticleSystem _particleSystem;
    [Header("Debuff effect")]
    [SerializeField] private int reduceHealth;
    [SerializeField] private int reduceMana;
    [SerializeField] private int reduceStrength;
    [SerializeField] private int reduceIntelligence;
    [SerializeField] private int reduceAgility;
    
    private void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _particleSystem.Play();
    }

    // We want to reduce the stats when this script is enabled
    private void OnEnable()
    {
        _player.bonusStrength -= reduceStrength;
        _player.bonusIntelligence -= reduceIntelligence;
        _player.bonusAgility -= reduceAgility;

        _player.bonusHealth -= reduceHealth;
        _player.bonusMana -= reduceMana;
        
        // Recalculate player stats
        _player.RecalculateStats();
    }
    
    // And add back the stats when this script is disabled
    private void OnDisable()
    {
        _player.bonusStrength += reduceStrength;
        _player.bonusIntelligence += reduceIntelligence;
        _player.bonusAgility += reduceAgility;

        _player.bonusHealth += reduceHealth;
        _player.bonusMana += reduceMana;
        
        // Recalculate player stats
        _player.RecalculateStats();
        
        // Stop particle system
        _particleSystem.Stop();
    }
}
