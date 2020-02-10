using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Helper_Classes;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

/// <summary>
/// Represents a single enemy.
/// </summary>
public class Enemy : MonoBehaviour
{
    /// <summary>
    /// Laser prefab to instantiate when firing.
    /// </summary>
    [SerializeField]
    private GameObject _enemylaserPrefab;

    /// <summary>
    /// The audio clip to play for hit explosion.
    /// </summary>
    [SerializeField]
    private AudioClip _explosionAudioClip;

    /// <summary>
    /// The audio clip to play when the laser is fired.
    /// </summary>
    [SerializeField]
    private AudioClip _laserFireAudioClip;

    /// <summary>
    /// The speed to travel.
    /// </summary>
    [SerializeField]
    private float _speed = 4f;

    /// <summary>
    /// Screen bounds for enemy movement.
    /// </summary>
    public static Bounds EnemyBounds => Globals.ScreenBounds;

    /// <summary>
    /// Delegate for the <see cref="EnemyDeath"/> event.
    /// </summary>
    /// <param name="enemy">The enemy that is the source of the event.</param>
    /// <param name="pointValue">The point value the <see cref="Enemy"/> is worth.</param>
    public delegate void EnemyDeathEventHandler(Enemy enemy, int pointValue = 0);

    /// <summary>
    /// Event fired when this enemy dies.
    /// </summary>
    public event EnemyDeathEventHandler EnemyDeath;

    /// <summary>
    /// Audio source for playing audio.
    /// </summary>
    private AudioSource _audioSource;

    /// <summary>
    /// Animator to use for triggering the explosion animation.
    /// </summary>
    private Animator _spriteAnimator;

    /// <summary>
    /// Delegate for the <see cref="Enemy.PlayerCollision"/> event.
    /// </summary>
    /// <param name="enemy">The enemy that collided with the player.</param>
    public delegate void PlayerCollisionEventHandler(Enemy enemy);

    /// <summary>
    /// Event fired with this enemy collides with the player.
    /// </summary>
    public event PlayerCollisionEventHandler PlayerCollision;

    /// <summary>
    /// Signifies if we are moving or not. If we are not moving then we are in the middle of a collision.
    /// Check this variable before collision logic to make sure only one collision is processed.
    /// <para>
    /// This should prevent double hits to the player during the explosion sequence.
    /// </para>
    /// </summary>
    private bool IsMoving => _speed > 0;

    void Start()
    {
        _spriteAnimator = gameObject.GetComponent<Animator>();
        Assert.IsNotNull(_spriteAnimator, "_spriteAnimator != null");
        _spriteAnimator.ResetTrigger(AnimationTriggers.EnemyDeath);

        _audioSource = gameObject.GetComponent<AudioSource>();
        Assert.IsNotNull(_audioSource, "_audioSource != null");
        _audioSource.gameObject.SetActive(true);
        _audioSource.enabled = true;
        //_nextFireTime = Time.time + Random.Range(3, 7); // * Time.deltaTime;
    }

    void Update()
    {
        if (!IsMoving)
        {
            return;
        }
        transform.Translate(Vector3.down * (_speed * Time.deltaTime));

        if (transform.position.y <= EnemyBounds.min.y)
        {
            transform.position = new Vector3(Random.Range(EnemyBounds.min.x, EnemyBounds.max.x), EnemyBounds.max.y, 0);
        }

        if (Time.time >= _nextFireTime)
        {
            FireLaser();
        }
    }

    /// <summary>
    /// Gets a spawn point with a random x location at the top of the screen
    /// </summary>
    /// <returns>Spawn point with the x axis randomized within <see cref="EnemyBounds"/></returns>
    public static Vector3 GetSpawnPoint()
    {
        return new Vector3(Random.Range(EnemyBounds.min.x, EnemyBounds.max.x), EnemyBounds.max.y, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (other.tag)
        {
            case "Laser":
                DoLaserCollision(other.GetComponent<Laser>());
                break;
            case "Player":
                DoPlayerCollision(other.GetComponent<Player>());
                break;
            default:
#if  DEBUG
                print("Unknown tag: " + other.tag);
#endif
                break;
        }
    }

    /// <summary>
    /// THe next minimum timestamp required to fire the laser. This is randomized between 3 and 7 seconds.
    /// </summary>
    private float _nextFireTime;
    
    void FireLaser()
    {
        //TODO: Move magic numbers to configurable variables
        _nextFireTime = Time.time + Random.Range(3, 7); // * Time.deltaTime;
        var newLaser = Instantiate(_enemylaserPrefab, transform.position, Quaternion.identity);
        newLaser.transform.parent = transform;
        _audioSource.PlayOneShot(_laserFireAudioClip);
    }
    /// <summary>
    /// Called when a <see cref="Laser"/> has collided with this enemy.
    /// </summary>
    /// <param name="laser">The laser that caused the collision.</param>
    private void DoLaserCollision(Laser laser)
    {
        if (laser == null)
        {
            print("Laser is null");
            return;
        }

        if (laser.IsEnemyLaser)
        {
            return;
        }

        Destroy(laser.gameObject);
        Die(10);
    }

    /// <summary>
    /// Called when the <see cref="Player"/> has collided with this enemy.
    /// </summary>
    /// <param name="player">The player that caused the collision.</param>
    private void DoPlayerCollision(Player player)
    {
        if (player == null)
        {
            print("Player is null.");
            return;
        }
        PlayerCollision?.Invoke(this);
        Die();
    }

    /// <summary>
    /// Called when a collision occurs that should destroy this enemy.
    /// </summary>
    /// <param name="pointValue">The point value this enemy is worth.</param>
    void Die(int pointValue = 0)
    {
        Destroy(GetComponent<Collider2D>());
        _speed = 0;
        _spriteAnimator.SetTrigger(AnimationTriggers.EnemyDeath);
        //_explosionAudio.enabled = true;
        //_explosionAudio.gameObject.SetActive(true);
        
        _audioSource.clip = _explosionAudioClip;
        _audioSource.Play();
        
        //yield return new WaitForSeconds(_spriteAnimator.GetCurrentAnimatorClipInfo(0).Length + _spriteAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime);
        Destroy(this.gameObject, 2.5f);
        EnemyDeath?.Invoke(this, pointValue);
        

    }

}
