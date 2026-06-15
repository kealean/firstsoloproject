using script.Managers;
using Script.Player;
using TMPro;
using UnityEngine;

namespace script.UI {
    public class Title : MonoBehaviour {
        [SerializeField]private TextMeshProUGUI title;
        [SerializeField] private NoteManager noteManager;

        public void Initialize() {
            title.SetText($"{noteManager.MapData.composer} - {noteManager.MapData.songTitle}");
        }
    }
}