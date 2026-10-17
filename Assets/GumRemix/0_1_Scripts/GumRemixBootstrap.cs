using UnityEngine;

namespace GumRemix
{
    /// <summary>Entry point: put this on one GameObject in an empty scene and press Play.</summary>
    public class GumRemixBootstrap : MonoBehaviour
    {
        void Awake()
        {
            Application.targetFrameRate = 60; QualitySettings.vSyncCount = 1;
            var go = new GameObject("GumRemixGame");
            go.AddComponent<GameCtl>();
        }
    }
}
