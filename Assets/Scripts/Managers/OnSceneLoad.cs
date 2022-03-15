using UnityEngine;
using UnityEngine.Events;

public class OnSceneLoad : MonoBehaviour
{
    public UnityEvent onLoad;
    
    void Start()
    {
        onLoad.Invoke();
    }
}
