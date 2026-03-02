using UnityEngine;
using DigitalRuby.LightningBolt;

public class CombineEffectManager : MonoBehaviour
{
    [SerializeField] private LightningBoltScript _Lightning;

    private Transform _Start;
    private Transform _End;
    private bool _IsActive;
    public bool IsActive => _IsActive;

    void Awake()
    {
        SetActive(false);
    }

    void Update()
    {
        if (!_IsActive) return;
        if (_Lightning == null) return;

        if (_Start == null || _End == null)
        {
            Clear();
            return;
        }

        // LightningBoltScript는 GameObject를 받음
        _Lightning.StartObject = _Start.gameObject;
        _Lightning.EndObject = _End.gameObject;
    }

    public void SetPair(Transform start, Transform end)
    {
        _Start = start;
        _End = end;
    }

    public void SetActive(bool isActive)
    {
        _IsActive = isActive;

        if (_Lightning != null && _Lightning.gameObject.activeSelf != isActive)
            _Lightning.gameObject.SetActive(isActive);
    }

    public void Clear()
    {
        _Start = null;
        _End = null;
        SetActive(false);
    }
}