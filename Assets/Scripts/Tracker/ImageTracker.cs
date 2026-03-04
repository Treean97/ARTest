using System;
using System.Collections.Generic;
using UnityEngine;
using easyar;

public class ImageTracker : MonoBehaviour
{
    public InfoUI _InfoUI;

    [SerializeField] private ImageTrackerFrameFilter _FrameFilter;

    private TrackedTarget _CurrentTarget;
    private TrackedTarget _PrimaryTarget;
    private TrackedTarget _SecondaryTarget;

    private readonly List<ImageTargetController> _Controllers = new();

    // 상태
    private IImageTrackerState _State;
    private SingleTrackingState _SingleState;
    private CombineSearchingState _SearchingState;
    private CombineDetectingState _DetectingState;

    public TrackedTarget CurrentTarget => _CurrentTarget;
    public TrackedTarget PrimaryTarget => _PrimaryTarget;
    public TrackedTarget SecondaryTarget => _SecondaryTarget;

    // CombineManager가 구독할 이벤트
    public event Action<TrackedTarget, TrackedTarget> OnCombineSecondaryFound;
    public event Action<TrackedTarget> OnCombinePrimaryLost;
    public event Action<TrackedTarget> OnCombineSecondaryLost;

    void Awake()
    {
        _SingleState = new SingleTrackingState(this);
        _SearchingState = new CombineSearchingState(this);
        _DetectingState = new CombineDetectingState(this);

        SetState(_SingleState);

        CacheControllersAndBindEvents();
        LoadTargets();
    }

    private void CacheControllersAndBindEvents()
    {
        _Controllers.Clear();

#if UNITY_2022_3_OR_NEWER
        var found = GameObject.FindObjectsByType<ImageTargetController>(FindObjectsSortMode.None);
#else
        var found = GameObject.FindObjectsOfType<ImageTargetController>();
#endif

        foreach (var controller in found)
        {
            if (controller == null) continue;

            _Controllers.Add(controller);

            controller.TargetFound += () => OnFound(controller);
            controller.TargetLost  += () => OnLost(controller);
        }
    }

    public void SetMaxTrackingTargets(int count)
    {
        if (_FrameFilter == null) return;
        _FrameFilter.SimultaneousNum = count;
    }

    public void LoadTargets()
    {
        if (_FrameFilter == null) return;

        foreach (var c in _Controllers)
        {
            if (c == null) continue;
            c.Tracker = _FrameFilter;
        }
    }

    private void SetState(IImageTrackerState newState)
    {
        if (newState == null || _State == newState) return;

        _State?.Exit();
        _State = newState;
        _State.Enter();
    }

    public void EnterSingleTracking() => SetState(_SingleState);

    public void EnterCombineSearching(TrackedTarget primary)
    {
        SetPrimaryTarget(primary);
        SetSecondaryTarget(null);
        SetState(_SearchingState);
    }

    public void EnterCombineDetecting(TrackedTarget primary, TrackedTarget secondary)
    {
        SetPrimaryTarget(primary);
        SetSecondaryTarget(secondary);
        SetState(_DetectingState);
    }

    public void ShowInfoUI(bool isOn)
    {
        if (_InfoUI == null) return;
        if (_InfoUI.gameObject.activeSelf == isOn) return;
        _InfoUI.gameObject.SetActive(isOn);
    }

    public void SetCurrentTarget(TrackedTarget target) => _CurrentTarget = target;
    public void SetPrimaryTarget(TrackedTarget target) => _PrimaryTarget = target;
    public void SetSecondaryTarget(TrackedTarget target) => _SecondaryTarget = target;

    // 상태에서만 쓰는 이벤트 발행 메서드
    public void RaiseCombineSecondaryFound(TrackedTarget primary, TrackedTarget secondary)
        => OnCombineSecondaryFound?.Invoke(primary, secondary);

    public void RaiseCombinePrimaryLost(TrackedTarget primary)
        => OnCombinePrimaryLost?.Invoke(primary);

    public void RaiseCombineSecondaryLost(TrackedTarget secondary)
        => OnCombineSecondaryLost?.Invoke(secondary);

    public void OnFound(ImageTargetController controller)
    {
        TrackedTarget target = controller.GetComponentInChildren<TrackedTarget>();
        if (target == null) return;

        target.OnFound();
        _State?.OnFound(target);
    }

    public void OnLost(ImageTargetController controller)
    {
        TrackedTarget target = controller.GetComponentInChildren<TrackedTarget>();
        if (target == null) return;

        _State?.OnLost(target);
        target.OnLost();
    }
}