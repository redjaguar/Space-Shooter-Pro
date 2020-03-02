using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using Assets.Helper_Classes;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    
    private Player _player;
    private SpawnManager _spawnManager;
    private Text _scoreText;
    private Text _gameOverText;
    private Text _restartText;
    private Slider _fuelSlider;
    private Text _ammoLevelText;

    [SerializeField]
    private Sprite[] _livesSprites;

    [SerializeField]
    private Image _livesImage;

    //Green : >10
    private static readonly string AmmoGreen = "#51E526";
    private static Color AmmoGreenColor;
        
    //Yellow : >5
    private static readonly string AmmoYellow = "#ECFF1A";
    private static Color AmmoYellowColor;

    //Red >0
    private static readonly string AmmoOrange = "#C68B34";
    private static Color AmmoOrangeColor;
    
    //DarkRed = 0
    private static readonly string AmmoRed = "#FF1F00";
    private static Color AmmoRedColor;

    private Color GetAmmoColor(int shotsLeft)
    {
        if (shotsLeft > 10)
        {
            return AmmoGreenColor;
        }

        if (shotsLeft > 5)
        {
            return AmmoYellowColor;
        }

        if (shotsLeft > 0)
        {
            return AmmoOrangeColor;
        }

        return AmmoRedColor;
    }
    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.FindWithTag("Player")?.GetComponent<Player>();
        Assert.IsNotNull(_player, "_player != null");

        _scoreText = GameObject.Find("Score_Text")?.GetComponent<Text>();
        Assert.IsNotNull(_scoreText, "_scoreText != null");

        _fuelSlider = GameObject.Find("Thruster_Fuel_Slider").GetComponent<Slider>();
        Assert.IsNotNull(_fuelSlider, "_fuelSlider != null");
        
        _player.PlayerScoreChanged += (p) => _scoreText.text = $"Score: {p.Score}";
        _player.PlayerLivesChanged += (p) =>
        {
            if (p.Lives < 0)
            {
                return;
            }

            _livesImage.sprite = _livesSprites[p.Lives];
            if (p.Lives == 0)
            {
                _gameOverText.gameObject.SetActive(true);
                _restartText.gameObject.SetActive(true);
                StartCoroutine(GameOverFlicker());
                StartCoroutine(CheckForReset());
            }
        };
        _player.ShotFired += shotsLeft =>
        {
            _ammoLevelText.text = shotsLeft.ToString();
            _ammoLevelText.color = GetAmmoColor(shotsLeft);
        };
        _player.Thruster.FuelLevelChanged += fuelLevel =>
        {
            //Debug.Log($"_fuelSlider.value before update: {fuelLevel}");
            _fuelSlider.value = fuelLevel;
            //Debug.Log($"_fuelSlider.value after update: {fuelLevel}");
        };
        _scoreText.text = "Score: 0";
        _livesImage.sprite = _livesSprites[_livesSprites.Length - 1];

        _gameOverText = GameObject.Find("Game_Over_Text")?.GetComponent<Text>();
        Assert.IsNotNull(_gameOverText, "_gameOverText != null");
        _gameOverText.gameObject.SetActive(false);
        _spawnManager = GameObject.Find("Spawn_Manager")?.GetComponent<SpawnManager>();
        Assert.IsNotNull(_spawnManager, "_spawnManager != null");

        _restartText = GameObject.Find("Restart_Text")?.GetComponent<Text>();
        Assert.IsNotNull(_restartText, "_restartText != null");
        _restartText.gameObject.SetActive(false);

        _ammoLevelText = GameObject.Find("Ammo_Level_Text")?.GetComponent<Text>();
        Assert.IsNotNull(_ammoLevelText, "_ammoLevelText != null");

        ColorUtility.TryParseHtmlString(AmmoGreen, out AmmoGreenColor);
        ColorUtility.TryParseHtmlString(AmmoYellow, out AmmoYellowColor);
        ColorUtility.TryParseHtmlString(AmmoOrange, out AmmoOrangeColor);
        ColorUtility.TryParseHtmlString(AmmoRed, out AmmoRedColor);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //TODO pause everything and show confirm dialog
            Application.Quit();
        }
    }

    IEnumerator GameOverFlicker()
    {
        while (true)
        {

            _gameOverText.enabled = !_gameOverText.enabled;
            yield return new WaitForSeconds(.5f);
        }
    }

    IEnumerator CheckForReset()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(Scenes.Game);
            }

            yield return null; //new WaitForEndOfFrame();
        }
    }

    void Restart()
    {
        StopCoroutine(GameOverFlicker());
        StopCoroutine(CheckForReset());
        _gameOverText.gameObject.SetActive(false);
        _livesImage.sprite = _livesSprites[_livesSprites.Length - 1];
        _scoreText.text = "Score: 0";
        
        //SpawnManager.Restart();
        //Player.Restart();
    }
   
}
