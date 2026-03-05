using System.Collections.Generic;
using UnityEngine;

public class CombinePartnerPreview : MonoBehaviour
{
    [Header("Quad")]
    [SerializeField] private Transform _Quad;
    [SerializeField] private MeshRenderer _QuadRenderer;

    [Header("Layout")]
    [SerializeField] private float _MarginMeters = 0.01f; // 카드 옆 간격
    [SerializeField] private float _SwapInterval = 1f; // 전환 주기

    public enum PreviewSide { Left, Right }

    private readonly List<Texture2D> _Textures = new();
    private int _Index;
    private float _Timer;
    private bool _IsActive;

    private float _TargetWidth;
    private float _TargetHeight;
    private PreviewSide _Side;

    void Awake()
    {
        if (_QuadRenderer != null)
        {
            var src = _QuadRenderer.sharedMaterial;
            if (src != null) _QuadRenderer.sharedMaterial = new Material(src);
        }

        SetVisible(false);
    }

    void Update()
    {
        if (!_IsActive) return;
        if (_Textures.Count == 0) return;

        _Timer += Time.deltaTime;
        if (_Timer < _SwapInterval) return;

        _Timer = 0f;

        // 무한 순회
        _Index = (_Index + 1) % _Textures.Count;
        ApplyTexture(_Textures[_Index]);
    }

    public void ShowPartners(IReadOnlyList<Texture2D> partnerTextures, float targetWidthMeters, float targetHeightMeters, PreviewSide side)
    {
        Debug.Log($"[Preview] ShowPartners called. QuadRoot={_Quad?.name}, Renderer={_QuadRenderer?.name}, count={(partnerTextures==null? -1 : partnerTextures.Count)}");
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

        _TargetWidth = Mathf.Max(0.0001f, targetWidthMeters);
        _TargetHeight = Mathf.Max(0.0001f, targetHeightMeters);
        _Side = side;

        _Index = 0;
        _Timer = 0f;

        ApplyLayout();
        ApplyTexture(_Textures[_Index]);

        _IsActive = true;
        SetVisible(true);
    }

    public void Hide()
    {
        _IsActive = false;
        _Textures.Clear();
        _Index = 0;
        _Timer = 0f;
        SetVisible(false);
    }

    private void ApplyLayout()
    {
        if (_Quad == null) return;

        // 카드와 항상 같은 크기(월드 크기 기준)
        _Quad.localScale = new Vector3(_TargetWidth, _TargetHeight, 1f);

        // 카드 옆에 "딱 붙게": (카드 반폭 + 프리뷰 반폭 + 마진)
        float sign = (_Side == PreviewSide.Left) ? -1f : 1f;
        float offsetX = _TargetWidth + _MarginMeters;

        _Quad.localPosition = new Vector3(sign * offsetX, 0f, 0f);
        _Quad.localRotation = Quaternion.identity;
    }

    private void ApplyTexture(Texture2D tex)
    {
        if (_QuadRenderer == null) return;
        Material m = _QuadRenderer.sharedMaterial;
        if (m == null) return;

        m.mainTexture = tex; // _MainTex 기반 머티리얼 전제
    }

    private void SetVisible(bool visible)
    {
        if (_Quad != null) _Quad.gameObject.SetActive(visible);
    }
}
