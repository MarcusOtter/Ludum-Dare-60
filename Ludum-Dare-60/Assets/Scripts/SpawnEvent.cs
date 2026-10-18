using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "SpawnEvent", menuName = "Scriptable Objects/SpawnEvent")]
public class SpawnEvent : ScriptableObject
{
    [FormerlySerializedAs("Time")] public float time;
    public FromDirection fromDirection;
    public RecycleObject recycleObject;
    public int onFireBounces;
}

public enum FromDirection
{
    Left,
    Right
}
