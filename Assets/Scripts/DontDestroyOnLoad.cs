using MarTools;
using UnityEngine;

public class DontDestroyOnLoad : Singleton<DontDestroyOnLoad>
{
    public override bool AddToDontDestroyOnLoad => true;
}
