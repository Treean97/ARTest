using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    [Header("데이터")]
    [SerializeField] SpawnObjectData _Data;
    public SpawnObjectData Data => _Data;
    
    [Header("설정")]
    [SerializeField] float _RotationSpeed = 5f;
    public float RotationSpeed { get => _RotationSpeed; }
    [SerializeField] float _RotationRestoreSpeed = 10f;
    public float RotationRestoreSpeed { get => _RotationRestoreSpeed; }
    [SerializeField] float _ModeChangeSpeed = 15f;
    public float ModeChangeSpeed { get => _ModeChangeSpeed; }
    [SerializeField] float _SwipeSensitivity = 0.1f;
    public float SwipeSensitivity { get => _SwipeSensitivity; }
    [SerializeField] float _EffectDuration = 2f;
    public float EffectDuration { get => _EffectDuration; }
    [SerializeField] float _DetectSpinSpeedDegPerSec = 180f;
    public float DetectSpinSpeedDegPerSec { get => _DetectSpinSpeedDegPerSec; }
    [SerializeField] float _CombineDuration = 3f;
    public float CombineDuration => _CombineDuration;

    [Header("이펙트")]
    [SerializeField] private SpawnObjectEffect _Effect;
    public SpawnObjectEffect Effect => _Effect;

    [Header("미리보기")]
    [SerializeField] private CombinePartnerPreview _CombinePartnerPreview;

    private IObjectState _CurrentState;

    private DefaultState _DefaultState;
    private EffectState _EffectState;
    private ReadyToCombineState _ReadyToCombineState;
    private DetectCombineTargetState _DetectCombineTargetState;
    private CombiningState _CombiningState;
    private CombinedState _CombinedState;

    public float _SwipeInput { get; set; }
    public bool _IsPressing { get; set; }

    public Quaternion _DefaultModeRotation { get; private set; }
    public Quaternion _ActiveModeRotation { get; private set; }

    private bool _IsAlreadySetup;

    void Awake()
    {
        SetUp();
    }
        
    private void SetUp()
    {
        if (_IsAlreadySetup) return;

        _DefaultModeRotation = transform.localRotation;
        _ActiveModeRotation  = _DefaultModeRotation * Quaternion.Euler(90f, 0f, 0f);

        _DefaultState = new DefaultState(this);
        _EffectState  = new EffectState(this);
        _ReadyToCombineState = new ReadyToCombineState(this);
        _DetectCombineTargetState = new DetectCombineTargetState(this);
        _CombiningState = new CombiningState(this);
        _CombinedState = new CombinedState(this);

        SetState(_DefaultState);
        _IsAlreadySetup = true;
    }

    public void OnTrackingFound()
    {
        gameObject.SetActive(true);

        SetUp();

        EnterDefaultState();
        transform.localRotation = _DefaultModeRotation;
    }

    public void OnTrackingLost()
    {
        if (_CombinePartnerPreview != null) _CombinePartnerPreview.Hide();
    
        gameObject.SetActive(false);
    }

    void Update()
    {
        _CurrentState?.UpdateState();
    }

    public void SetState(IObjectState newState)
    {
        if (newState == null || _CurrentState == newState) return;

        _CurrentState?.Exit();
        _CurrentState = newState;
        _CurrentState.Enter();
    }

    public void EnterDefaultState() => SetState(_DefaultState);
    public void EnterEffectState() => SetState(_EffectState);
    public void EnterReadyToCombineState() => SetState(_ReadyToCombineState);
    public void EnterDetectCombineTargetState() => SetState(_DetectCombineTargetState);
    public void EnterCombiningState() => SetState(_CombiningState);
    public void EnterCombinedCompleteState() => SetState(_CombinedState);

    public void DispatchPressChanged(bool isPressing)
    {
        _IsPressing = isPressing;

        if (_CurrentState is IPressHandler handler)
            handler.OnPressChanged(isPressing);
    }

    public void DispatchSwipeDelta(float deltaX, float deltaY)
    {
        if (_CurrentState is ISwipeHandler handler)
            handler.OnSwipeDelta(deltaX, deltaY);
    }

    public void DispatchTap()
    {
        if (_CurrentState is ITapHandler handler)
            handler.OnTap();
    }

    public void DispatchDoubleTap()
    {
        if (_CurrentState is IDoubleTapHandler handler)
            handler.OnDoubleTap();
    }

    public Coroutine RunCoroutine(IEnumerator routine) => StartCoroutine(routine);

    public void StopOwnerCoroutine(Coroutine routine)
    {
        if (routine != null) StopCoroutine(routine);
    }

    public void ShowCombinePreview(IReadOnlyList<Texture2D> textures, float widthMeters, float heightMeters, CombinePartnerPreview.PreviewSide side)
    {
        if (_CombinePartnerPreview == null) return;
        _CombinePartnerPreview.ShowPartners(textures, widthMeters, heightMeters, side);
    }

    public void HideCombinePreview()
    {
        if (_CombinePartnerPreview == null) return;
        _CombinePartnerPreview.Hide();
    }    
}