using System.Collections.Generic;
using UnityEngine;

public class CombineManager : MonoBehaviour
{
    public static CombineManager Instance { get; private set; }

    [SerializeField] private ImageTracker _ImageTracker;

    [Header("합체 조합식")]
    [SerializeField] List<CombineRecipe> _CombineRecipes = new();
    private readonly HashSet<PairKey> _AllowedPairs = new();
    private readonly Dictionary<int, List<SpawnObjectData>> _CombinePartners = new();

    [Header("합체 앵커")]
    [SerializeField] private CombineAnchor _CombineAnchor;

    [Header("합체 가능 거리")]
    [SerializeField] private float _JudgeDeltaX = 1.2f;
    [SerializeField] private float _JudgeDeltaY = 1f;

    [Header("이펙트")]
    [SerializeField] private CombineEffect _CombineEffect;

    private TrackedTarget _PrimaryTarget;
    public TrackedTarget PrimaryTarget => _PrimaryTarget;

    private TrackedTarget _SecondaryTarget;
    public TrackedTarget SecondaryTarget => _SecondaryTarget;

    private Camera _Cam;
    public Vector3 CombinePosition { get; private set; }

    private readonly Dictionary<SpawnObject, Transform> _OriginalParents = new();

    public enum CombinePhase
    {
        Idle,
        Detecting,
        Combining,
        Combined,
    }

    [SerializeField] private CombinePhase _Phase = CombinePhase.Idle;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _Cam = Camera.main;

        // 레시피 캐싱
        CacheRecipes();
    }

    void OnEnable()
    {
        if (_ImageTracker == null) return;

        // ImageTracker 이벤트 구독
        _ImageTracker.OnCombineSecondaryFound += HandleCombineSecondaryFound;
        _ImageTracker.OnCombinePrimaryLost += HandleCombinePrimaryLost;
        _ImageTracker.OnCombineSecondaryLost += HandleCombineSecondaryLost;

        // 객체 상태 이벤트 등록
        // (삭제) 이펙트는 CombinePhase 전환 지점에서만 제어
    }

    void OnDisable()
    {
        if (_ImageTracker == null) return;

        // 구독 해제
        _ImageTracker.OnCombineSecondaryFound -= HandleCombineSecondaryFound;
        _ImageTracker.OnCombinePrimaryLost -= HandleCombinePrimaryLost;
        _ImageTracker.OnCombineSecondaryLost -= HandleCombineSecondaryLost;

        // 비활성화 시 이펙트 정리
        ApplyEffectPhase(CombinePhase.Idle);
    }

    private void HandleCombineSecondaryFound(TrackedTarget primary, TrackedTarget secondary)
    {
        // ImageTracker가 알려준 이벤트를 받아, CombineManager가 스스로 처리
        NotifySecondaryTargetFound(primary, secondary);
    }

    private void HandleCombinePrimaryLost(TrackedTarget lostPrimary)
    {
        NotifyPrimaryTargetLost(lostPrimary);
    }

    private void HandleCombineSecondaryLost(TrackedTarget lostSecondary)
    {
        NotifySecondaryTargetLost(lostSecondary);
    }

    void Update()
    {
        // Detecting/Combining/Combined 에서만 처리
        if (_Phase == CombinePhase.Idle) return;
        if (_PrimaryTarget == null || _SecondaryTarget == null) return;

        Vector3 p1 = _PrimaryTarget.transform.position;
        Vector3 p2 = _SecondaryTarget.transform.position;

        CombinePosition = (p1 + p2) * 0.5f;

        // 합체 중/완료 단계면 앵커 위치 계속 갱신
        if ((_Phase == CombinePhase.Combining || _Phase == CombinePhase.Combined) && _CombineAnchor != null)
        {
            _CombineAnchor.transform.position = CombinePosition;
        }

        // 월드 좌표 기준 dx/dy
        float dx = Mathf.Abs(p1.x - p2.x);
        float dy = Mathf.Abs(p1.y - p2.y);

        switch (_Phase)
        {
            case CombinePhase.Detecting:
                // 가까워지면 합체 시작
                if (dx <= _JudgeDeltaX && dy <= _JudgeDeltaY)
                {
                    StartCombine();
                }
                break;

            case CombinePhase.Combining:
                // 멀어지면 합체 해제
                if (dx > _JudgeDeltaX || dy > _JudgeDeltaY)
                {
                    BreakCombineToDetecting();
                }
                break;

            case CombinePhase.Combined:
                // 멀어지면 합체 해제
                if (dx > _JudgeDeltaX || dy > _JudgeDeltaY)
                {
                    BreakCombineToDetecting();
                    break;
                }

                // TrySummon을 매 프레임 호출
                TrySummon();
                break;
        }
    }

    public void SetPrimaryTarget(TrackedTarget target) => _PrimaryTarget = target;
    public void SetSecondaryTarget(TrackedTarget target) => _SecondaryTarget = target;

    public void RequestCombine(SpawnObject requester)
    {
        if (_ImageTracker == null) return;

        TrackedTarget current = _ImageTracker.CurrentTarget;
        if (current == null) return;

        SpawnObject currentSpawn = current.GetSpawnObject();
        if (currentSpawn == null) return;

        if (currentSpawn != requester) return;

        _PrimaryTarget = current;
        _SecondaryTarget = null;

        _Phase = CombinePhase.Idle;

        // 합체 플로우를 새로 시작하니 이펙트는 싹 정리
        ApplyEffectPhase(CombinePhase.Idle);

        _ImageTracker.EnterCombineSearching(_PrimaryTarget);
        SetSecondaryTarget(null);
    }

    private void StartCombine()
    {
        if (!TryGetSpawnObjects(out SpawnObject primaryObj, out SpawnObject secondaryObj)) return;
        if (_CombineAnchor == null || _CombineAnchor.LeftAnchor == null || _CombineAnchor.RightAnchor == null) return;

        // 합체 시작 Combining
        _Phase = CombinePhase.Combining;

        // Combining 이펙트 ON
        ApplyEffectPhase(CombinePhase.Combining);

        _CombineAnchor.transform.SetParent(_PrimaryTarget.transform, true);
        _CombineAnchor.transform.localRotation = Quaternion.identity;
        _CombineAnchor.transform.position = CombinePosition;

        float x1 = _Cam.WorldToScreenPoint(_PrimaryTarget.transform.position).x;
        float x2 = _Cam.WorldToScreenPoint(_SecondaryTarget.transform.position).x;

        SpawnObject leftObj = (x1 <= x2) ? primaryObj : secondaryObj;
        SpawnObject rightObj = (x1 <= x2) ? secondaryObj : primaryObj;

        CacheOriginalParent(leftObj);
        CacheOriginalParent(rightObj);

        PlaceUnderAnchor(leftObj, _CombineAnchor.LeftAnchor);
        PlaceUnderAnchor(rightObj, _CombineAnchor.RightAnchor);

        // 두 객체 모두 합체 중 상태로 전환
        primaryObj.EnterCombiningState();
        secondaryObj.EnterCombiningState();
    }

    // 보조 대상 찾음
    public void NotifySecondaryTargetFound(TrackedTarget primary, TrackedTarget secondary)
    {
        if (_ImageTracker == null) return;
        if (primary == null || secondary == null) return;
        if (_PrimaryTarget != primary) return;
        if (secondary == _PrimaryTarget) return;

        _SecondaryTarget = secondary;

        if (!TryGetSpawnObjects(out SpawnObject primaryObj, out SpawnObject secondaryObj)) return;

        if (!CheckRecipe(primaryObj.Data, secondaryObj.Data))
        {
            Debug.Log("조합 불가능");
            _SecondaryTarget = null;

            return;
        }
        else
        {
            Debug.Log("조합 가능");
        }

        // 두 객체 모두 합체 대상을 찾은 상태로 전환
        primaryObj.EnterDetectCombineTargetState();
        secondaryObj.EnterDetectCombineTargetState();

        _Phase = CombinePhase.Detecting;

        // Detecting 이펙트 ON (pair 포함)
        ApplyEffectPhase(CombinePhase.Detecting);

        _ImageTracker.EnterCombineDetecting(_PrimaryTarget, _SecondaryTarget);
    }

    // SpawnObject 쪽에서 합체 완료가 되면 호출
    public void NotifyCombineCompleted()
    {
        if (_Phase != CombinePhase.Combining) return;

        _Phase = CombinePhase.Combined;

        // Combined 이펙트 ON
        ApplyEffectPhase(CombinePhase.Combined);
    }

    // 거리 멀어지면 다시 해제
    private void BreakCombineToDetecting()
    {
        if (!TryGetSpawnObjects(out SpawnObject primaryObj, out SpawnObject secondaryObj)) return;

        _Phase = CombinePhase.Detecting;

        // Detecting로 복귀 (pair 포함)
        ApplyEffectPhase(CombinePhase.Detecting);

        RestoreCombineAnchor();

        RestoreToOriginalParent(primaryObj);
        RestoreToOriginalParent(secondaryObj);

        primaryObj.EnterDetectCombineTargetState();
        secondaryObj.EnterDetectCombineTargetState();
    }

    // 주대상 사라짐
    public void NotifyPrimaryTargetLost(TrackedTarget lost)
    {
        if (_ImageTracker == null) return;
        if (_PrimaryTarget == null) return;
        if (lost != _PrimaryTarget) return;

        // 합체/감지 종료
        _Phase = CombinePhase.Idle;

        // 타겟 잃으면 이펙트 싹 정리
        ApplyEffectPhase(CombinePhase.Idle);

        // 앵커 복귀
        RestoreCombineAnchor();

        // secondary 정리
        if (_SecondaryTarget != null)
        {
            SpawnObject secondaryObj = _SecondaryTarget.GetSpawnObject();
            if (secondaryObj != null)
            {
                RestoreToOriginalParent(secondaryObj);
                secondaryObj.EnterDefaultState();
            }
        }

        // primary 정리
        SpawnObject primaryObj = _PrimaryTarget.GetSpawnObject();
        if (primaryObj != null)
        {
            RestoreToOriginalParent(primaryObj);
            primaryObj.EnterDefaultState();
        }

        _PrimaryTarget = null;
        _SecondaryTarget = null;

        // 트래커는 싱글로
        _ImageTracker.EnterSingleTracking();
    }

    // 보조 대상 사라짐
    public void NotifySecondaryTargetLost(TrackedTarget lost)
    {
        if (_ImageTracker == null) return;
        if (_SecondaryTarget == null) return;
        if (lost != _SecondaryTarget) return;

        // secondary가 사라졌으니 감지는 종료, 다시 탐색으로
        _Phase = CombinePhase.Idle;

        // 타겟 잃으면 이펙트 싹 정리
        ApplyEffectPhase(CombinePhase.Idle);

        // 앵커 복귀
        RestoreCombineAnchor();

        SpawnObject secondaryObj = _SecondaryTarget.GetSpawnObject();
        if (secondaryObj != null)
        {
            RestoreToOriginalParent(secondaryObj);
            secondaryObj.EnterDefaultState();
        }

        _SecondaryTarget = null;

        if (_PrimaryTarget != null)
        {
            SpawnObject primaryObj = _PrimaryTarget.GetSpawnObject();
            if (primaryObj != null)
            {
                RestoreToOriginalParent(primaryObj);
                primaryObj.EnterReadyToCombineState();
            }

            _ImageTracker.EnterCombineSearching(_PrimaryTarget);
        }
        else
        {
            _ImageTracker.EnterSingleTracking();
        }
    }

    private void TrySummon()
    {
        // 고스트 소환
    }

    // 상태 별 이펙트 적용
    private void ApplyEffectPhase(CombinePhase phase)
    {
        if (_CombineEffect == null) return;
        _CombineEffect.ApplyPhase(phase);
    }

    // 소환 객체 추출
    private bool TryGetSpawnObjects(out SpawnObject primaryObj, out SpawnObject secondaryObj)
    {
        primaryObj = null;
        secondaryObj = null;

        if (_PrimaryTarget == null || _SecondaryTarget == null) return false;

        primaryObj = _PrimaryTarget.GetSpawnObject();
        secondaryObj = _SecondaryTarget.GetSpawnObject();

        return primaryObj != null && secondaryObj != null;
    }

    private void CacheOriginalParent(SpawnObject obj)
    {
        if (obj == null) return;
        if (_OriginalParents.ContainsKey(obj)) return;
        _OriginalParents.Add(obj, obj.transform.parent);
    }

    private void PlaceUnderAnchor(SpawnObject obj, Transform anchor)
    {
        obj.transform.SetParent(anchor, false);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }

    private void RestoreToOriginalParent(SpawnObject obj)
    {
        if (obj == null) return;
        if (!_OriginalParents.TryGetValue(obj, out Transform parent)) return;

        obj.transform.SetParent(parent, true);
        _OriginalParents.Remove(obj);
    }

    private void RestoreCombineAnchor()
    {
        if (_CombineAnchor == null) return;
        _CombineAnchor.transform.SetParent(transform, true);
    }

    private void CacheRecipes()
    {
        _AllowedPairs.Clear();
        _CombinePartners.Clear();

        if (_CombineRecipes == null) return;

        foreach (var recipe in _CombineRecipes)
        {
            if (recipe == null) continue;
            if (recipe.A == null || recipe.B == null) continue;

            int idA = recipe.A.ID;
            int idB = recipe.B.ID;

            var key = new PairKey(idA, idB);

            // 합체 조합 캐싱
            _AllowedPairs.Add(key);

            // 파트너 캐싱
            AddPartner(idA, recipe.B);
            AddPartner(idB, recipe.A);
        }
    }

    private void AddPartner(int keyId, SpawnObjectData partner)
    {
        if (partner == null) return;

        if (!_CombinePartners.TryGetValue(keyId, out List<SpawnObjectData> list))
        {
            list = new List<SpawnObjectData>();
            _CombinePartners.Add(keyId, list);
        }

        // 중복 방지 (레시피 중복 / (A,B)와 (B,A) 둘 다 등록 대비)
        if (!list.Contains(partner))
            list.Add(partner);
    }

    public IReadOnlyList<SpawnObjectData> GetCombinePartners(SpawnObjectData primary)
    {
        if (primary == null) return System.Array.Empty<SpawnObjectData>();

        return _CombinePartners.TryGetValue(primary.ID, out List<SpawnObjectData> list)
            ? list
            : System.Array.Empty<SpawnObjectData>();
    }

    public bool CheckRecipe(SpawnObjectData a, SpawnObjectData b)
    {
        if (a == null || b == null) return false;
        return _AllowedPairs.Contains(new PairKey(a.ID, b.ID));
    }
}