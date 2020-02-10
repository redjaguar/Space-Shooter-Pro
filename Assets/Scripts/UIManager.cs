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

    [SerializeField]
    private Sprite[] _livesSprites;

    [SerializeField]
    private Image _livesImage;
    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.FindWithTag("Player")?.GetComponent<Player>();
        Assert.IsNotNull(_player, "_player != null");

        _scoreText = GameObject.Find("Score_Text")?.GetComponent<Text>();
        Assert.IsNotNull(_scoreText, "_scoreText != null");

        
        _player.PlayerScoreChanged += (p) => _scoreText.text = $"Score: {p.Score}";
        _player.PlayerDeath += (p) =>
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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
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
