using System.Collections;
using UnityEngine;

public interface IObjectState
{
    void Enter();
    void UpdateState();
    void Exit();
}

public interface IPressHandler { void OnPressChanged(bool isPressing); }
public interface ISwipeHandler { void OnSwipeDelta(float deltaX, float deltaY); }
public interface ITapHandler { void OnTap(); }
public interface IDoubleTapHandler { void OnDoubleTap(); }

public class DefaultState : IObjectState, ISwipeHandler, ITapHandler, IDoubleTapHandler
{
    private readonly SpawnObject _owner;

    public DefaultState(SpawnObject owner) => _owner = owner;

    public void Enter()
    {
        _owner._SwipeInput = 0f;
        _owner.transform.localPosition = Vector3.zero;
    }

    public void UpdateState()
    {
        float speed = _owner._IsPressing ? _owner.RotationSpeed : _owner.RotationRestoreSpeed;

        Quaternion targetRot =
            _owner._DefaultModeRotation *
            Quaternion.Euler(0f, 0f, _owner._SwipeInput);

        _owner.transform.localRotation = Quaternion.Slerp(
            _owner.transform.localRotation,
            targetRot,
            Time.deltaTime * speed
        );

        if (!_owner._IsPressing)
            _owner._SwipeInput = 0f;
    }

    public void Exit() { }

    public void OnSwipeDelta(float deltaX, float deltaY)
    {
        if (!_owner._IsPressing) return;

        float combined = deltaX + deltaY;

        _owner._SwipeInput -= combined * _owner.SwipeSensitivity;
    }

    public void OnTap()
    {
        _owner.EnterEffectState();
    }

    public void OnDoubleTap()
    {
        _owner.EnterReadyToCombineState();
    }
}

public class EffectState : IObjectState, IDoubleTapHandler
{
    private readonly SpawnObject _owner;
    private Coroutine _returnRoutine;

    public EffectState(SpawnObject owner) => _owner = owner;

    public void Enter()
    {
        _returnRoutine = _owner.RunCoroutine(ReturnToDefaultAfterSeconds(_owner.EffectDuration));
        _owner.transform.localPosition = Vector3.zero;
        _owner.Effect.SetEffectStateEffect(true);
    }

    public void UpdateState()
    {
        _owner.transform.localRotation = Quaternion.Slerp(
            _owner.transform.localRotation,
            _owner._ActiveModeRotation,
            Time.deltaTime * _owner.ModeChangeSpeed
        );
    }

    public void Exit()
    {
        _owner.StopOwnerCoroutine(_returnRoutine);
        _returnRoutine = null;
        _owner.Effect.SetEffectStateEffect(false);
    }

    public void OnDoubleTap()
    {
        _owner.EnterReadyToCombineState();
    }

    private IEnumerator ReturnToDefaultAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _owner.EnterDefaultState();
    }
}

public class ReadyToCombineState : IObjectState
{
    private readonly SpawnObject _owner;

    public ReadyToCombineState(SpawnObject owner) => _owner = owner;

    public void Enter()
    {
        _owner.transform.localPosition = Vector3.zero;
        _owner.Effect.SetReadyToCombineStateEffect(true);
        if (CombineManager.Instance != null)
            CombineManager.Instance.RequestCombine(_owner);
    }

    public void UpdateState()
    {
        _owner.transform.localRotation = Quaternion.Slerp(
            _owner.transform.localRotation,
            _owner._ActiveModeRotation,
            Time.deltaTime * _owner.ModeChangeSpeed
        );
    }

    public void Exit() 
    { 
        _owner.Effect.SetReadyToCombineStateEffect(false);
    }
}

public class DetectCombineTargetState : IObjectState
{
    private readonly SpawnObject _owner;

    public DetectCombineTargetState(SpawnObject owner) => _owner = owner;

    public void Enter()
    {
        _owner.transform.localRotation = _owner._ActiveModeRotation;
        _owner.transform.localPosition = Vector3.zero;
    }

    public void UpdateState()
    {
        _owner.transform.localRotation *= Quaternion.Euler(
            0f, 0f, _owner.DetectSpinSpeedDegPerSec * Time.deltaTime
            );
    }

    public void Exit() { }
}

public class CombiningState : IObjectState
{
    private readonly SpawnObject _owner;
    private Coroutine _Routine;

    public CombiningState(SpawnObject owner) => _owner = owner;

    public void Enter()
    {
        _Routine = _owner.RunCoroutine(WaitAndComplete(_owner.CombineDuration));
    }

    public void UpdateState() { }

    public void Exit()
    {
        _owner.StopOwnerCoroutine(_Routine);
        _Routine = null;
    }

    private IEnumerator WaitAndComplete(float sec)
    {
        yield return new WaitForSeconds(sec);
        _owner.EnterCombinedCompleteState(); // 합체 완료 상태로 전환
    }
}

public class CombinedState : IObjectState
{
    private readonly SpawnObject _owner;

    public CombinedState(SpawnObject owner) => _owner = owner;

    public void Enter()
    {
        if (CombineManager.Instance != null)
        CombineManager.Instance.NotifyCombineCompleted();
    }
    public void UpdateState() { }
    public void Exit() { }

}