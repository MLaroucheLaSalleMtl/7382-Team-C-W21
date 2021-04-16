using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    [SerializeField] private float fillSpeed;

    [Header("Status Text")]
    [SerializeField] private TextMeshProUGUI playerNameTMP;
    [SerializeField] private Text healthText;
    [SerializeField] private Text manaText;
    [SerializeField] private TextMeshProUGUI levelTMP;
    [Header("Bars")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image manaBar;
    [SerializeField] private Image experienceBar;
    [Header("Player Info")]
    [SerializeField] private Player _player;
    public float healthPercentage;
    public float manaPercentage;
    public float experiencePercentage;

    // Update is called once per frame
    void Update()
    {
        // Display the player name
        playerNameTMP.text = _player.myName;
        
        // Calculate the percentage of health, mana and experience
        if (_player.health > 0)
            healthPercentage = ((float)_player.health / (float)_player.maxHealth);
        if (_player.health <= 0)
            healthPercentage = 0;

        if (_player.mana > 0)
            manaPercentage = ((float)_player.mana / (float)_player.maxMana);
        if (_player.mana <= 0)
            manaPercentage = 0;

        if (_player.experience > 0)
            experiencePercentage = (float)_player.experience / (float)_player.experiencePerLevel;
        
        // Update the displayed health/maxHealth
        healthText.text = _player.health + "/" + _player.maxHealth;
        healthBar.fillAmount = healthPercentage;

        // Update the displayed mana/maxMana
        manaText.text = _player.mana + "/" + _player.maxMana;
        manaBar.fillAmount = manaPercentage;
        
        // Update the displayed experience and level
        experienceBar.fillAmount = experiencePercentage;
        levelTMP.text = "Level " + _player.level;
    }
}
