![SignalBus](https://github.com/Moreault/SignalBus/blob/master/signalbus.png)
# SignalBus

## Getting started

```c#
//The signal identifier can be just about anything but I do recommend using enums
public enum Signal
{
	Something,
	SomethingElse,
	AnotherThing
}

private readonly ISignalBus _signalBus;

public SomeService(ISignalBus signalBus)
{
	_signalBus = signalBus;
}

public void SomeMethod()
{
	//Whenever Signal.Something is triggered anywhere in your application this will call the 'TheSomethingMethod' below
	_signalBus.Subscribe(Signal.Something, TheSomethingMethod)
}

public void SomeOtherMethod()
{
	//This will trigger every action that has subscribed to the Signal.Something identifier with this argument
	_signalBus.Trigger(Signal.Something, new ActualArgumentType { Name = "Henry", Level = 15, Job = Job.Warrior });
}

//The callback signature requires an object? parameter even if you don't use arguments
private void TheSomethingMethod(object? args)
{
	var arguments = (ActualArgumentType)args!;

	...
}

```

### Setup

```c#
services.AddSignalBus();
```

## Strongly-typed signals

Every subscribe/unsubscribe method has a generic overload so you can work with the real argument type instead of casting from `object?` yourself. `Subscribe`, `SubscribeRetroactively`, `Unsubscribe` and `IsSubscribed` all accept an `Action<TArgs?>`.

```c#
public void SomeMethod()
{
	//The callback now receives a strongly-typed ActualArgumentType? - no casting required
	_signalBus.Subscribe<ActualArgumentType>(Signal.Something, TheSomethingMethod);
}

public void SomeOtherMethod()
{
	//Triggering is unchanged
	_signalBus.Trigger(Signal.Something, new ActualArgumentType { Name = "Henry", Level = 15, Job = Job.Warrior });
}

private void TheSomethingMethod(ActualArgumentType? args)
{
	//args is already the right type
	...
}
```

You unsubscribe (and query) with the same typed callback you subscribed with:

```c#
_signalBus.Unsubscribe<ActualArgumentType>(Signal.Something, TheSomethingMethod);

if (_signalBus.IsSubscribed<ActualArgumentType>(Signal.Something, TheSomethingMethod)) { ... }
```

The typed and weak-typed (`Action<object?>`) overloads can be mixed freely on the same signal. A typed subscriber simply receives the triggered argument cast to its type. Note that this cast is strict: if a signal is triggered with arguments that aren't assignable to `TArgs`, the typed callback throws an `InvalidCastException`. A parameterless `Trigger` (or one triggered with `null`) is passed to the callback as `default`.

## Subscribing
There are two ways to subscribe to events : `Subscribe` and `SubscribeRetroactively`.

`Subscribe` works like you would expect. The subscribing callback will be called when an object triggers the signal. `SubscribeRetroactively` does the same thing but will trigger right away if the signal was already triggered when it subscribed. It may not apply to every use case but it's useful when you want to avoid time dependencies. 

## Important
This may seem like a great idea to apply everywhere but I strongly advise against it. 
There are times when using signals is cleaner than the alternative but other times when you should prioritize localized events.

Signals should only be used for global events that affect many objects simultaneously.

For instance, if the player needs to know when an enemy dies then it may be a good idea to use signals. 
This way, the player doesn't have to subscribe to every single enemy's OnDeath event directly.

However, if you want to know when the player's collider comes into collision with an object then it may be a better idea for your player to subscribe to its collider's event directly instead of broadcasting the collision via the SignalBus.
Unless other unrelated objects need to know about this of course.

## Breaking changes

### 4.0.0
- Now targets .NET 10
- `ToolBX.AutoInject` is no longer a dependency of this library
- The `AutoInjectOptions` parameter has been removed from `AddSignalBus()`
- `SignalBus` is now registered as a singleton (previously scoped via AutoInject)