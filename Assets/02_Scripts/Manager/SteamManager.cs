using UnityEngine;
using Steamworks;

namespace Assets.Scripts.Facepunch
{
    public class SteamManager : MonoBehaviour
    {
        public static SteamManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {              
                try
                {
                    SteamClient.Init(1493240,false);                    
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(e.Message);
                    Destroy(gameObject);
                    return;
                }
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update() 
        {
            SteamClient.RunCallbacks();    
        }

        private void OnDestroy()
        {
            SteamClient.Shutdown();
        }
    }
}