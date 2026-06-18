using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace script.Lib.Sound {
    public class VolumeSlider : MonoBehaviour {
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private string param;

        public void SliderChange() {
            mixer.SetFloat(param, volumeSlider.value <= volumeSlider.minValue ? -80f : volumeSlider.value);
        }
    }
}