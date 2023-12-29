using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[DefaultExecutionOrder(1000000000)]
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class UdonProfiler : UdonSharpBehaviour
{
    private const int profilerLinePreStart = 0;
    private const int profilerLineCPU = 1;
    private const int profilerLineGPUWait = 2;
    private const int profilerLinePreviousFrameEnd = 3;
    private const int profilerLineCurrentFrameStart = 4;
    private const int profilerLineFixedUpdate = 5;
    private const int profilerLineUpdate = 6;
    private const int profilerLineLateUpdate = 7;
    private const int profilerLinePostLateUpdate = 8;
    private const int profilerLineRender = 9;
    private const int profilerLineRenderBetween = 10;

    private const int numberDisplayFrameCount = 0;
    private const int numberDisplayPushedFrames = 1;
    private const int numberDisplayTotalMilliseconds = 2;
    private const int numberDisplayCpuMilliseconds = 3;
    private const int numberDisplayGpuMilliseconds = 4;

    [SerializeField]
    private RectTransform[] _profilerLines = new RectTransform[11];

    [SerializeField]
    private MeshRenderer[] _numberDisplayMeshes = new MeshRenderer[5];

    // Shared with Other Classes
    [NonSerialized]
    public double FixedUpdateStart;

    [NonSerialized]
    public double UpdateStart;

    [NonSerialized]
    public double LateUpdateStart;

    [NonSerialized]
    public double PostLateUpdateStart;

    [NonSerialized]
    public double RendeToTextureStart;

    [NonSerialized]
    public double RenderToScreenStart;

    private double _postLateUpdateEnd;
    private double _renderToTextureEnd;
    private double _renderToScreenEnd;

    private double _fixedUpdateDuration;
    private double _updateDuration;
    private double _lateUpdateDuration;
    private double _postLateUpdateDuration;

    private double _previousFrameEnd;
    private double _currentFrameStart;

    private int _previousFrameCount;
    private int _nextTimeAsInt;
    private int _pushedFramesPerSecond;

    private float _currentFloatResolution;
    private float _drawLineMinSize;

    private int _previousTotalNumber;
    private int _previousGpuNumber;
    private int _previousCpuNumber;

    private MaterialPropertyBlock[] materialPropertyBlocks = new MaterialPropertyBlock[5];

    private int _shaderPropertyId_Number;

    private void Start()
    {
        // Enable Screen Camera
        gameObject.GetComponent<Camera>().enabled = true;

        int i = 0;

        // Calculate Time Resolution
        int currentFloatTime = (int)Time.unscaledTime + 1;
        while (currentFloatTime >> i != 0)
        {
            i++;
        }
        _currentFloatResolution = (1 << i) / 16384f;
        _drawLineMinSize = 1f;
        //_drawLineMinSize = Mathf.Max(_currentFloatResolution * 10, 1);
        Debug.Log($"[UdonProfiler] Game Uptime: {currentFloatTime} seconds. Current Float Resolution: {_currentFloatResolution} ms.");

        // Initialize MaterialPropertyBlock
        _shaderPropertyId_Number = VRCShader.PropertyToID("_Number");
        for (i = 0; i < 5; i++)
        {
            materialPropertyBlocks[i] = new MaterialPropertyBlock();
            _numberDisplayMeshes[i].GetPropertyBlock(materialPropertyBlocks[i]);
        }

        materialPropertyBlocks[numberDisplayFrameCount].SetInteger("_ZeroFill", 1);
    }

    private void FixedUpdate()
    {
        _fixedUpdateDuration = Time.realtimeSinceStartupAsDouble - FixedUpdateStart;
    }

    private void Update()
    {
        _updateDuration = Time.realtimeSinceStartupAsDouble - UpdateStart;
    }

    private void LateUpdate()
    {
        _lateUpdateDuration = Time.realtimeSinceStartupAsDouble - LateUpdateStart;
    }

    public override void PostLateUpdate()
    {
        _postLateUpdateEnd = Time.realtimeSinceStartupAsDouble;
        _postLateUpdateDuration = _postLateUpdateEnd - PostLateUpdateStart;
    }

    public void _OnRenderToTextureFinshed()
    {
        _renderToTextureEnd = Time.realtimeSinceStartupAsDouble;
    }

    // Camera: Screen, Depth 98
    private void OnPostRender()
    {
        _currentFrameStart = Time.unscaledTimeAsDouble;
        _renderToScreenEnd = Time.realtimeSinceStartupAsDouble;

        double cpuStart = FixedUpdateStart < 0d ? UpdateStart : FixedUpdateStart;

        _profilerLines[profilerLineGPUWait].anchoredPosition = new Vector2((float)((_postLateUpdateEnd - cpuStart) * 5000), -1);
        _profilerLines[profilerLinePreviousFrameEnd].anchoredPosition = new Vector2((float)((_previousFrameEnd - cpuStart) * 5000), -1);
        _profilerLines[profilerLineCurrentFrameStart].anchoredPosition = new Vector2((float)((_currentFrameStart - cpuStart) * 5000), -1);
        _profilerLines[profilerLineFixedUpdate].anchoredPosition = new Vector2((float)((FixedUpdateStart - cpuStart) * 5000), -3);
        _profilerLines[profilerLineUpdate].anchoredPosition = new Vector2((float)((UpdateStart - cpuStart) * 5000), -4);
        _profilerLines[profilerLineLateUpdate].anchoredPosition = new Vector2((float)((LateUpdateStart - cpuStart) * 5000), -5);
        _profilerLines[profilerLinePostLateUpdate].anchoredPosition = new Vector2((float)((PostLateUpdateStart - cpuStart) * 5000), -6);
        _profilerLines[profilerLineRender].anchoredPosition = new Vector2((float)((RendeToTextureStart - cpuStart) * 5000), -7);
        _profilerLines[profilerLineRenderBetween].anchoredPosition = new Vector2((float)((_renderToTextureEnd - cpuStart) * 5000), -7);

        _profilerLines[profilerLinePreStart].sizeDelta = new Vector2(Mathf.Max((float)((cpuStart - _previousFrameEnd) * 5000), _drawLineMinSize), 2);
        _profilerLines[profilerLineCPU].sizeDelta = new Vector2(Mathf.Max((float)((_renderToScreenEnd - cpuStart) * 5000), _drawLineMinSize), 2);
        _profilerLines[profilerLineGPUWait].sizeDelta = new Vector2((float)((RendeToTextureStart - _postLateUpdateEnd) * 5000), 2);
        _profilerLines[profilerLineFixedUpdate].sizeDelta = new Vector2(Mathf.Max((float)(_fixedUpdateDuration * 5000), _drawLineMinSize), 1);
        _profilerLines[profilerLineUpdate].sizeDelta = new Vector2(Mathf.Max((float)(_updateDuration * 5000), _drawLineMinSize), 1);
        _profilerLines[profilerLineLateUpdate].sizeDelta = new Vector2(Mathf.Max((float)(_lateUpdateDuration * 5000), _drawLineMinSize), 1);
        _profilerLines[profilerLinePostLateUpdate].sizeDelta = new Vector2(Mathf.Max((float)(_postLateUpdateDuration * 5000), _drawLineMinSize), 1);
        _profilerLines[profilerLineRender].sizeDelta = new Vector2(Mathf.Max((float)((_renderToScreenEnd - RendeToTextureStart) * 5000), _drawLineMinSize), 1);
        _profilerLines[profilerLineRenderBetween].sizeDelta = new Vector2(Mathf.Max((float)((RenderToScreenStart - _renderToTextureEnd) * 5000), 1), 1);

        materialPropertyBlocks[numberDisplayFrameCount].SetInteger(_shaderPropertyId_Number, Time.frameCount % 10000);
        _numberDisplayMeshes[numberDisplayFrameCount].SetPropertyBlock(materialPropertyBlocks[numberDisplayFrameCount]);

        if (_nextTimeAsInt < _currentFrameStart)
        {
            _pushedFramesPerSecond = Time.frameCount - _previousFrameCount;
            _previousFrameCount = Time.frameCount;
            _nextTimeAsInt = (int)_currentFrameStart + 1;
            materialPropertyBlocks[numberDisplayPushedFrames].SetInteger(_shaderPropertyId_Number, _pushedFramesPerSecond);
            _numberDisplayMeshes[numberDisplayPushedFrames].SetPropertyBlock(materialPropertyBlocks[numberDisplayPushedFrames]);
        }

        int number = (int)((_renderToScreenEnd - _previousFrameEnd) * 1000);
        if (number != _previousTotalNumber)
        {
            materialPropertyBlocks[numberDisplayTotalMilliseconds].SetInteger(_shaderPropertyId_Number, number);
            _numberDisplayMeshes[numberDisplayTotalMilliseconds].SetPropertyBlock(materialPropertyBlocks[numberDisplayTotalMilliseconds]);
            _previousTotalNumber = number;
        }
        number = (int)((cpuStart - _previousFrameEnd + RendeToTextureStart - _postLateUpdateEnd) * 1000);
        if (number != _previousGpuNumber)
        {
            materialPropertyBlocks[numberDisplayGpuMilliseconds].SetInteger(_shaderPropertyId_Number, number);
            _numberDisplayMeshes[numberDisplayGpuMilliseconds].SetPropertyBlock(materialPropertyBlocks[numberDisplayGpuMilliseconds]);
            _previousGpuNumber = number;
        }
        number = _previousTotalNumber - _previousGpuNumber;
        if (number != _previousCpuNumber)
        {
            materialPropertyBlocks[numberDisplayCpuMilliseconds].SetInteger(_shaderPropertyId_Number, number);
            _numberDisplayMeshes[numberDisplayCpuMilliseconds].SetPropertyBlock(materialPropertyBlocks[numberDisplayCpuMilliseconds]);
            _previousCpuNumber = number;
        }

        FixedUpdateStart = double.MinValue;
        _previousFrameEnd = _renderToScreenEnd;
    }
}
