using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Helper class for useful audio functions.
    /// </summary>
    [PublicAPI]
    public class AudioUtilities
    {
        /// <summary>
        /// Amplifies a given audio clip by <paramref name="amplificationFactor"/>.
        /// <para>
        /// The default factor is 2 so the sound volume of the clip will be doubled.
        /// </para>
        /// </summary>
        /// <param name="audioClip">The <see cref="AudioClip"/> to amplify.</param>
        /// <param name="amplificationFactor">Amplification factor. Default is 2.</param>
        /// <returns>The amplified <see cref="AudioClip"/>. The return value will always be the same AudioClip instance passed in via <paramref name="audioClip"/> </returns>
        [PublicAPI]
        public static AudioClip Amplify(AudioClip audioClip, float amplificationFactor = 2)
        {
            var sampleData = new float[audioClip.samples];
            audioClip.GetData(sampleData, 0);
            for (int i= 0; i < sampleData.Length; i++)
            {
                sampleData[i] *= 2;
            }

            audioClip.SetData(sampleData, 0);
            return audioClip;
        }
    }
}
