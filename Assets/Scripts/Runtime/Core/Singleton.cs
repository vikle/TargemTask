using UnityEngine;

[DisallowMultipleComponent]
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    static T s_Instance;

    public static T Get
    {
        get
        {
            if( s_Instance != null ) return s_Instance;
            return (s_Instance = FindObjectOfType<T>());
        }
    }
};
