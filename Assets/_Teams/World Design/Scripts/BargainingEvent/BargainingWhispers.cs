using UnityEngine;
using FMODUnity;

[CreateAssetMenu(fileName = "BargainingWhispers", menuName = "Bargaining/BargainingWhispers")]
public class BargainingWhispers : ScriptableObject
{
    public EventReference[] whisperLines;
}
