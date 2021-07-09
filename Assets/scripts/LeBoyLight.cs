using UnityEngine;
using UnityEngine.UI;

namespace LeBoy.Unity
{
    public class LeBoyLight : MonoBehaviour
    {
        public LeBoyScript leboy;
        public Image gbLight;

        private void Update()
        {
            gbLight.enabled = leboy != null && leboy.IsOn;
        }
    }
}