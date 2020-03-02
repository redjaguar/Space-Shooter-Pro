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

    [SerializeField]
    private float _probabilityForSuperWeapon = .10f;


    [SerializeField] private Powerup[] superPowerups;

    private GameObject _enemyContainer;
    private float _enemySpawnRate = 5;

    private bool _isSpawningEnemies;
    private bool _isSpawningPowerups;
    private float _powerupSpawnRateMin = 3f;
    private float _powerupSpawnRateMax = 7f;

    private static readonly PowerupType[] RegularPowerupTypes = Powerup.GetPowerupTypes();
    private static readonly PowerupType[] SuperPowerupTypes = Powerup.GetPowerupTypes(PowerupFilter.Super);
    
    private static readonly int NumberOfRegularPowerupTypes = RegularPowerupTypes.Length;
    private static readonly int NumberOfSuperPowerupTypes = SuperPowerupTypes.Length;

    
    void Start()
    {
        _player.PlayerLivesChanged += OnPlayerLivesChanged;
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
            
            _player.RegisterEnemyHandlers(newEnemy);
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
            var powerupTypeProbability = Random.value;
            bool isSuperPowerup = powerupTypeProbability >= _probabilityForSuperWeapon;

            var spawnPosition = new Vector3(spawnXPoint, Globals.ScreenBounds.max.y);
            var powerupToInstantiate = isSuperPowerup
                ? superPowerups[Random.Range(0, superPowerups.Length - 1)]
                : powerups[Random.Range(0, powerups.Length - 1)];

            Powerup newPowerup = Instantiate(powerupToInstantiate, spawnPosition, Quaternion.identity);

            if (newPowerup != null)
            {
                _player.RegisterPowerupHandlers(newPowerup);
            }

            yield return new WaitForSeconds(Random.Range(_powerupSpawnRateMin, _powerupSpawnRateMax));
        }
            
           
    }
    #endregion

    #region  Player Event Handlers
    void OnPlayerLivesChanged(Player player)
    {
        if (player.Lives > 0)
        {
            return;
        }
        // TODO: Multiplayer?
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