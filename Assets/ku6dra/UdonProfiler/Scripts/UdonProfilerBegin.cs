using UdonSharp;
using UnityEngine;

[DefaultExecutionOrder(-1000000000)]
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class UdonProfilerBegin : UdonSharpBehaviour
{
    [SerializeField]
    private UdonProfiler _profiler;

    private void Start()
    {
        // Enable Screen Camera
        gameObject.GetComponent<Camera>().enabled = true;
    }

    private void FixedUpdate()
    {
        // FixedUpdate hasn't been executed yet in this frame
        if (_profiler.FixedUpdateStart < 0d)
        {
            _profiler.FixedUpdateStart = Time.realtimeSinceStartupAsDouble;
        }
    }

    private void Update()
    {
        _profiler.UpdateStart = Time.realtimeSinceStartupAsDouble;
    }

    private void LateUpdate()
    {
        _profiler.LateUpdateStart = Time.realtimeSinceStartupAsDouble;
    }

    public override void PostLateUpdate()
    {
        _profiler.PostLateUpdateStart = Time.realtimeSinceStartupAsDouble;
    }

    // Camera: Screen, Depth -100
    private void OnPreCull()
    {
        _profiler.RenderToScreenStart = Time.realtimeSinceStartupAsDouble;
    }
}
