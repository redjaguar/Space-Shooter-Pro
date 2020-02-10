using System.Collections;
using System.Collections.Generic;
using Assets.Helper_Classes;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField]
    private float _speed = 8f;

    private readonly float _maxY = 8.0f;
    // Update is called once per frame

    public Vector3 Direction { get; set; } = Vector3.up;
    public bool IsEnemyLaser => Direction == Vector3.down;

    //public delegate void LaserCollisionEventHandler(Laser laser);
    //public event LaserCollisionEventHandler LaserCollision;

    void Update()
    {
        transform.Translate(Direction * (_speed * Time.deltaTime));

        if(!Globals.ScreenBounds.Contains(transform.position))
        {
            if (transform.parent != null)
            {
                Destroy(transform.parent.gameObject);
            }

            Destroy(this.gameObject);
        }
    }
}
