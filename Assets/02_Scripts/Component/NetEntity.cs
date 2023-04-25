using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace ArtsyNetcode
{
    public class NetEntity : NetworkBehaviour
    {
        protected virtual void Awake()
        {
            GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        protected virtual void OnDestroy()
        {
            GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        public virtual void OnGameStateChanged(GameState newGameState)
        {
            enabled = newGameState == GameState.GamePlay;
        }
    }
}
