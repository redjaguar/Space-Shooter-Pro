using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Helper_Classes;
using Assets.Scripts.Powerups;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    private static SpawnManager _instance;
    public static SpawnManager Instance => _instance;
    [SerializeField]
    private Player _player;
    [SerializeField]
    private Enemy _enemy;
    
    [SerializeField]
    private Powerup[] powerups;

    private GameObject _enemyContainer;
    private GameObject _powerupContainer;
    private float _enemySpawnRate = 5;

    private bool _isSpawningEnemies;
    private bool _isSpawningPowerups;
    private float _powerupSpawnRateMin = 3f;
    private float _powerupSpawnRateMax = 7f;

    private static readonly PowerupType[] PowerupTypes = (PowerupType[]) Enum.GetValues(typeof(PowerupType));
    private static readonly int NumberOfPowerupTypes = PowerupTypes.Length;

    public SpawnManager()
    {
        _instance = this;
    }

    void Start()
    {
        //_player = GameObject.FindWithTag("Player").GetComponent<Player>();
        //Assert.IsNotNull(_player, "_player != null");
        //_enemy = Resources.Load<Enemy>("Prefabs/Enemy/Enemy"); // GameObject.FindWithTag("Enemy").GetComponent<Enemy>();
        //Assert.IsNotNull(_enemy, "_enemy != null");
        //_tripleShotPowerup = Resources.Load<TripleShotPowerup>("Prefabs/Powerups/Triple_Shot_Powerup");
        //Assert.IsNotNull(_tripleShotPowerup, "_tripleShotPowerup != null");
        _player.PlayerDeath += OnPlayerDeath;
        var parentTransform = gameObject.transform;
        _enemyContainer = new GameObject("Enemy Container");
        _enemyContainer.transform.parent = parentTransform;
        _powerupContainer = new GameObject("Powerup Container");
        _powerupContainer.transform.parent = parentTransform;

        StartSpawningEnemy();
        StartSpawningPowerups();
    }

    // Update is called once per frame
    //void Update()
    //{
    //}

    public void StartSpawningEnemy()
    {
        _isSpawningEnemies = true;
        StartCoroutine(nameof(SpawnEnemy), 1);
    }

    public void StartSpawningPowerups()
    {
        _isSpawningPowerups = true;
        StartCoroutine(nameof(SpawnPowerups));
    }

    void OnPlayerDeath(Player player)
    {
        if (player.Lives > 0)
        {
            return;
        }
        // Multiplayer?
        _isSpawningEnemies = false;
        _isSpawningPowerups = false;
        Clear<Laser>();
        Clear<Powerup>();
        Destroy(_enemyContainer);
    }

    void Clear<T>() where T : Component
    {
        var components = GameObject.FindObjectsOfType<T>();

        if (components == null || components.Length == 0)
        {
            return;
        }

        for (int i = 0; i < components.Length; i++)
        {
            Destroy(components[i].gameObject);
        }
    }

    void OnEnemyDeath(Enemy enemy)
    {
        //For possible future use
    }

    IEnumerator SpawnEnemy()
    {
       
            while (_isSpawningEnemies)
            {
                var newEnemy = Instantiate(_enemy, Enemy.GetSpawnPoint(), Quaternion.identity);
                newEnemy.transform.parent = _enemyContainer.transform;
                newEnemy.EnemyDeath += _player.OnEnemyDeath;
                yield return new WaitForSeconds(_enemySpawnRate);
            }

    }


    IEnumerator SpawnPowerups()
    {
        
            while (_isSpawningPowerups)
            {
                var spawnXPoint = Random.Range(Globals.PlayerBounds.min.x, Globals.PlayerBounds.max.x);
                var powerupToSpawn = PowerupTypes[Random.Range(0, NumberOfPowerupTypes)];
                //powerupToSpawn = PowerupType.TripleShot;
                var spawnPosition = new Vector3(spawnXPoint, Globals.ScreenBounds.max.y);
                Powerup newPowerup = Instantiate(powerups[Random.Range(0, powerups.Length)], spawnPosition, Quaternion.identity);

                if (newPowerup != null)
                {
                    newPowerup.transform.parent = _powerupContainer.transform;
                    newPowerup.PowerupCollected += _player.OnPowerupCollected;
                }

                yield return new WaitForSeconds(Random.Range(_powerupSpawnRateMin, _powerupSpawnRateMax));
            }
            
           
    }
}