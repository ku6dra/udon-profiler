using UdonSharp;
using UnityEngine;

[DefaultExecutionOrder(-1000000000)]
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class UdonProfilerTextureBegin : UdonSharpBehaviour
{
    [SerializeField]
    private UdonProfiler _profiler;

    // Camera: RenderTexture, Depth -100
    private void OnPostRender()
    {
        _profiler.RendeToTextureStart = Time.realtimeSinceStartupAsDouble;
    }
}
