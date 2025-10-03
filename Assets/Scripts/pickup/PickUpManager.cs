using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PickUpManager : MonoBehaviour
{
    public static PickUpManager Instance { get; private set; }

    private readonly HashSet<PickUpObject> registered = new HashSet<PickUpObject>();

    [SerializeField]
    bool discoverOnStart = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (!discoverOnStart)
            return;
        foreach (var p in FindObjectsOfType<PickUpObject>(includeInactive: true))
            Register(p);
    }

    public void Register(PickUpObject obj)
    {
        if (obj)
            registered.Add(obj);
    }

    public void Unregister(PickUpObject obj)
    {
        if (obj)
            registered.Remove(obj);
    }

    /// True if any registered pickup that blocks swap was held at least once this cycle.
    public bool AnyHeldThisCycle =>
        registered.Any(o => o && o.BlocksSwap && o.WasHeldSinceLastCycle);

    /// Activate the most recently held pickup in this cycle (if any). Returns true if one was activated.
    public void ActivateMostRecentHeld()
    {
        var target = registered
            .Where(o => o && o.WasHeldSinceLastCycle)
            .OrderByDescending(o => o.LastHeldTime)
            .FirstOrDefault();

        if (!target)
            return;

        target.Activate();
    }

    /// Activate all pickups that were held this cycle (rare, but sometimes useful).
    public void ActivateAllHeld()
    {
        foreach (var o in registered.Where(o => o && o.WasHeldSinceLastCycle))
        {
            o.Activate();
        }
    }

    /// Call this after your ML/tap decision completes (wrist/elbow/no-tap).
    public void ResetCycle()
    {
        foreach (var o in registered)
            if (o)
                o.ResetCycleFlag();
    }
}
