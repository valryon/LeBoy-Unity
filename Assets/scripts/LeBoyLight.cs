using UnityEngine;
using UnityEngine.UI;

namespace LeBoy.Unity
{
    public class LeBoyLight : MonoBehaviour
    {
        public LeBoyScript leboy;
        public Image light;

        private void Update()
        {
            light.enabled = leboy != null && leboy.IsOn;
        }
    }
}