using UnityEngine;

public class SpawnObjectEffect : MonoBehaviour
{
    [Header("이펙트 상태")]
    [SerializeField] GameObject _EffectState_Effect_;
    [Header("합체 대기 상태")]
    [SerializeField] GameObject _ReadyToCombineState_Effect_Sparkle;
    [SerializeField] GameObject _ReadyToCombineState_Effect_Ghost;

    public void SetEffectStateEffect(bool isActive)
    {
        if(_EffectState_Effect_ == null) return;

        _EffectState_Effect_.SetActive(isActive);
    }

    public void SetReadyToCombineStateEffect(bool isActive)
    {
        if(_ReadyToCombineState_Effect_Sparkle == null) return;

        _ReadyToCombineState_Effect_Sparkle.SetActive(isActive);        
        _ReadyToCombineState_Effect_Ghost.SetActive(isActive);
    }

}
