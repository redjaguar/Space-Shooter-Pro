using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Helper_Classes;
using Assets.Scripts.Powerups;
using UnityEngine;
using UnityEngine.Assertions;

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
    /// Delegate for the <see cref="PlayerDeath"/> event.
    /// </summary>
    /// <param name="player">The player that is the source of the event.</param>
    public delegate void PlayerDeathEventHandler(Player player);

    /// <summary>
    /// Event fired when this player dies.
    /// </summary>
    public event PlayerDeathEventHandler PlayerDeath;

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

    /// <summary>
    /// Rate of fire limit in seconds for the lasers.
    /// </summary>
    [SerializeField]
    private float _fireRate = 0.15f;

    /// <summary>
    /// The next game time in seconds the lase can be fired.
    /// </summary>
    private float _fireDelayTime = -1f;

    /// <summary>
    /// Number of lives this player has.
    /// </summary>
    [SerializeField]
    private int _lives = 3;

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

    /// <summary>
    /// Whether or not shields are currently active.
    /// </summary>
    [SerializeField]
    private bool _shieldsActive;

    /// <summary>
    /// The players shields.
    /// </summary>
    private GameObject _shields;

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
    /// Animation to use for explosions.
    /// </summary>
    [SerializeField] private GameObject _exlposionAnim;
    private Animator _spriteAnimator;

    /// <summary>
    /// AudioSource to use for playing sound effects.
    /// </summary>
    private AudioSource _audioSource;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = Vector3.zero;
        _shields = GameObject.FindWithTag("Player Shields");
        _shields.SetActive(false);

        _spriteAnimator = _exlposionAnim.GetComponent<Animator>();
        Assert.IsNotNull(_spriteAnimator, "_spriteAnimator != null");

        _audioSource = gameObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        Move();
        if (Input.GetKeyDown(KeyCode.Space) && CanFire)
        {
            FireLaser();
        } 

    }

    /// <summary>
    /// Spawns normal laser or a triple shot laser if the triple shot is active.
    /// </summary>
    void FireLaser()
    {
        _fireDelayTime = Time.time + _fireRate;
        var playerPosition = transform.position;
        var newTripleShot = Instantiate(_tripleShotActive ? _tripleShotPreFab : _laserPreFab, _tripleShotActive ? playerPosition : playerPosition + new Vector3(0, _laserOffset), Quaternion.identity);
        _audioSource.PlayOneShot(_laserShotAudioClip);
    }
    
    /// <summary>
    /// Callback for the <see cref="Powerup.PowerupCollected"/>.
    /// </summary>
    /// <param name="powerup">Powerup that was collected.</param>
    public void OnPowerupCollected(Powerup powerup)
    {
        switch (powerup.PowerupType)
        {
            case PowerupType.Shield:
                EnableShields();
                break;
            case PowerupType.Speedup:
                speed *= _speedBoostFactor;
                StartCoroutine(nameof(DisableSpeedBoost));
                break;
            case PowerupType.TripleShot:
                _tripleShotActive = true;
                StartCoroutine(nameof(DisableTripleShot));
                break;
            default:
                throw new ArgumentOutOfRangeException();
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
    
    //TODO: See if one method will work with a "powerup type" parameter.

    /// <summary>
    /// Coroutine to disable the triple shot powerup.
    /// </summary>
    /// <returns>Coroutine</returns>
    IEnumerator DisableTripleShot()
    {
        yield return new WaitForSeconds(5f);
        _tripleShotActive = false;
        yield return null;
    }

    /// <summary>
    /// Coroutine to disable the speed boost.
    /// </summary>
    /// <returns></returns>
    IEnumerator DisableSpeedBoost()
    {
        yield return new WaitForSeconds(5f);
        _speedBoostFactor /= _speedBoostFactor;
        yield return null;
    }

    /// <summary>
    /// Enables the shields
    /// </summary>
    void EnableShields()
    {
        _shieldsActive = true;
        _shields.SetActive(true);
    }

    /// <summary>
    /// Disables the shields.
    /// </summary>
    void DisableShields()
    {
        _shieldsActive = false;
        _shields.SetActive(false);

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
        if (_shieldsActive)
        {
            DisableShields();
            return;
        }

        _lives--;
        PlayerDeath?.Invoke(this);
        if (_lives < 1)
        {
            _spriteAnimator.SetTrigger(AnimationTriggers.EnemyDeath);
            _audioSource.PlayOneShot(_explosionAudio);
            Destroy(this.gameObject, .50f);
        }
        UpdateHurtVisibility();
    }

    /// <summary>
    /// Updates which hurt animations are visible.
    /// </summary>
    private void UpdateHurtVisibility()
    {
        switch (Lives)
        {
            case 2:
                _leftWingHurtAnim.SetActive(true);
                break;
            case 1:
                _rightWingHurtAnim.SetActive(true);
                break;
        }
    }
}