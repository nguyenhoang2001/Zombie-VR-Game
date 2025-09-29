using UnityEngine;

public class EventManagerBootstrap : MonoBehaviour
{
    [Header("Predefined events (string-named)")]
    [SerializeField] private string[] predefinedEvents =
    {
        EventNames.BEGIN_TAP,
        EventNames.TAPP_RIGHT_WRIST,
        EventNames.TAPP_LEFT_WRIST,
        EventNames.TAPP_RIGHT_MID,
        EventNames.TAPP_LEFT_MID,
        EventNames.TAPP_RIGHT_ELBOW,
        EventNames.TAPP_LEFT_ELBOW,
        EventNames.NO_TAPP,
        EventNames.RIGHT_GESTURE,
        EventNames.LEFT_GESTURE,
        EventNames.UP_GESTURE,
        EventNames.DOWN_GESTURE,
        EventNames.GESTURE_ACTION_START,
        EventNames.NO_GESTURE
    };

    private void Awake()
    {
        // Pre-create channels so Subscribe/Publish are safe anywhere after this.
        EventManager.Initialize(predefinedEvents);

        // Optional: Persist across scenes
        DontDestroyOnLoad(gameObject);
    }
}
