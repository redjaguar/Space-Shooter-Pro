using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Helper_Classes;
using Assets.Scripts.Player;
using Assets.Scripts.Powerups;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

#pragma  warning disable CS0649
public class Player : MonoBehaviour
{
    /// <summary>
    /// Player movement speed.
    /// </summary>
    private float speed = 5f;
    
    /// <inheritdoc cref="Globals.PlayerBounds"/>
    public Bounds PlayerBounds => Globals.PlayerBounds;
    
    /// <summary>
    /// Delegate for the <see cref="Player.PlayerLivesChanged"/> event.
    /// </summary>
    /// <param name="player">The player that is the source of the event.</param>
    public delegate void PlayerLivesChangedEventHandler(Player player);

    /// <summary>
    /// Event fired when this player dies.
    /// </summary>
    public event PlayerLivesChangedEventHandler PlayerLivesChanged;

    public delegate void ShotFiredEventHandler(int shotsLeft);

    public event ShotFiredEventHandler ShotFired;

    /// <summary>
    /// Y axis offset for the laser when fired.
    /// </summary>
    [SerializeField]
    private float _laserOffset = 0.8f;

    /// <summary>
    /// Laser prefab prototype to use for <see cref="UnityEngine.Object.Instantiate{T}(T, Vector3, Quaternion)"/>.
    /// </summary>
    [SerializeField]
    private GameObject _laserPreFab;

    /// <summary>
    /// Triple shot prefab prototype to use for <see cref="UnityEngine.Object.Instantiate{T}(T, Vector3, Quaternion)"/>.
    /// </summary>
    [SerializeField] 
    private GameObject _tripleShotPreFab;

    [SerializeField]
    private GameObject _minePrefab;

    [SerializeField]
    private bool _minesActive;
    /// <summary>
    /// Rate of fire limit in seconds for the lasers.
    /// </summary>
    [SerializeField]
    private float _fireRate = 0.15f;

    /// <summary>
    /// The next game time in seconds the lase can be fired.
    /// </summary>
    private float _fireDelayTime = -1f;

    [SerializeField]
    private int _maximumLives = 3;

    /// <summary>
    /// Number of lives this player has.
    /// </summary>
    [SerializeField]
    private int _lives;

    /// <inheritdoc cref="_lives"/>
    public int Lives => _lives;
    
    /// <summary>
    /// Whether or not the laser can currently be fired.
    /// </summary>
    private bool CanFire => Time.time >= _fireDelayTime;

    /// <summary>
    /// Whether or not Triple Shot is active.
    /// </summary>
    [SerializeField]
    private bool _tripleShotActive;

    /// <summary>
    /// Scalar factor to apply to the speed when the speed boost is active.
    /// </summary>
    [SerializeField]
    private float _speedBoostFactor = 2f;

    ///// <summary>
    ///// Whether or not shields are currently active.
    ///// </summary>
    //[SerializeField]
    //private bool _shieldsActive;

    /// <summary>
    /// This players shields.
    /// </summary>
    private Shields _shields;

    /// <summary>
    /// This players current score.
    /// </summary>
    public int Score { get; private set; }

    /// <summary>
    /// Delegate for the <see cref="PlayerScoreChanged"/>
    /// </summary>
    /// <param name="player">The player that is the source of the event.</param>
    public delegate void PlayerScoreChangedEventHandler(Player player);

    /// <summary>
    /// Event fired when this players score changes.
    /// </summary>
    public event PlayerScoreChangedEventHandler PlayerScoreChanged;

    /// <summary>
    /// Animation to use for the left wing hit.
    /// </summary>
    [SerializeField]
    private GameObject _leftWingHurtAnim;

    /// <summary>
    /// Animation to use for the right wing hit.
    /// </summary>
    [SerializeField]
    private GameObject _rightWingHurtAnim;

    /// <summary>
    /// AudioClip to use when firing the laser.
    /// </summary>
    [SerializeField]
    private AudioClip _laserShotAudioClip;

    /// <summary>
    /// AudioClip to use for explosions.
    /// </summary>
    [SerializeField]
    private AudioClip _explosionAudio;

    /// <summary>
    /// AudioCLip to use when attempting to fire with no ammo.
    /// </summary>
    [SerializeField]
    private AudioClip _noAmmoAudioClip;

    private Thruster _thruster;
    public Thruster Thruster => _thruster;
    /// <summary>
    /// Animation to use for explosions.
    /// </summary>
    [SerializeField] private GameObject _exlposionAnim;
    private Animator _spriteAnimator;

    /// <summary>
    /// AudioSource to use for playing sound effects.
    /// </summary>
    private AudioSource _audioSource;

    [SerializeField]
    private int _maximumLaserShots = 15;

    private int _laserShotsLeft = 15;

    // Start is called before the first frame update
    void Start()
    {
        _lives = _maximumLives;
        
        transform.position = Vector3.zero;
        _shields = GameObject.FindWithTag("Player Shields")?.GetComponent<Shields>();
        Assert.IsNotNull(_shields, "_shields != null");
        _shields.gameObject.SetActive(false);
        _spriteAnimator = _exlposionAnim.GetComponent<Animator>();
        Assert.IsNotNull(_spriteAnimator, "_spriteAnimator != null");

        _audioSource = gameObject.GetComponent<AudioSource>();
        _thruster = GameObject.Find("Thruster").GetComponent<Thruster>();
        Assert.IsNotNull(_thruster, "_thruster != null");

        _thruster.FuelLevelChanged += fuelLevel =>
        {
            speed /= fuelLevel <= 0 ? _thruster.SpeedScalar : 1;
        };

        _camera = GameObject.FindWithTag("MainCamera")?.GetComponent<Camera>();
        Assert.IsNotNull(_camera, "_camera != null");
    }

    void Update()
    {
        Move();
        if (Input.GetKeyDown(KeyCode.Space) && CanFire)
        {
            FireWeapon();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (_thruster.IsFiring)
            {
                return;
            }

            _thruster.TurnOn();
            speed *= Thruster.SpeedScalar;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            if (!_thruster.IsFiring)
            {
                return;
            }

            _thruster.TurnOff();
            speed /= Thruster.SpeedScalar;
        }

    }

    /// <summary>
    /// Spawns normal laser or a triple shot laser if the triple shot is active.
    /// </summary>
    void FireWeapon()
    {
        
        var playerPosition = transform.position;

        if (_laserShotsLeft == 0 && !_minesActive)
        {
            AudioSource.PlayClipAtPoint(_noAmmoAudioClip, playerPosition);
            return;
        }

        _fireDelayTime = Time.time + _fireRate;

        if (_minesActive)
        {
            var mines = Instantiate(_minePrefab, playerPosition, Quaternion.identity);
            return;
        }

        var newTripleShot = Instantiate(_tripleShotActive ? _tripleShotPreFab : _laserPreFab, _tripleShotActive ? playerPosition : playerPosition + new Vector3(0, _laserOffset), Quaternion.identity);
        _audioSource.PlayOneShot(_laserShotAudioClip);
        _laserShotsLeft--;
        ShotFired?.Invoke(_laserShotsLeft);
    }

    public void RegisterPowerupHandlers(Powerup powerup)
    {
        powerup.PowerupCollected += OnPowerupCollected;
        powerup.PowerupCollected += _shields.OnPowerupCollected;
    }

    public void RegisterEnemyHandlers(Enemy enemy)
    {
        enemy.PlayerCollision += OnEnemyCollision;
        enemy.EnemyDeath += OnEnemyDeath;
    }

    /// <summary>
    /// Callback for the <see cref="Powerup.PowerupCollected"/>.
    /// </summary>
    /// <param name="powerup">Powerup that was collected.</param>
    public void OnPowerupCollected(Powerup powerup)
    {
        switch (powerup.PowerupType)
        {
            case PowerupType.AmmoRefill:
                _laserShotsLeft = _maximumLaserShots;
                ShotFired?.Invoke(_laserShotsLeft);
                break;
            case PowerupType.Mine:
                _minesActive = true;
                StartCoroutine(DisablePowerup(powerup.PowerupType));
                break;
            case PowerupType.Ship:
                if (_lives < 3)
                {
                    _lives++;
                    UpdateHurtVisibility();
                    PlayerLivesChanged?.Invoke(this);
                    
                }
                break;
            case PowerupType.Speedup:
                speed *= _speedBoostFactor;
                StartCoroutine(DisablePowerup(powerup.PowerupType)); // nameof(DisableSpeedBoost));
                break;
            case PowerupType.TripleShot:
                _tripleShotActive = true;
                StartCoroutine(DisablePowerup(powerup.PowerupType));
                break;
            default:
                Debug.LogWarningFormat("Unknown powerup: {0}", powerup.PowerupType);
                break;

        }

    }


    private bool ignoreNextLaser;

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Laser"))
        {
            if (ignoreNextLaser)
            {
                ignoreNextLaser = false;
                return;
            }

            ignoreNextLaser = true;
            var laser = other.GetComponent<Laser>();
            Assert.IsNotNull(laser, "laser != null");
            if (laser.IsEnemyLaser)
            {
                OnEnemyCollision(null);
                Destroy(laser.gameObject);
            }
        }
    }

    /// <summary>
    /// Callback for the <see cref="Enemy.EnemyDeath"/>
    /// </summary>
    /// <param name="enemy">The enemy that is the source of the event.</param>
    /// <param name="pointValue">Point value for the enemy.</param>
    public void OnEnemyDeath(Enemy enemy, int pointValue = 0)
    {
        
        Score += pointValue;
        if (pointValue > 0)
        {
            PlayerScoreChanged?.Invoke(this);
        }
    }
    
    /// <summary>
    /// Coroutine to disable powerups.
    /// </summary>
    /// <returns>Coroutine</returns>
    IEnumerator DisablePowerup(PowerupType powerupType)
    {
        yield return new WaitForSeconds(5f);
        switch (powerupType)
        {
            case PowerupType.Mine:
                _minesActive = false;
                break;
            case PowerupType.Speedup:
                _speedBoostFactor /= _speedBoostFactor;
                break;
            case PowerupType.TripleShot:
                _tripleShotActive = false;
                break;
        }
        _tripleShotActive = false;
        yield return null;
    }

   
    

    /// <summary>
    /// Moves the player while restricting the movement to be within <see cref="PlayerBounds"/>.
    /// </summary>
    void Move()
    {
        Transform playerTransform;
        (playerTransform = transform).Translate(new Vector3(Input.GetAxis(@"Horizontal"), Input.GetAxis(@"Vertical")) * (speed * Time.deltaTime));
        var playerPosition = playerTransform.position;

        if (PlayerBounds.Contains(playerPosition))
        {
            return;
        }

        var newX = playerPosition.x;
        var newY = Mathf.Clamp(playerPosition.y, PlayerBounds.min.y, PlayerBounds.max.y);

        if (newX >= PlayerBounds.max.x)
        {
            newX = PlayerBounds.min.x;
        }
        else if (newX <= PlayerBounds.min.x)
        {
            newX = PlayerBounds.max.x;
        }

        playerTransform.position = new Vector3(newX, newY);
    }

    /// <summary>
    /// Callback for <see cref="Enemy.PlayerCollision"/>.
    /// </summary>
    /// <param name="enemy">Enemy that is the source of the event.</param>
    public void OnEnemyCollision(Enemy enemy)
    {
        if (_shields.Active)
        {
            _shields.Hit();
            return;
        }

        _lives--;
        PlayerLivesChanged?.Invoke(this);
        if (_lives < 1)
        {
            _spriteAnimator.SetTrigger(AnimationTriggers.EnemyDeath);
            _audioSource.PlayOneShot(_explosionAudio);
            Destroy(this.gameObject, .50f);
        }
        UpdateHurtVisibility();
        StartCoroutine(CameraShake());
    }

    private Camera _camera;

    IEnumerator CameraShake()
    {

        _camera.transform.Rotate(Vector3.forward, 1f);
        yield return new WaitForSeconds(Time.deltaTime);
        _camera.transform.Rotate(Vector3.forward, -2f);
        yield return new WaitForSeconds(Time.deltaTime);
        _camera.transform.Rotate(Vector3.forward, 2f);
        yield return new WaitForSeconds(Time.deltaTime);
        _camera.transform.Rotate(Vector3.forward, -2f);
        yield return new WaitForSeconds(Time.deltaTime);
        _camera.transform.Rotate(Vector3.forward, 2f);
        yield return new WaitForSeconds(Time.deltaTime);

    }

    /// <summary>
    /// Updates which hurt animations are visible.
    /// </summary>
    private void UpdateHurtVisibility()
    {
        switch (Lives)
        {
            case 3:
                _leftWingHurtAnim.SetActive(false);
                _rightWingHurtAnim.SetActive(false);
                break;
            case 2:
                _leftWingHurtAnim.SetActive(true);
                _rightWingHurtAnim.SetActive(false);
                break;
            case 1:
                _rightWingHurtAnim.SetActive(true);
                break;
        }
    }
}