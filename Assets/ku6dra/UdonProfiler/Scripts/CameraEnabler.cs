using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class CameraEnabler : UdonSharpBehaviour
{
    [SerializeField]
    private Camera _targetCamera;

    private void Start()
    {
        _EnableCamera();
    }

    public void _EnableCamera()
    {
        _targetCamera.enabled = true;
    }
}
