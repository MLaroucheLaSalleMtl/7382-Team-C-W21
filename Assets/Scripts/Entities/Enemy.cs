using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Enemy : Entity
{
    [Header("Movements variables")]
    private bool _playerIsInCollision = false;
    NavMeshAgent _enemyAgent;
    private float _targetDistance;
    [SerializeField] private GameObject spawnPosition;

    [Header("Attack variables")]
    [SerializeField] private float _attackRange;
    [SerializeField] private float _attackTime;
    private GameObject _enemyTarget;
    [SerializeField] private int _enemyAttack1;
    [SerializeField] private int _enemyAttack2;
    [SerializeField] private int _enemyAttack3; 
    private int _attack;

    [Header("Enemy Stats variables")] 
    [SerializeField] public int health;
    [SerializeField] public int maxHealth;
    [SerializeField] private int healthRegen;
    private bool _regenerating;
    private float _regenerationTimer = 0;
    private float _enemyAttackTimer = 0;
    public float playerGotHitTimer = 0;
    private bool _isDead;

    [Header("Floating Damage variables")]
    [SerializeField] public TextMeshPro damageTMP;
    [SerializeField] public GameObject floatingDamageText;

    [Header("Components")] 
    public LootingSystem lootingSystem;
    public int givenExperience;

    void OnEnable()
    {
        transform.position = spawnPosition.transform.position;
        floatingDamageText.SetActive(false);
    }

    void Awake()
    {
        _enemyAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        lootingSystem = GetComponent<LootingSystem>();

        // Set the tag in case we forgot to set it in Unity
        gameObject.tag = "Enemy";
    }

    void Update()
    {
        
        // If enemy is dead do this
        if (health <= 0)
        {
            health = 0;
            
            if (_isDead) 
                return;
            
            _isDead = true;
            gameObject.tag = "Lootable";
            Invoke(nameof(DisableEnemyPefab), 30.0f);
            PlayAnimation("Death");
            Invoke(nameof(Respawn), 40.0f);

            return;
        }

        _isDead = false;
        
        // Timers
        // max value of float is 340282346638528859811704183484516925440
        // we don't need to worry about the timer always running.
        _enemyAttackTimer += Time.deltaTime;
        _regenerationTimer += Time.deltaTime;

        // Follow the Player and Look at it
        if (_playerIsInCollision)
        {
            // Enemy movement
            _enemyAgent.destination = _enemyTarget.transform.position;
            _targetDistance = Vector3.Distance(_enemyAgent.transform.position, _enemyTarget.transform.position);
            animator.SetFloat("locomotion", 1);
            LookAtPlayer();
            
            //Debug.Log(_targetDistance);

            // If i'm close to 
            if (_targetDistance <= _attackRange)
            {
                _enemyAgent.isStopped = true;
                animator.SetFloat("locomotion", 0);
                if (_enemyAttackTimer >= _attackTime)
                {
                    Attack();
                    
                    // Reset timer
                    _enemyAttackTimer = 0f;
                }
            }
            else
                _enemyAgent.isStopped = false;
        }
        
        // Default animation
        if (!_playerIsInCollision)
        {
            _enemyAgent.destination = spawnPosition.transform.position;

            //Debug.Log(Vector3.Distance(transform.position, spawnPosition.transform.position));
            if (Vector3.Distance(transform.position, spawnPosition.transform.position) <= 1f )
                animator.SetFloat("locomotion", 0);
        }
        
        // Regeneration
        if (_regenerating)
        {
            // Having the check in here makes the enemy regen health as soon as the enemy runs away.
            if (_regenerationTimer >= 2.0f)
            {
                IncreaseHealth();
                _regenerationTimer = 0f;
                gameObject.tag = "Enemy";
            }
        }
    }

    void DisableEnemyPefab()
    {
        gameObject.SetActive(false);
    }

    void Attack()
    {
        var player = _enemyTarget.GetComponent<Player>();
        int typeattack = Random.Range(1, 3);
        if (!player.isDead)
        {
            switch (typeattack)
            {
                case 1:
                    PlayAnimation("Attack1");
                    StartCoroutine(Attack1(false, playerGotHitTimer, player));
                    break;

                case 2:
                    PlayAnimation("Attack2");
                    StartCoroutine(Attack1(false, playerGotHitTimer, player));
                    break;

                case 3:
                    PlayAnimation("Attack3");
                    StartCoroutine(Attack1(false, playerGotHitTimer, player));
                    break;
            }
        }
    }
    
    IEnumerator Attack1(bool status, float delayTime, Player player)
    {
        yield return new WaitForSeconds(delayTime);
        _attack = _enemyAttack1;
        player.DecreaseHealth(_attack);
    }
    
    IEnumerator Attack2(bool status, float delayTime, Player player)
    {
        yield return new WaitForSeconds(delayTime);
        _attack = _enemyAttack2;
        player.DecreaseHealth(_attack);
    }
    
    IEnumerator Attack3(bool status, float delayTime, Player player)
    {
        yield return new WaitForSeconds(delayTime);
        _attack = _enemyAttack3;
        player.DecreaseHealth(_attack);
    }

    public void LookAtPlayer()
    {
        Vector3 direction = (_enemyTarget.transform.position - transform.position).normalized;
        Quaternion rotate = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, rotate, Time.deltaTime * 7f);
    }

    // If player gets in contact with player, change the target
    void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            _enemyTarget = GameObject.FindGameObjectWithTag("Player");
            _playerIsInCollision = true;
            _regenerating = false;
        }
    }

    // If player gets out of danger zone, change the target
    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            _enemyTarget = spawnPosition;
            _playerIsInCollision = false;
            _regenerating = true;
        }
    }

    public void ReceiveAttack(int damage)
    {
        damageTMP.SetText(damage.ToString());
        DecreaseHealth(damage);
        FloatingDamageToggle();
        PlayAnimation("GotHit");
        //Debug.Log("Invoking Callback status is: " + floatingDamageText.activeSelf);
        Invoke(nameof(FloatingDamageToggle), 1.2f);
    }

    public void IncreaseHealth()
    {
        if (health >= maxHealth)
            return;
        
        health += healthRegen;
    }
    
    public void DecreaseHealth(int damage)
    {
        health -= (damage);
    }

    // The player will respawn at the spawn position
    public void Respawn()
    {
        transform.position = spawnPosition.transform.position;
        health = maxHealth;

        // The enemy is looking for a player to follow because its target is the spawn position
        _enemyTarget = spawnPosition;
        _playerIsInCollision = false;
        gameObject.tag = "Enemy";
        gameObject.SetActive(true);
    }

    void FloatingDamageToggle()
    {
        //Debug.Log("BEFORE status is: " + floatingDamageText.activeSelf);
        floatingDamageText.SetActive(!floatingDamageText.activeSelf);
        //Debug.Log("AFTER status is: " + floatingDamageText.activeSelf);
    }
}
