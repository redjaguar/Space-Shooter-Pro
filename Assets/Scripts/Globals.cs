using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Helper_Classes
{

    /// <summary>
    /// Contains global variables that are not subject to change during execution
    /// </summary>
    public class Globals
    {
        static Globals()
        {
            PlayerBounds = new Bounds(new Vector3(0, -2, 0), new Vector3(23, 4));
            ScreenBounds =new Bounds(new Vector3(0, 1.25f, 0), new Vector3(18.5f, 12.5f));
        }

        /// <summary>
        /// Screen bounds for <see cref="Player"/> movement.
        /// </summary>
        public static Bounds PlayerBounds { get; }

        /// <summary>
        /// Screen bounds for the game screen.
        /// </summary>
        public static Bounds ScreenBounds { get; }

    }

    /// <summary>
    /// Contains index values for all scenes which are accessible by name.
    /// </summary>
    public static class Scenes
    {
        /// <summary>
        /// The index for the "Main Menu" scene.
        /// </summary>
        public static readonly int MainMenu = 0;

        /// <summary>
        /// The  index for the "Game" scene.
        /// </summary>
        public static readonly int Game = 1;
    }

    /// <summary>
    /// Contains cached index values for animation triggers accessible by name.
    /// </summary>
    public static class AnimationTriggers
    {
        /// <summary>
        /// Cached index value for the explosion trigger on the explosion animation clip.
        /// </summary>
        public static readonly int EnemyDeath = Animator.StringToHash("OnEnemyDeath");

    }

    public static class ExtensionMethods
    {
        public static bool IsInRange(this float value, float min, float max)
        {
            return (value >= min && value <= max);
        }

        public static bool IsInRange(this int value, float min, float max)
        {
            return (value >= min && value <= max);
        }
    }
}
