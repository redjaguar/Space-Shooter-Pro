using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Powerups;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Scripts.Player
{
    public class Shields : MonoBehaviour
    {
        
        private Vector3 _shieldFullScale = new Vector3(2.3f, 2.3f);
        private Vector3 _shieldMediumScale = new Vector3(2f, 2f);
        private Vector3 _shieldLowScale = new Vector3(1.7f, 1.7f);

        /// <summary>
        /// Current shield strength.
        /// </summary>
        private int _strength = -1;

        /// <summary>
        /// Scalar to use as the scaling factor for full strength shields.
        /// <para>
        /// Scale steps are calculated using the formula (<see cref="_fullShieldsScalar"/> - <see cref="_lowShieldsScalar"/>) / <see cref="_maximumStrength"/>
        /// </para>
        /// </summary>
        [SerializeField]
        private float _fullShieldsScalar = 2.3f;

        /// <summary>
        /// Scalar to use as the scaling factor for low strength shields.
        /// </summary>
        [SerializeField]
        private float _lowShieldsScalar = 1.7f;

        /// <summary>
        /// Scale vectors for scaling shields based on shield strength.
        /// <para>Vectors are in ascending order with [0] being the low shield scale vector and [<see cref="_maximumStrength"/>-1] being the full scale vector.</para>
        /// </summary>
        private Vector3[] _shieldScaleVectors;

        private void Awake()
        {
            CalculateScaleVectors();
        }

        private void Start()
        {
            //gameObject.SetActive(false);
        }
        private void CalculateScaleVectors()
        {
            var scalarStepSize = (_fullShieldsScalar - _lowShieldsScalar) / _maximumStrength;
            _shieldScaleVectors = new Vector3[_maximumStrength + 1];
            for (int i = 0; i < _maximumStrength + 1; i++)
            {
                var scaleValue = _lowShieldsScalar + (scalarStepSize * (i + 1));
                _shieldScaleVectors[i] = new Vector3(scaleValue, scaleValue);
            }
        }

        /// <summary>
        /// Strength of the shields when 
        /// </summary>
        [SerializeField]
        private int _maximumStrength = 2;

        /// <summary>
        /// Forces a specific shield strength. Only used in the Unity editor.
        /// </summary>
        [SerializeField]
        private int _forcedStrength = -1;
        
        /// <summary>
        /// Whether or not shields are currently active.
        /// </summary>
        public bool Active => _strength > -1 || _forcedStrength > -1;

        public void OnPowerupCollected(Powerup powerup)
        {
            if (powerup.PowerupType == PowerupType.Shield)
            {
                SetStrength(_maximumStrength);
            }
        }

        private void Update()
        {
#if DEBUG
            if (_forcedStrength > -1)
            {
                SetStrength(_forcedStrength);
            }   
#endif
        }

        private void SetStrength(int strength)
        {
            if (strength >= _shieldScaleVectors.Length)
            {
                return;
            }
            
            _strength = strength;
            gameObject.SetActive(Active);
            if (Active)
            {
                transform.localScale = _shieldScaleVectors[strength];
            }
            
        }

        public void Hit()
        {
            if (!Active)
            {
                return;
            }
            SetStrength(_strength - 1);
        }
    }
}
