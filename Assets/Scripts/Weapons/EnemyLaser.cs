using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLaser : MonoBehaviour
{
    [SerializeField]
    private Laser _leftLaser;

    [SerializeField]
    private Laser _rightLaser;

    void Start()
    {
        _leftLaser.Direction = Vector3.down;
        _rightLaser.Direction = Vector3.down;
    }
}
