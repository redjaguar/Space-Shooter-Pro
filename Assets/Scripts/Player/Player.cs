using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Helper_Classes;
using Assets.Scripts.Powerups;
using UnityEngine;

#pragma  warning disable CS0649
public class Player : MonoBehaviour
{
    public float speed = 5f;
    
    public Bounds PlayerBounds => Globals.PlayerBounds;
    
    public delegate void PlayerDeathEventHandler(Player player);
    public event PlayerDeathEventHandler PlayerDeath;

    [SerializeField]
    private float _laserOffset = 0.8f;
    [SerializeField]
    private GameObject _laserPreFab;

    [SerializeField] 
    private GameObject _tripleShotPreFab;

    [SerializeField]
    private float _fireRate = 0.15f;
    private float _fireDelayTime = -1f;

    [SerializeField]
    private int _lives = 3;

    public int Lives => _lives;
    private bool CanFire => Time.time >= _fireDelayTime;

    [SerializeField]
    private bool _tripleShotActive;

    [SerializeField]
    private bool _speedBoostActive;
    
    
    private float _speedBoostFactor = 2f;

    [SerializeField]
    private bool _shieldsActive;

    private GameObject _shields;

    private SpriteRenderer _shieldsRenderer;

    [SerializeField]
    private int score;

    public int Score => score;

    public delegate void PlayerScoreChangedEventHandler(Player player);

    public event PlayerScoreChangedEventHandler PlayerScoreChanged;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = Vector3.zero;
        _shields = GameObject.FindWithTag("Player Shields");
        _shields.SetActive(false);
        //_shieldsRenderer= _shields.transform.GetComponent<SpriteRenderer>();
        //_shieldsRenderer.enabled = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        CalculateMovement();
        if (Input.GetKeyDown(KeyCode.Space) && CanFire)
        {
            FireLaser();
        }
    }

    void FireLaser()
    {
        _fireDelayTime = Time.time + _fireRate;
        var playerPosition = transform.position;
        var newTripleShot = Instantiate(_tripleShotActive ? _tripleShotPreFab : _laserPreFab, _tripleShotActive ? playerPosition : playerPosition + new Vector3(0, _laserOffset), Quaternion.identity);
        //print("Player position: " + playerPosition);
        //print("Triple shot position: " + newTripleShot.transform.position);
    }
    
    public void OnPowerupCollected(Powerup powerup)
    {
        switch (powerup.PowerupType)
        {
            case PowerupType.Shield:
                EnableShields();
                break;
            case PowerupType.Speedup:
                _speedBoostActive = true;
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

    public void OnEnemyDeath(Enemy enemy, int pointValue = 0)
    {
        
        score += pointValue;
        if (pointValue > 0)
        {
            PlayerScoreChanged?.Invoke(this);
        }
    }
    //TODO: See if one method will work with a "powerup type" parameter.

    IEnumerator DisableTripleShot()
    {
        yield return new WaitForSeconds(5f);
        _tripleShotActive = false;
        yield return null;
    }

    IEnumerator DisableSpeedBoost()
    {
        yield return new WaitForSeconds(5f);
        _speedBoostActive = false;
        _speedBoostFactor /= _speedBoostFactor;
        yield return null;
    }

    void EnableShields()
    {
        _shieldsActive = true;
        _shields.SetActive(true);
        //_shieldsRenderer.enabled = true;

    }

    void DisableShields()
    {
        _shieldsActive = false;
        _shields.SetActive(false);
        //_shieldsRenderer.enabled = false;

    }
    void CalculateMovement()
    {
        transform.Translate(new Vector3(Input.GetAxis(@"Horizontal"), Input.GetAxis(@"Vertical")) * (speed * Time.deltaTime));

        if (PlayerBounds.Contains(transform.position))
        {
            return;
        }

        float newX = transform.position.x;
        float newY = Mathf.Clamp(transform.position.y, PlayerBounds.min.y, PlayerBounds.max.y);

        if (newX >= PlayerBounds.max.x)
        {
            newX = PlayerBounds.min.x;
        }
        else if (newX <= PlayerBounds.min.x)
        {
            newX = PlayerBounds.max.x;
        }

        transform.position = new Vector3(newX, newY);
    }

    public void Damage()
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
            Destroy(this.gameObject);
        }
        
    }
}