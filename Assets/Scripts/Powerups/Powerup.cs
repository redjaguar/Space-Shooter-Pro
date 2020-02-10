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
    /// <summary>
    /// Types of powerups available.
    /// </summary>
    public enum PowerupType
    {
        /// <summary>
        /// Shield powerup.
        /// </summary>
        Shield,
        /// <summary>
        /// Speedup powerup.
        /// </summary>
        Speedup,
        /// <summary>
        /// Triple shot powerup.
        /// </summary>
        TripleShot

    }

    /// <summary>
    /// Base class for all powerups which provides the basic functionality of being collected and notifying the player which powerup has been collected.
    /// All powerups must derive from this class. Specific behavior required for a powerup must be implemented in the child class.
    /// </summary>
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

        [SerializeField]
        private AudioClip _audioClip;

        void Start()
        {
            //_audioClip = AudioUtilities.Amplify(_audioClip, 6f);
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag.Equals("Player"))
            {
                AudioSource.PlayClipAtPoint(_audioClip, transform.position);
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
