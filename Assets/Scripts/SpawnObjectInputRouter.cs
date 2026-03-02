using UnityEngine;
using UnityEngine.EventSystems;

public class SpawnObjectInputRouter : MonoBehaviour
{
    public SpawnObject _Target;

    public float _TapThreshold = 20f;
    public float _DoubleTapInterval = 0.3f;

    private Camera _Cam;

    private Vector2 _PressStartPos;
    private Vector2 _PrevPos;
    private bool _IsSwiping;
    private float _LastTapTime;

    private bool _PressedOnTarget;

    void Awake()
    {
        if (_Target == null) _Target = GetComponent<SpawnObject>();
        _Cam = Camera.main;
    }

    void Update()
    {
        if (_Target == null || _Cam == null) return;

#if UNITY_EDITOR
        HandleMouse();
#else
        if (Input.touchCount != 1) return;
        HandleTouch(Input.GetTouch(0));
#endif
    }

#if UNITY_EDITOR
    private void HandleMouse()
    {
        // UI 위 클릭이면 입력 무시
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Input.mousePosition;

            _PressStartPos = pos;
            _PrevPos = pos;
            _IsSwiping = false;

            _PressedOnTarget = IsClickedTarget(pos);

            _Target.DispatchPressChanged(true);
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 pos = Input.mousePosition;

            if (!_IsSwiping && Vector2.Distance(_PressStartPos, pos) > _TapThreshold)
                _IsSwiping = true;

            if (_IsSwiping)
            {
                float deltaX = pos.x - _PrevPos.x;
                float deltaY = pos.y - _PrevPos.y;
                _Target.DispatchSwipeDelta(deltaX, deltaY);
            }

            _PrevPos = pos;
        }

        if (Input.GetMouseButtonUp(0))
        {
            _Target.DispatchPressChanged(false);

            if (!_IsSwiping && _PressedOnTarget)
                ProcessTap();
        }
    }
#endif

    private void HandleTouch(Touch touch)
    {
        // UI 위 터치면 입력 무시
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            return;

        Vector2 pos = touch.position;

        if (touch.phase == TouchPhase.Began)
        {
            _PressStartPos = pos;
            _PrevPos = pos;
            _IsSwiping = false;

            _PressedOnTarget = IsClickedTarget(pos);

            _Target.DispatchPressChanged(true);
        }
        else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
        {
            if (!_IsSwiping && Vector2.Distance(_PressStartPos, pos) > _TapThreshold)
                _IsSwiping = true;

            if (_IsSwiping)
            {
                float deltaX = pos.x - _PrevPos.x;
                float deltaY = pos.y - _PrevPos.y;
                _Target.DispatchSwipeDelta(deltaX, deltaY);
            }

            _PrevPos = pos;
        }
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            _Target.DispatchPressChanged(false);

            if (!_IsSwiping && _PressedOnTarget)
                ProcessTap();
        }
    }

    private void ProcessTap()
    {
        float now = Time.time;
        float interval = now - _LastTapTime;
        _LastTapTime = now;

        if (interval <= _DoubleTapInterval) _Target.DispatchDoubleTap();
        else _Target.DispatchTap();
    }

    private bool IsClickedTarget(Vector2 screenPos)
    {
        Ray ray = _Cam.ScreenPointToRay(screenPos);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return false;

        Transform t = hit.transform;
        return (t == _Target.transform) || t.IsChildOf(_Target.transform);
    }
}