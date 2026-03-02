using UnityEngine;

public class TrackedTarget : MonoBehaviour
{
    [SerializeField] private SpawnObject _SpawnObject;

    public void OnFound()
    {
        if (_SpawnObject != null)
            _SpawnObject.OnTrackingFound();
    }

    public void OnLost()
    {
        if (_SpawnObject != null)
            _SpawnObject.OnTrackingLost();
    }

    public SpawnObject GetSpawnObject() => _SpawnObject;
}