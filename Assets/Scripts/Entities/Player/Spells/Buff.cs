using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff : MonoBehaviour
{
    [SerializeField] private Player _player;
    private ParticleSystem _particleSystem;
    [Header("Buff effect")]
    [SerializeField] private int bonusHealth;
    [SerializeField] private int bonusMana;
    [SerializeField] private int bonusStrength;
    [SerializeField] private int bonusIntelligence;
    [SerializeField] private int bonusAgility;

    private void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _particleSystem.Play();
    }

    // We want to apply the bonus stats when this script is enabled
    private void OnEnable()
    {
        _player.bonusStrength += bonusStrength;
        _player.bonusIntelligence += bonusIntelligence;
        _player.bonusAgility += bonusAgility;

        _player.bonusHealth += bonusHealth;
        _player.bonusMana += bonusMana;
        
        // Recalculate player stats
        _player.RecalculateStats();
    }
    
    // And remove the bonus stats when this script is disabled
    private void OnDisable()
    {
        _player.bonusStrength -= bonusStrength;
        _player.bonusIntelligence -= bonusIntelligence;
        _player.bonusAgility -= bonusAgility;

        _player.bonusHealth -= bonusHealth;
        _player.bonusMana -= bonusMana;
        
        // Recalculate player stats
        _player.RecalculateStats();
        
        // Stop particle system
        _particleSystem.Stop();
        _particleSystem.Clear();
    }
}
