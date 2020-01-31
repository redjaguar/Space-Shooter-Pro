using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Helper_Classes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Powerups
{
    public enum PowerupType
    {
        Shield,
        Speedup,
        TripleShot

    }

    public abstract class Powerup : MonoBehaviour
    {
        public delegate void PowerupCollectedEventHandler(Powerup powerup);

        public event PowerupCollectedEventHandler PowerupCollected;
        public abstract PowerupType PowerupType { get; }

        [SerializeField]
        private float powerupDurationSeconds = 5f;

        public float PowerupDurationSeconds => powerupDurationSeconds;

        [SerializeField]
        private float powerupSpeed = 3f;

        public float PowerupSpeed => powerupSpeed;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag.Equals("Player"))
            {
                PowerupCollected?.Invoke(this);
                Destroy(this.gameObject);
            }
        }

        /// <summary>
        /// Returns this powerup as type T.
        /// </summary>
        /// <typeparam name="T">Powerup type to return.</typeparam>
        /// <returns>This powerup as T or null if this powerup cannot be cast to type T.</returns>
        public T GetPowerupObject<T>() where T : Powerup
        {
            return this as T;
        }

        void Update()
        {
            transform.Translate(Vector3.down * (powerupSpeed * Time.deltaTime));
            if (!Globals.ScreenBounds.Contains(transform.position))
            {
                Destroy(this.gameObject);
            }
        }
    }
    
    
}
