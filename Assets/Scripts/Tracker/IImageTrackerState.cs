using UnityEngine;

public interface IImageTrackerState
{
    void Enter();
    void Exit();
    void OnFound(TrackedTarget target);
    void OnLost(TrackedTarget target);
}


public class SingleTrackingState : IImageTrackerState
{
    private readonly ImageTracker _owner;
    public SingleTrackingState(ImageTracker owner) => _owner = owner;

    public void Enter()
    {
        _owner.SetSecondaryTarget(null);
        _owner.SetMaxTrackingTargets(1);
    }

    public void Exit() { }

    public void OnFound(TrackedTarget target)
    {
        _owner.ShowInfoUI(true);

        _owner.SetCurrentTarget(target);
        _owner.SetPrimaryTarget(target);
    }

    public void OnLost(TrackedTarget target)
    {
        if (_owner.CurrentTarget == target)
        {
            _owner.SetCurrentTarget(null);
            _owner.ShowInfoUI(false);
        }

        _owner.SetPrimaryTarget(null);
        _owner.SetSecondaryTarget(null);
    }
}

public class CombineSearchingState : IImageTrackerState
{
    private readonly ImageTracker _owner;
    public CombineSearchingState(ImageTracker owner) => _owner = owner;

    public void Enter() => _owner.SetMaxTrackingTargets(2);
    public void Exit() { }

    public void OnFound(TrackedTarget target)
    {
        if (_owner.PrimaryTarget != null && target == _owner.PrimaryTarget)
        {
            _owner.SetCurrentTarget(target);
            return;
        }

        // secondary 확정
        _owner.SetSecondaryTarget(target);

        // CombineManager에게 "secondary found" 이벤트로 알림
        if (_owner.PrimaryTarget != null)
        {
            _owner.RaiseCombineSecondaryFound(_owner.PrimaryTarget, target);
        }
    }

    public void OnLost(TrackedTarget target)
    {
        // primary 잃으면 이벤트로 알림
        if (_owner.PrimaryTarget != null && target == _owner.PrimaryTarget)
        {
            _owner.RaiseCombinePrimaryLost(target);
            return;
        }

        // secondary는 탐색 중에 잃으면 null 처리 + 이벤트(원하면)도 가능
        if (_owner.SecondaryTarget != null && target == _owner.SecondaryTarget)
        {
            _owner.SetSecondaryTarget(null);
            _owner.RaiseCombineSecondaryLost(target);
        }
    }
}

public class CombineDetectingState : IImageTrackerState
{
    private readonly ImageTracker _owner;
    public CombineDetectingState(ImageTracker owner) => _owner = owner;

    public void Enter() => _owner.SetMaxTrackingTargets(2);
    public void Exit() { }

    public void OnFound(TrackedTarget target)
    {
        _owner.SetCurrentTarget(target);
    }

    public void OnLost(TrackedTarget target)
    {
        if (_owner.PrimaryTarget != null && target == _owner.PrimaryTarget)
        {
            _owner.RaiseCombinePrimaryLost(target);
            return;
        }

        if (_owner.SecondaryTarget != null && target == _owner.SecondaryTarget)
        {
            _owner.RaiseCombineSecondaryLost(target);
            return;
        }
    }
}