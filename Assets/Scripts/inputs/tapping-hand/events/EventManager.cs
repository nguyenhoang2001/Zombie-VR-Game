using System;
using System.Collections.Generic;

public static class EventManager
{
    // --------------------------
    // String-named channels (new)
    // --------------------------
    // One list per channel; each handler is stored as Action<object?>.
    private static readonly Dictionary<string, Action<object?>> _channels = new(
        StringComparer.Ordinal
    );

    // Lets us unsubscribe using the original delegate.
    private static readonly Dictionary<Delegate, Action<object?>> _wrapperLookup = new();

    /// Pre-create channels (call once on boot).
    public static void Initialize(IEnumerable<string> eventNames)
    {
        if (eventNames == null)
            return;
        foreach (var name in eventNames)
            EnsureEvent(name);
    }

    /// Ensure a channel exists; no-op if already present.
    public static void EnsureEvent(string eventName)
    {
        if (string.IsNullOrWhiteSpace(eventName))
            return;
        if (!_channels.ContainsKey(eventName))
            _channels[eventName] = null;
    }

    /// Subscribe to a no-payload channel by name.
    public static void Subscribe(string eventName, Action handler)
    {
        if (handler == null)
            return;
        EnsureEvent(eventName);
        Action<object?> wrapper = _ => handler();

        _wrapperLookup[handler] = wrapper;
        _channels[eventName] += wrapper;
    }

    /// Subscribe with a strongly-typed payload.
    public static void Subscribe<T>(string eventName, Action<T> handler)
    {
        if (handler == null)
            return;
        EnsureEvent(eventName);
        Action<object?> wrapper = o =>
        {
            // Ignore if payload isn’t the expected type.
            if (o is T t)
                handler(t);
        };

        _wrapperLookup[handler] = wrapper;
        _channels[eventName] += wrapper;
    }

    /// Unsubscribe (no-payload) by original handler.
    public static void Unsubscribe(string eventName, Action handler)
    {
        if (handler == null)
            return;
        if (!_wrapperLookup.TryGetValue(handler, out var wrapper))
            return;
        if (_channels.TryGetValue(eventName, out var chain))
        {
            chain -= wrapper;
            _channels[eventName] = chain;
        }
        _wrapperLookup.Remove(handler);
    }

    /// Unsubscribe (typed payload) by original handler.
    public static void Unsubscribe<T>(string eventName, Action<T> handler)
    {
        if (handler == null)
            return;
        if (!_wrapperLookup.TryGetValue(handler, out var wrapper))
            return;
        if (_channels.TryGetValue(eventName, out var chain))
        {
            chain -= wrapper;
            _channels[eventName] = chain;
        }
        _wrapperLookup.Remove(handler);
    }

    /// Publish a no-payload event by name.
    public static void Publish(string eventName)
    {
        if (_channels.TryGetValue(eventName, out var chain))
            chain?.Invoke(null);
    }

    /// Publish with a payload.
    public static void Publish<T>(string eventName, T payload)
    {
        if (_channels.TryGetValue(eventName, out var chain))
            chain?.Invoke(payload);
    }

    /// Clear all handlers (both buses).
    public static void Clear()
    {
        _channels.Clear();
        _wrapperLookup.Clear();
    }
}
