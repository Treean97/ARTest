using UnityEngine;

public class UIBlockGuard : MonoBehaviour
{
    private bool _Pushed;

    void OnEnable()
    {
        if (_Pushed) return;
        _Pushed = true;
        UIBlockCounter.Push();
    }

    void OnDisable()
    {
        if (!_Pushed) return;
        _Pushed = false;
        UIBlockCounter.Pop();
    }
}
