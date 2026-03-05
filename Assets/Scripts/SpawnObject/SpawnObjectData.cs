using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName ="ScriptableObject/SpawnObjectData")]
public class SpawnObjectData : ScriptableObject
{
    [SerializeField] int _ID;
    public int ID => _ID;

    [Header("Preview")]
    [SerializeField] private Sprite _PreviewSprite;
    public Sprite PreviewSprite => _PreviewSprite;

}
