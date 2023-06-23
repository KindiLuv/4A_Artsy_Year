using UnityEngine;

namespace Assets.Scripts.Systems
{
    public static class ServerSystem
    {
        public static bool IsInstanciate { get; private set; } = false;
        public static bool IsServerBuild { get; private set; } = false;
        public static bool IsGraphicsBuild { get; private set; } = true;
        public static int Port = 7776;
        public static string Ip = "192.168.1.42";

        public static void Instanciate()
        {
            if (!IsInstanciate)
            {                
                IsServerBuild = SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null;
#if UNITY_EDITOR
                IsServerBuild = PlayerPrefs.GetInt("ServerBuildMode", 0) == 1;
#endif
                IsGraphicsBuild = !IsServerBuild;
                if (IsServerBuild)
                {
                    Debug.Log("");
                    Debug.Log("------------------------------------------------------------");
                    Debug.Log("SERVER BUILD");
                    Debug.Log("------------------------------------------------------------");
                    string p = GetArg("-port");
                    if(p != null && p.Length > 0)
                    {
                        Port = int.Parse(p);
                    }
                    else
                    {
                        Port++;
                    }
                    Debug.Log("Port : " + Port);
                }
                IsInstanciate = true;
            }
        }

        private static string GetArg(string name)
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
            return null;
        }

    }
}