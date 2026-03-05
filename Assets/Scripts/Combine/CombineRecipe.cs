using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "ScriptableObject/Combine/Recipe")]
public class CombineRecipe : ScriptableObject
{
    [Header("조합 재료")]
    [SerializeField] private SpawnObjectData _A;
    [SerializeField] private SpawnObjectData _B;

    [Header("결과(소환 대상)")]
    [SerializeField] private List<SpawnObjectData> _Results = new();

    public SpawnObjectData A => _A;
    public SpawnObjectData B => _B;
    public IReadOnlyList<SpawnObjectData> Results => _Results;
}