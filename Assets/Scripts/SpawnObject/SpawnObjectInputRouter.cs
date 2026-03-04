using UnityEngine;

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
    private bool _WasPressingTarget; // Press(true)를 보냈던 상태인지

    void Awake()
    {
        if (_Target == null) _Target = GetComponent<SpawnObject>();
        _Cam = Camera.main;
    }

    void Update()
    {
        if (_Target == null || _Cam == null) return;

        // UI가 켜져있으면 월드 입력 완전 차단 + 남은 상태 정리
        if (UIBlockCounter.IsBlocking)
        {
            if (_WasPressingTarget)
            {
                _Target.DispatchPressChanged(false);
                _WasPressingTarget = false;
            }

            _IsSwiping = false;
            _PressedOnTarget = false;
            return;
        }

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
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Input.mousePosition;

            _PressStartPos = pos;
            _PrevPos = pos;
            _IsSwiping = false;

            _PressedOnTarget = IsClickedTarget(pos);
            _WasPressingTarget = false;

            if (_PressedOnTarget)
            {
                _Target.DispatchPressChanged(true);
                _WasPressingTarget = true;
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (!_PressedOnTarget) return;

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
            if (!_PressedOnTarget) return;

            if (_WasPressingTarget)
            {
                _Target.DispatchPressChanged(false);
                _WasPressingTarget = false;
            }

            if (!_IsSwiping)
                ProcessTap();

            _PressedOnTarget = false;
            _IsSwiping = false;
        }
    }
#endif

    private void HandleTouch(Touch touch)
    {
        Vector2 pos = touch.position;

        if (touch.phase == TouchPhase.Began)
        {
            _PressStartPos = pos;
            _PrevPos = pos;
            _IsSwiping = false;

            _PressedOnTarget = IsClickedTarget(pos);
            _WasPressingTarget = false;

            if (_PressedOnTarget)
            {
                _Target.DispatchPressChanged(true);
                _WasPressingTarget = true;
            }
        }
        else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
        {
            if (!_PressedOnTarget) return;

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
            if (!_PressedOnTarget) return;

            if (_WasPressingTarget)
            {
                _Target.DispatchPressChanged(false);
                _WasPressingTarget = false;
            }

            if (!_IsSwiping)
                ProcessTap();

            _PressedOnTarget = false;
            _IsSwiping = false;
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