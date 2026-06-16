using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace script.Managers {
    public class ResultManager : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI perfectPlus;
        [SerializeField] private TextMeshProUGUI perfect;
        [SerializeField] private TextMeshProUGUI good;
        [SerializeField] private TextMeshProUGUI poor;
        [SerializeField] private TextMeshProUGUI miss;

        [SerializeField] private TextMeshProUGUI rank;
        [SerializeField] private TextMeshProUGUI score;
        [SerializeField] private TextMeshProUGUI rate;

        [SerializeField] private TextMeshProUGUI title;
        
        private void Start() {
            title.SetText(GameManager.Instance.titleText);
            perfectPlus.SetText(GameManager.Instance.perfectPlus.ToString());
            perfect.SetText(GameManager.Instance.perfect.ToString());
            good.SetText(GameManager.Instance.good.ToString());
            poor.SetText(GameManager.Instance.poor.ToString());
            miss.SetText(GameManager.Instance.miss.ToString());
            rate.SetText($"{GameManager.Instance.rate}%");
            score.SetText(GameManager.Instance.score.ToString());
            rank.SetText(GameManager.Instance.rate >= 100 ? "S" : GameManager.Instance.rate >= 96 ? "A+" : GameManager.Instance.rate >= 93 ? "A" : GameManager.Instance.rate >= 90 ? "B" : GameManager.Instance.rate >= 86 ? "C" : GameManager.Instance.rate >= 83 ? "D" : GameManager.Instance.rate >= 80 ? "E" : "F");
        }

        private void Update() {
            if (Keyboard.current.anyKey.wasPressedThisFrame) {
                SceneManager.LoadSceneAsync(1);
            }
        }
    }
}