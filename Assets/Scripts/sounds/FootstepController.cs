using UnityEngine;
using UnityEngine.InputSystem; // For XR input system

public class FootstepController : MonoBehaviour
{
    public float stepDelay = 0.5f; // Time between steps (adjust as needed)

    private float stepTimer;
    private Vector2 moveInput;

    [SerializeField]
    InputActionReference m_Move;

    void Update()
    {
        moveInput = m_Move.action.ReadValue<Vector2>();
        // Check if player is moving
        if (moveInput.magnitude > 0.1f)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.footstep, 0.7f);
                stepTimer = stepDelay;
            }
        }
        else
        {
            stepTimer = 0f; // reset when not moving
        }
    }
}
