using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private float fillSpeed;

    [Header("Status Text")]
    [SerializeField] private Text healthText;
    
    [Header("Bars")]
    [SerializeField] private Image healthBar;
    
    [Header("Camera")]
    [SerializeField] private GameObject cam;
    
    [Header("Player Info")]
    // TODO: change to entity
    [SerializeField] private Enemy _enemy;
    public float healthPercentage;
    
    // Update is called once per frame
    void Update()
    {
        // Calculate the percentage of health and mana
        if (_enemy.health > 0)
            healthPercentage = ((float)_enemy.health / (float)_enemy.maxHealth);
        if (_enemy.health <= 0)
            healthPercentage = 0;

        // Update the displayed health/maxHealth
        healthText.text = _enemy.health + "/" + _enemy.maxHealth;
        healthBar.fillAmount = healthPercentage;

        //LookAtPlayer();
    }
    
   /* public void LookAtPlayer()
    {
        Vector3 direction = (cam.transform.position - transform.position).normalized;
        Quaternion rotate = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, rotate, Time.deltaTime * 7f);
    }*/
}
