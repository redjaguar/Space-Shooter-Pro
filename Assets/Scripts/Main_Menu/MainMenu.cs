using System.Collections;
using System.Collections.Generic;
using Assets.Helper_Classes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //scene 0 = main menu
    //scene 1 = game


    public void StartNewGame()
    {
        SceneManager.LoadScene(Scenes.Game);
    }
}
