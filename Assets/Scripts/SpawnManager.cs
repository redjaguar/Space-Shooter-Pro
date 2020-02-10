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
 
    
    [SerializeField]
    private Player _player;

    [SerializeField]
    private Enemy _enemy;
    
    [SerializeField]
    private Powerup[] powerups;

    private GameObject _enemyContainer;
    private float _enemySpawnRate = 5;

    private bool _isSpawningEnemies;
    private bool _isSpawningPowerups;
    private float _powerupSpawnRateMin = 3f;
    private float _powerupSpawnRateMax = 7f;

    private static readonly PowerupType[] PowerupTypes = (PowerupType[]) Enum.GetValues(typeof(PowerupType));
    private static readonly int NumberOfPowerupTypes = PowerupTypes.Length;

    
    void Start()
    {
        _player.PlayerDeath += OnPlayerDeath;
        var parentTransform = gameObject.transform;
        _enemyContainer = new GameObject("Enemy Container");
        _enemyContainer.transform.parent = parentTransform;

        var asteroid = GameObject.FindObjectOfType<Asteroid>();
        Assert.IsNotNull(asteroid, "asteroid != null");
        asteroid.AsteroidExploded += OnAsteroidExploded;
    }

    public void OnAsteroidExploded(Asteroid asteroid)
    {
        StartSpawningEnemy();
        StartSpawningPowerups();
    }

    #region Enemy
    public void StartSpawningEnemy()
    {
        _isSpawningEnemies = true;
        StartCoroutine(nameof(SpawnEnemy), 1);
    }

    IEnumerator SpawnEnemy()
    {
       
        while (_isSpawningEnemies)
        {
            var newEnemy = Instantiate(_enemy, Enemy.GetSpawnPoint(), Quaternion.identity);
            newEnemy.transform.parent = _enemyContainer.transform;
            newEnemy.EnemyDeath += _player.OnEnemyDeath;
            newEnemy.PlayerCollision += _player.OnEnemyCollision;
            yield return new WaitForSeconds(_enemySpawnRate);
        }

    }
    #endregion

    #region Powerup
    public void StartSpawningPowerups()
    {
        _isSpawningPowerups = true;
        StartCoroutine(nameof(SpawnPowerups));
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
                newPowerup.PowerupCollected += _player.OnPowerupCollected;
            }

            yield return new WaitForSeconds(Random.Range(_powerupSpawnRateMin, _powerupSpawnRateMax));
        }
            
           
    }
    #endregion

    #region  Player Event Handlers
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

    void OnEnemyDeath(Enemy enemy)
    {
        //For possible future use
    }
    #endregion

    /// <summary>
    /// Clears all components of type T from the scene.
    /// </summary>
    /// <typeparam name="T">Any type that is a component.</typeparam>
    void Clear<T>() where T : Component
    {
        var components = FindObjectsOfType<T>();

        if (components == null || components.Length == 0)
        {
            return;
        }

        for (int i = 0; i < components.Length; i++)
        {
            Destroy(components[i].gameObject);
        }
    }

   

    
}