using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField]
    private float hitPoints = 100f;

    [Header("Death Collision Handling")]
    [Tooltip(
        "If ON → disables colliders; if OFF → converts them to triggers (still detectable but no physics contacts)."
    )]
    [SerializeField]
    private bool disableCollidersOnDeath = true;

    [Tooltip("Set GameObject static at death (editor flag).")]
    [SerializeField]
    private bool setStaticOnDeath = true;

    private bool isDead;

    // caches
    private Collider[] colliders;
    private Rigidbody[] rigidbodies;
    private Animator animator;
    private NavMeshAgent agent;
    private CharacterController characterController;
    private EnemyAI enemyAI; // your AI script
    private EnemyAttack enemyAttack; // your attack script

    private void Awake()
    {
        colliders = GetComponentsInChildren<Collider>(includeInactive: true);
        rigidbodies = GetComponentsInChildren<Rigidbody>(includeInactive: true);
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        characterController = GetComponent<CharacterController>();
        enemyAI = GetComponent<EnemyAI>();
        enemyAttack = GetComponent<EnemyAttack>();
    }

    public bool IsDead() => isDead;

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        BroadcastMessage("OnDamageTaken", SendMessageOptions.DontRequireReceiver);
        hitPoints -= damage;
        if (hitPoints <= 0f)
            Die();
    }

    private void Die()
    {
        if (isDead)
            return;
        isDead = true;

        // Animation
        if (animator)
            animator.SetTrigger("die");

        // Stop gameplay logic / locomotion
        if (enemyAI)
            enemyAI.enabled = false;
        if (enemyAttack)
            enemyAttack.enabled = false;
        if (agent)
            agent.enabled = false;
        if (characterController)
            characterController.enabled = false;

        // Kill physics interaction
        foreach (var rb in rigidbodies)
        {
            if (!rb)
                continue;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        foreach (var col in colliders)
        {
            if (!col)
                continue;
            if (disableCollidersOnDeath)
                col.enabled = false;
            else
                col.isTrigger = true; // still detectable by triggers (sensors/loot)
        }

        // Optional editor/static flag
        if (setStaticOnDeath)
            gameObject.isStatic = true;
    }

    private static void SetLayerRecursively(Transform root, int layer)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
    }
}
