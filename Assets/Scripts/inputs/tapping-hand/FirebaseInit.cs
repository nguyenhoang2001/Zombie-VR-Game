using Firebase;
using UnityEngine;
using System.Threading.Tasks;

public class FirebaseInit : MonoBehaviour
{
    public static bool IsReady { get; private set; }
    public static Task InitTask { get; private set; }

    private async void Awake()
    {
        DontDestroyOnLoad(gameObject);   // keep this object across scenes
        if (IsReady) return;

        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status != DependencyStatus.Available)
        {
            Debug.LogError("Firebase dependencies not available: " + status);
            return;
        }

        // If you have other Firebase modules to warm up, do it here.
        IsReady = true;
    }
}
