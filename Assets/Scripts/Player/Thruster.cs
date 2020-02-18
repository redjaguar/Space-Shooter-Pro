using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

public class Thruster : MonoBehaviour
{
    private AudioSource _audioSource;

    private Vector3 _originalScale;
    private Vector3 _originalPosition;

    private readonly Vector3 _thrustersOnScale = Vector3.one;
    private readonly Vector3 _thrustersOnPosition = new Vector3(-0.02f, -2.97f);

    public bool IsFiring { get; private set; }

    public delegate void ThrustersExhaustedEventHandler();

    public event ThrustersExhaustedEventHandler ThrustersExhausted;

    public delegate void FuelLevelChangedEventHandler(float fuelLevel);

    public event FuelLevelChangedEventHandler FuelLevelChanged;

    [SerializeField] private float _speedScalar = 2f;

    private float _duration = 15f;
    private float _fuelLevel = 1f;
    private float _burnRatePerSecond;

    public float SpeedScalar => _speedScalar;
    // Start is called before the first frame update

    void Start()
    {
        _burnRatePerSecond = (1f / _duration);
        _originalScale = transform.localScale;
        _originalPosition = transform.localPosition;
        _audioSource = gameObject.GetComponent<AudioSource>();
        Assert.IsNotNull(_audioSource, "_audioSource != null");
    }

    public void TurnOn()
    {
        if (_fuelLevel <= 0)
        {
            return;
        }

        transform.localScale = _thrustersOnScale;
        transform.localPosition = _thrustersOnPosition;
        _audioSource.Play();
        IsFiring = true;
    }

    public void TurnOff()
    {
        transform.localScale = _originalScale;
        transform.localPosition = _originalPosition;
        _audioSource.Stop();
        IsFiring = false;
    }

    void Update()
    {
        if (IsFiring)
        {
            _fuelLevel -= _burnRatePerSecond * Time.deltaTime;
            FuelLevelChanged?.Invoke(_fuelLevel);
            if (_fuelLevel > 0)
            {
                return;
            }

            _fuelLevel = 0;
            TurnOff();
            ThrustersExhausted?.Invoke();
            
        }
        else
        {
            
            if (_fuelLevel >= 1f)
            {
                return;
            }

            _fuelLevel += _burnRatePerSecond * Time.deltaTime;
            _fuelLevel = Mathf.Clamp01(_fuelLevel);
            FuelLevelChanged?.Invoke(_fuelLevel);
            
        }
    }
}