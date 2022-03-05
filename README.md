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

public SomeOtherMethod()
{
	//This will trigger every action that has subscribed to the Signal.Something identifier with this argument
	_signalBus.Trigger(Signal.Something, new ActualArgumentType { Name = "Henry", Level = 15, Job = Job.Warrior });
}

//You do need an object-typed args parameter even if you don't use arguments... I haven't found a better way to deal with this yet unfortunately
private void TheSomethingMethod(object args)
{
	var arguments = (ActualArgumentType)args;

	...
}

```

### Setup

#### With AutoInject

If you already use AutoInject or AssemblyInitializer then you're already good to go.

See the AutoInject repo for more information on how to set it up.

#### Without AutoInject

Add the following line when adding services.

```c#
services.AddSignalBus();
```

## Important
This may seem like a great idea to apply everywhere but I strongly advise against it. 
There are times when using signals is cleaner than the alternative but other times when you should prioritize localized events.

Signals should only be used for global events that affect many objects simultaneously.

For instance, if the player needs to know when an enemy dies then it may be a good idea to use signals. 
This way, the player doesn't have to subscribe to every single enemy's OnDeath event directly.

However, if you want to know when the player's collider comes into collision with an object then it may be a better idea for your player to subscribe to its collider's event directly instead of broadcasting the collision via the SignalBus.
Unless other unrelated objects need to know about this of course.