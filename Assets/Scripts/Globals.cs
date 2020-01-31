using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Helper_Classes
{

    public class Globals
    {
        static Globals()
        {
            PlayerBounds = new Bounds(new Vector3(0, -2, 0), new Vector3(23, 4));
            ScreenBounds =new Bounds(new Vector3(0, 1.25f, 0), new Vector3(18.5f, 12.5f));
        }

        public static Bounds PlayerBounds { get; }
        public static Bounds ScreenBounds { get; }
        public static int NumberOfPlayerStartingLives => 3;
    }

    public static class Scenes
    {
        public static readonly int MainMenu = 0;
        public static readonly int Game = 1;
    }
}
