using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Asteroid : MonoBehaviour
{
    private float rotationSpeed = 20;
    private float rotationAngle = 30 * Mathf.Deg2Rad;
    private Vector3 zAsxisVector = new Vector3(0,0,1);

    private GameObject explosion;

    public delegate void AsteroidExplodedEventHandler(Asteroid asteroid);

    public event AsteroidExplodedEventHandler AsteroidExploded;

    void Start()
    {
        explosion = Resources.Load<GameObject>("Prefabs/Explosion");
        Assert.IsNotNull(explosion);
    }

    // Update is called once per frame
    void Update()
    {
        //transform.Rotate(Vector3.forward, rotationAngle);    
        transform.Rotate(Vector3.forward * (rotationSpeed * Time.deltaTime));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag.Equals("Laser"))
        {
            var newExplosion = Instantiate(explosion, transform.position, Quaternion.identity);
            //newExplosion.transform.parent = transform;
            Destroy(other.gameObject);
            Destroy(this.gameObject, .5f);
        }
    }

    private void OnDestroy()
    {
        AsteroidExploded?.Invoke(this);
    }
}
