using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "ScriptableObject/Combine/Recipe")]
public class CombineRecipe : ScriptableObject
{
    [Header("조합 재료")]
    [SerializeField] private SpawnObjectData _A;
    [SerializeField] private SpawnObjectData _B;

    public SpawnObjectData A => _A;
    public SpawnObjectData B => _B;
}