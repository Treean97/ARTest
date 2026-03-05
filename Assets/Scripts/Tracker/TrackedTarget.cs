using easyar;
using UnityEngine;

public class TrackedTarget : MonoBehaviour
{
    [SerializeField] private SpawnObject _SpawnObject;

    private Vector2 _TargetSizeMeters = new Vector2(0.1f, 0.1f);
    public Vector2 TargetSizeMeters => _TargetSizeMeters;

    private ImageTargetController _Controller;

    public void OnFound(ImageTargetController controller)
    {
        if (_SpawnObject != null) _SpawnObject.OnTrackingFound();
        Bind(controller);
    }

    public void OnLost()
    {
        if (_SpawnObject != null) _SpawnObject.OnTrackingLost();
    }

    public SpawnObject GetSpawnObject() => _SpawnObject;

    private void Bind(ImageTargetController controller)
    {
        if (controller == null) return;

        // 기존 컨트롤러 구독 제거
        if (_Controller != null)
            _Controller.TargetDataLoad -= OnTargetDataLoad;

        // 새 컨트롤러로 덮기
        _Controller = controller;
        _Controller.TargetDataLoad += OnTargetDataLoad;

        // 이미 로드된 상태면 즉시 Size 반영
        if (_Controller.Target != null)
            _TargetSizeMeters = _Controller.Size;
    }

    private void OnTargetDataLoad(bool success)
    {
        if (!success) return;
        if (_Controller == null) return;
        _TargetSizeMeters = _Controller.Size;
    }
}