using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinePartnerPreview : MonoBehaviour
{
    [Header("Quad")]
    [SerializeField] private Transform _QuadObj;
    [SerializeField] private MeshRenderer _QuadRenderer;

    [Header("Layout")]
    [SerializeField] private float _MarginMeters = 0.01f;

    [Header("Swap Timing")]
    [SerializeField] private float _HoldSec = 1.0f;      // 한 장 보여주는 시간(페이드 제외)

    [Header("Fade")]
    [SerializeField] private float _FadeInSec = 0.15f;
    [SerializeField] private float _FadeOutSec = 0.15f;

    public enum PreviewSide { Left, Right }

    private readonly List<Texture2D> _Textures = new();
    private int _Index;
    private bool _IsActive;

    private float _TargetWidth;
    private float _TargetHeight;
    private PreviewSide _Side;

    private Material _Mat;
    private Coroutine _LoopRoutine;

    void Awake()
    {
        if (_QuadRenderer != null)
        {
            var src = _QuadRenderer.sharedMaterial;
            if (src != null)
            {
                _Mat = new Material(src);
                _QuadRenderer.sharedMaterial = _Mat;
            }
        }

        // 시작은 꺼둠
        SetAlpha(0f);
        SetVisible(false);
    }


    public void ShowPartners(IReadOnlyList<Texture2D> partnerTextures, float targetWidthMeters, float targetHeightMeters, PreviewSide side)
    {
        _Textures.Clear();
        if (partnerTextures != null)
        {
            for (int i = 0; i < partnerTextures.Count; i++)
            {
                Texture2D t = partnerTextures[i];
                if (t == null) continue;
                _Textures.Add(t);
            }
        }

        if (_Textures.Count == 0)
        {
            Hide();
            Debug.Log("Textures 비었음");
            return;
        }

        _TargetWidth = targetWidthMeters;
        _TargetHeight = targetHeightMeters;
        _Side = side;

        ApplyLayout();

        _Index = 0;
        _IsActive = true;

        SetVisible(true);
        ApplyTexture(_Textures[_Index]);
        SetAlpha(0f); // 첫 등장도 페이드 인으로

        RestartLoop();
    }

    /// <summary>
    /// 표시 중지 (즉시 숨김)
    /// </summary>
    public void Hide()
    {
        _IsActive = false;

        if (_LoopRoutine != null)
        {
            StopCoroutine(_LoopRoutine);
            _LoopRoutine = null;
        }

        _Textures.Clear();
        _Index = 0;

        SetAlpha(0f);
        SetVisible(false);
    }

    private void RestartLoop()
    {
        if (_LoopRoutine != null)
        {
            StopCoroutine(_LoopRoutine);
            _LoopRoutine = null;
        }
        _LoopRoutine = StartCoroutine(CoLoop());
    }

    private IEnumerator CoLoop()
    {
        // 첫 텍스처 FadeIn
        yield return CoFade(1f, _FadeInSec);

        while (_IsActive && _Textures.Count > 0)
        {
            // 한 장 유지
            if (_HoldSec > 0f)
                yield return new WaitForSeconds(_HoldSec);

            // 다음 텍스처가 없으면 그대로 반복
            int nextIndex = (_Index + 1) % _Textures.Count;

            // FadeOut
            yield return CoFade(0f, _FadeOutSec);

            if (!_IsActive) yield break;

            // Swap
            _Index = nextIndex;
            ApplyTexture(_Textures[_Index]);

            // FadeIn
            yield return CoFade(1f, _FadeInSec);
        }
    }

    private void ApplyLayout()
    {
        if (_QuadObj == null) return;

        _QuadObj.localScale = new Vector3(_TargetWidth, _TargetHeight, 1f);

        float sign = (_Side == PreviewSide.Left) ? -1f : 1f;
        float offsetX = _TargetWidth + _MarginMeters;

        _QuadObj.localPosition = new Vector3(sign * offsetX, 0f, 0f);
        _QuadObj.localRotation = Quaternion.identity;
    }

    private void ApplyTexture(Texture2D tex)
    {
        if (_Mat == null) return;
        _Mat.mainTexture = tex;
    }

    private void SetVisible(bool visible)
    {
        if (_QuadObj != null) _QuadObj.gameObject.SetActive(visible);
    }

    private IEnumerator CoFade(float targetAlpha, float duration)
    {
        if (duration <= 0f)
        {
            SetAlpha(targetAlpha);
            yield break;
        }

        float startAlpha = _Mat.color.a;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float a)
    {
        if (_Mat == null) return;
        
        Color c = _Mat.color;
        c.a = Mathf.Clamp01(a);
        _Mat.color = c;

        Debug.Log($"alpha={c.a}");
    }
}