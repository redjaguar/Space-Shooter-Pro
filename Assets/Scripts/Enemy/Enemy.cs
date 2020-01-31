using System.Collections;
using System.Collections.Generic;
using Assets.Helper_Classes;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float _speed = 4f;

    public static Bounds EnemyBounds => Globals.ScreenBounds;
    public delegate void EnemyDeathEventHandler(Enemy enemy, int pointValue = 0);

    public event EnemyDeathEventHandler EnemyDeath;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime);

        if (transform.position.y <= EnemyBounds.min.y)
        {
            transform.position = new Vector3(Random.Range(EnemyBounds.min.x, EnemyBounds.max.x), EnemyBounds.max.y, 0);
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
                HandleLaser(other.GetComponent<Laser>());
                break;
            case "Player":
                HandlePlayer(other.GetComponent<Player>());
                break;
            default:
#if  DEBUG
                print("Unknown tag: " + other.tag);
#endif
                break;
        }
    }

    private void HandleLaser(Laser laser)
    {
        if (laser == null)
        {
            print("Laser is null");
            return;
        }
        Destroy(laser.gameObject);
        Die(10);
    }

    private void HandlePlayer(Player player)
    {
        if (player == null)
        {
            print("Player is null.");
            return;
        }
        player.Damage();
        Die();
    }

    private void Die(int pointValue = 0)
    {
        Destroy(this.gameObject);
        EnemyDeath?.Invoke(this, pointValue);
    }
}
