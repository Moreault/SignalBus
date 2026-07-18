namespace ToolBX.SignalBus;

/// <summary>
/// Emits signals that can be received by any listener.
/// </summary>
public interface ISignalBus
{
    /// <summary>
    /// Subscribes an action to execute once the identifier is triggered.
    /// </summary>
    void Subscribe(object identifier, Action<object?> callback);

    /// <summary>
    /// Subscribes a strongly-typed action to execute once the identifier is triggered.
    /// The callback throws <see cref="InvalidCastException"/> if the identifier is triggered with arguments that are not assignable to <typeparamref name="TArgs"/>.
    /// </summary>
    void Subscribe<TArgs>(object identifier, Action<TArgs?> callback);

    /// <summary>
    /// Subscribes an action to execute once the identifier is triggered and also instantly triggers if it was triggered at least once before subscribing.
    /// </summary>
    void SubscribeRetroactively(object identifier, Action<object?> callback);

    /// <summary>
    /// Subscribes a strongly-typed action to execute once the identifier is triggered and also instantly triggers if it was triggered at least once before subscribing.
    /// The callback throws <see cref="InvalidCastException"/> if the identifier was/is triggered with arguments that are not assignable to <typeparamref name="TArgs"/>.
    /// </summary>
    void SubscribeRetroactively<TArgs>(object identifier, Action<TArgs?> callback);

    /// <summary>
    /// Triggers a signal for all listeners without parameters.
    /// </summary>
    void Trigger(object identifier);

    /// <summary>
    /// Triggers a signal for all listeners.
    /// </summary>
    void Trigger<TArgs>(object identifier, TArgs? args);

    /// <summary>
    /// Clears all identifiers from the <see cref="SignalBus"/>.
    /// </summary>
    void Clear();

    /// <summary>
    /// Clears all callbacks for the specified identifier.
    /// </summary>
    void Clear(object identifier);

    /// <summary>
    /// Removes a callback action for the specified identifier.
    /// </summary>
    void Unsubscribe(object identifier, Action<object?> callback);

    /// <summary>
    /// Removes a strongly-typed callback action for the specified identifier.
    /// </summary>
    void Unsubscribe<TArgs>(object identifier, Action<TArgs?> callback);

    /// <summary>
    /// Returns whether there is anything listening to the specified identifier.
    /// </summary>
    bool IsSubscribed(object identifier);

    /// <summary>
    /// Returns whether a callback action is listening for the specified identifier.
    /// </summary>
    bool IsSubscribed(object identifier, Action<object?> callback);

    /// <summary>
    /// Returns whether a strongly-typed callback action is listening for the specified identifier.
    /// </summary>
    bool IsSubscribed<TArgs>(object identifier, Action<TArgs?> callback);
}

/// <inheritdoc cref="ISignalBus"/>
public sealed class SignalBus : ISignalBus
{
    /// <summary>
    /// Pairs the original delegate (used for identity when (un)subscribing) with the boxed invoker used at trigger time.
    /// </summary>
    private readonly struct Subscription(Delegate original, Action<object?> invoke)
    {
        public Delegate Original { get; } = original;
        public Action<object?> Invoke { get; } = invoke;
    }

    private readonly Dictionary<object, List<Subscription>> _subscriptions = [];
    private readonly Dictionary<object, object?> _triggeredSignals = [];
    private readonly Lock _lock = new();

    public void Subscribe(object identifier, Action<object?> callback)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        ArgumentNullException.ThrowIfNull(callback);
        lock (_lock)
            AddSubscription(identifier, callback, callback);
    }

    public void Subscribe<TArgs>(object identifier, Action<TArgs?> callback)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        ArgumentNullException.ThrowIfNull(callback);
        lock (_lock)
            AddSubscription(identifier, callback, o => callback((TArgs?)o));
    }

    public void SubscribeRetroactively(object identifier, Action<object?> callback)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        ArgumentNullException.ThrowIfNull(callback);

        bool replay;
        object? args;
        lock (_lock)
        {
            AddSubscription(identifier, callback, callback);
            replay = _triggeredSignals.TryGetValue(identifier, out args);
        }

        if (replay) callback.Invoke(args);
    }

    public void SubscribeRetroactively<TArgs>(object identifier, Action<TArgs?> callback)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        ArgumentNullException.ThrowIfNull(callback);

        bool replay;
        object? args;
        lock (_lock)
        {
            AddSubscription(identifier, callback, o => callback((TArgs?)o));
            replay = _triggeredSignals.TryGetValue(identifier, out args);
        }

        if (replay) callback.Invoke((TArgs?)args);
    }

    // Must be called while holding _lock.
    private void AddSubscription(object identifier, Delegate original, Action<object?> invoke)
    {
        if (!_subscriptions.TryGetValue(identifier, out var callbacks))
        {
            callbacks = [];
            _subscriptions[identifier] = callbacks;
        }

        callbacks.Add(new Subscription(original, invoke));
    }

    public void Trigger(object identifier) => Trigger<object>(identifier, null);

    public void Trigger<TArgs>(object identifier, TArgs? args)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        // Snapshot the subscribers under the lock, then invoke them outside it: this keeps arbitrary
        // user callbacks (which may re-enter the bus from this or another thread) from running while
        // the lock is held, and means a callback subscribed during the trigger only fires next time.
        Subscription[] snapshot;
        lock (_lock)
        {
            _triggeredSignals[identifier] = args;
            if (!_subscriptions.TryGetValue(identifier, out var callbacks) || callbacks.Count == 0) return;
            snapshot = [.. callbacks];
        }

        foreach (var sub in snapshot)
            sub.Invoke(args);
    }

    public void Clear()
    {
        lock (_lock)
        {
            _subscriptions.Clear();
            _triggeredSignals.Clear();
        }
    }

    public void Clear(object identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        lock (_lock)
        {
            _subscriptions.Remove(identifier);
            _triggeredSignals.Remove(identifier);
        }
    }

    public void Unsubscribe(object identifier, Action<object?> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        RemoveSubscription(identifier, callback);
    }

    public void Unsubscribe<TArgs>(object identifier, Action<TArgs?> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        RemoveSubscription(identifier, callback);
    }

    private void RemoveSubscription(object identifier, Delegate callback)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        lock (_lock)
        {
            if (!_subscriptions.TryGetValue(identifier, out var callbacks)) return;

            callbacks.RemoveAll(x => x.Original.Equals(callback));

            if (callbacks.Count == 0)
                _subscriptions.Remove(identifier);
        }
    }

    public bool IsSubscribed(object identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        lock (_lock)
            return _subscriptions.ContainsKey(identifier);
    }

    public bool IsSubscribed(object identifier, Action<object?> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return IsSubscribedCore(identifier, callback);
    }

    public bool IsSubscribed<TArgs>(object identifier, Action<TArgs?> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return IsSubscribedCore(identifier, callback);
    }

    private bool IsSubscribedCore(object identifier, Delegate callback)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        lock (_lock)
            return _subscriptions.TryGetValue(identifier, out var callbacks)
                && callbacks.Exists(x => x.Original.Equals(callback));
    }
}
