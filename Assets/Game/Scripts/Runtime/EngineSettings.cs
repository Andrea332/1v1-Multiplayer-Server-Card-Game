using UnityEngine;

namespace Game
{
    public static class EngineSettings
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void SetEngineSettings()
        {
#if UNITY_SERVER
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
#else
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
#endif
        }
    }
}
