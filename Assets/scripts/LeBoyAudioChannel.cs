using LeBoyLib;
using UnityEngine;
using UnityEngine.Serialization;

namespace LeBoy.Unity
{
    public class LeBoyAudioChannel : MonoBehaviour
    {
        public int channel = 1;

        [FormerlySerializedAs("audio")]
        public AudioSource audioSource;

        private float[] audioBuffer;

        void Awake()
        {
            audioBuffer = new float[GBZ80.SPUSampleRate * 2];

            // Prepare sounds
            audioSource.clip = AudioClip.Create("GB", audioBuffer.Length,
                2, // Stereo
                GBZ80.SPUSampleRate, false);
            audioSource.Play();
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = audioBuffer[i];
            }
        }

        public void SetAudioBuffer(short[] buffer, int count)
        {
            if (enabled == false || audioBuffer == null) return;

            for (int i = 0; i < audioBuffer.Length; i++)
            {
                if (i < count)
                {
                    float f = buffer[i] / (float) short.MaxValue;
                    audioBuffer[i] = f;
                }
                else
                {
                    audioBuffer[i] = 0;
                }
            }
        }
    }
}