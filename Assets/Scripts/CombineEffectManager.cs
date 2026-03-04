using UnityEngine;
using DigitalRuby.LightningBolt;

public class CombineEffectManager : MonoBehaviour
{
    [SerializeField] private LightningBoltScript _DetectCombineTargetState_Effect_Lightning;
    [SerializeField] private GameObject _CombiningState_Effect_Sparkle;
    [SerializeField] private GameObject _CombinedState_Effect_Sparkle;

    private bool _IsDetectActive;
    private bool _IsCombiningActive;
    private bool _IsCombinedActive;

    void Update()
    {
        if (!_IsDetectActive) return;

        CombineManager mgr = CombineManager.Instance;
        if (mgr == null || mgr.PrimaryTarget == null || mgr.SecondaryTarget == null)
        {
            SetDetectActive(false);
        }
    }

    // CombinePhase를 그대로 받아 이펙트를 적용
    public void ApplyPhase(CombineManager.CombinePhase phase)
    {
        ClearAll();

        switch (phase)
        {
            case CombineManager.CombinePhase.Detecting:
                SetDetectActive(true);
                break;

            case CombineManager.CombinePhase.Combining:
                SetCombiningActive(true);
                break;

            case CombineManager.CombinePhase.Combined:
                SetCombinedActive(true);
                break;

            case CombineManager.CombinePhase.Idle:
            default:
                break;
        }
    }

    public void SetDetectActive(bool isActive)
    {
        _IsDetectActive = isActive;

        if (_DetectCombineTargetState_Effect_Lightning != null &&
            _DetectCombineTargetState_Effect_Lightning.gameObject.activeSelf != isActive)
        {
            _DetectCombineTargetState_Effect_Lightning.gameObject.SetActive(isActive);
        }

        if (!isActive) return;
        
        if (_DetectCombineTargetState_Effect_Lightning == null) return;

        CombineManager mgr = CombineManager.Instance;
        if (mgr == null) { SetDetectActive(false); return; }

        TrackedTarget primary = mgr.PrimaryTarget;
        TrackedTarget secondary = mgr.SecondaryTarget;

        if (primary == null || secondary == null)
        {
            // 타겟이 없으면 Detect를 켤 이유가 없음
            SetDetectActive(false);
            return;
        }

        // TrackedTarget의 Transform을 따라가려면 LightningBoltScript는 GameObject를 받아야 함
        _DetectCombineTargetState_Effect_Lightning.StartObject = primary.gameObject;
        _DetectCombineTargetState_Effect_Lightning.EndObject = secondary.gameObject;
    }

    public void SetCombiningActive(bool isActive)
    {
        _IsCombiningActive = isActive;

        if (_CombiningState_Effect_Sparkle != null &&
            _CombiningState_Effect_Sparkle.activeSelf != isActive)
        {
            _CombiningState_Effect_Sparkle.SetActive(isActive);
        }
    }

    public void SetCombinedActive(bool isActive)
    {
        _IsCombinedActive = isActive;

        if (_CombinedState_Effect_Sparkle != null &&
            _CombinedState_Effect_Sparkle.activeSelf != isActive)
        {
            _CombinedState_Effect_Sparkle.SetActive(isActive);
        }
    }

    public void ClearAll()
    {
        SetDetectActive(false);
        SetCombiningActive(false);
        SetCombinedActive(false);
    }
}