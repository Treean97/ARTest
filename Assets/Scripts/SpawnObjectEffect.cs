using UnityEngine;

public class SpawnObjectEffect : MonoBehaviour
{
    [SerializeField] GameObject _EffectState_EffectObj;
    [SerializeField] GameObject _ReadyToCombineState_EffectObj;

    public void SetEffectStateEffect(bool isActive)
    {
        if(_EffectState_EffectObj == null) return;

        _EffectState_EffectObj.SetActive(isActive);
    }

    public void SetReadyToCombineStateEffect(bool isActive)
    {
        if(_ReadyToCombineState_EffectObj == null) return;

        _ReadyToCombineState_EffectObj.SetActive(isActive);
    }
}
