using System;
using System.Collections.Generic;
using script.Lib;
using script.Lib.Pooling;
using UnityEngine;

namespace script.Managers {
    public class SoundManager : MonoSingleton<SoundManager> {
        [SerializeField] private PoolItemSO soundPlayerItem;
        
        private Dictionary<int, SoundPlayer> _items;

        public void PlaySfx(Vector3 position, SoundClipSO soundClip) {
            SoundPlayer soundPlayer = PoolManager.Instance.Pop(soundPlayerItem.ItemName) as  SoundPlayer;
            soundPlayer.transform.position = position;
            soundPlayer.PlaySound(soundClip);
            soundPlayer.OnClipEnd += HandleClipEnd;
        }

        private void HandleClipEnd(SoundPlayer targetPlayer) {
            targetPlayer.OnClipEnd -= HandleClipEnd;
            PoolManager.Instance.Push(targetPlayer);
        }
    }
}