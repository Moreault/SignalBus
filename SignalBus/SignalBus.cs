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
    /// Subscribes an action to execute once the identifier is triggered and also instantly triggers if it was triggered at least once before subscribing.
    /// </summary>
    void SubscribeRetroactively(object identifier, Action<object?> callback);

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
    /// Returns whether or not there is anything listening to the specified identifier.
    /// </summary>
    bool IsSubscribed(object identifier);

    /// <summary>
    /// Returns whether or not a callback action is listening for the specified identifier.
    /// </summary>
    bool IsSubscribed(object identifier, Action<object?> callback);
}

/// <inheritdoc cref="ISignalBus"/>
[AutoInject(Lifetime = ServiceLifetime.Scoped)]
public class SignalBus : ISignalBus
{
    private readonly IDictionary<object, IList<Action<object?>>> _subscriptions = new Dictionary<object, IList<Action<object?>>>();

    private bool _isExecuting;
    private readonly List<Action> _deferredActions = new();
    private readonly Dictionary<object, object?> _triggeredSignals = new();

    public void Subscribe(object identifier, Action<object?> callback)
    {
        if (identifier == null) throw new ArgumentNullException(nameof(identifier));
        if (callback == null) throw new ArgumentNullException(nameof(callback));

        if (_isExecuting)
            _deferredActions.Add(() => SubscribeInternal(identifier, callback));
        else
            SubscribeInternal(identifier, callback);
    }

    public void SubscribeRetroactively(object identifier, Action<object?> callback)
    {
        Subscribe(identifier, callback);

        if (_triggeredSignals.TryGetValue(identifier, out var args))
            callback.Invoke(args);
    }

    private void SubscribeInternal(object identifier, Action<object?> callback)
    {
        if (!IsSubscribed(identifier))
            _subscriptions[identifier] = new List<Action<object?>>();

        _subscriptions[identifier].Add(callback);
    }

    public void Trigger(object identifier) => Trigger<object>(identifier, null!);

    public void Trigger<TArgs>(object identifier, TArgs? args)
    {
        if (identifier == null) throw new ArgumentNullException(nameof(identifier));
        if (!IsSubscribed(identifier)) return;

        _isExecuting = true;
        foreach (var sub in _subscriptions[identifier])
            sub.Invoke(args);
        _isExecuting = false;

        foreach (var action in _deferredActions)
            action.Invoke();
        _deferredActions.Clear();

        _triggeredSignals[identifier] = args;
    }

    public void Clear()
    {
        if (_isExecuting)
            _deferredActions.Add(() => _subscriptions.Clear());
        else
            _subscriptions.Clear();

        _triggeredSignals.Clear();
    }

    public void Clear(object identifier)
    {
        if (identifier == null) throw new ArgumentNullException(nameof(identifier));
        if (_isExecuting)
            _deferredActions.Add(() => ClearInternal(identifier));
        else
            ClearInternal(identifier);
    }

    private void ClearInternal(object identifier)
    {
        if (IsSubscribed(identifier))
        {
            _subscriptions[identifier].Clear();
            _subscriptions.Remove(identifier);
        }
    }

    public void Unsubscribe(object identifier, Action<object?> callback)
    {
        if (identifier == null) throw new ArgumentNullException(nameof(identifier));
        if (callback == null) throw new ArgumentNullException(nameof(callback));

        if (_isExecuting)
            _deferredActions.Add(() => UnsubscribeInternal(identifier, callback));
        else
            UnsubscribeInternal(identifier, callback);
    }

    private void UnsubscribeInternal(object identifier, Action<object?> callback)
    {
        if (!IsSubscribed(identifier, callback)) return;
        var hashset = _subscriptions[identifier];
        var subscription = hashset.SingleOrDefault(x => x == callback);
        if (subscription == null) return;
        hashset.Remove(subscription);

        if (!_subscriptions[identifier].Any())
            _subscriptions.Remove(identifier);
    }

    public bool IsSubscribed(object identifier) => _subscriptions.ContainsKey(identifier ?? throw new ArgumentNullException(nameof(identifier)));

    public bool IsSubscribed(object identifier, Action<object?> callback) => IsSubscribed(identifier) && _subscriptions[identifier].Contains(callback ?? throw new ArgumentNullException(nameof(callback)));
}