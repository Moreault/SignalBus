namespace ToolBX.SignalBus.Tests;

[TestClass]
public class SignalBusTester : Tester<SignalBus>
{
    [TestMethod]
    public void Subscribe_WhenIdentifierNull_Throw()
    {
        //Arrange
        object identifier = null!;
        var callback = Dummy.Create<Action<object?>>();

        //Act
        var action = () => Instance.Subscribe(identifier, callback);

        //Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void Subscribe_WhenCallbackNull_Throw()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        Action<object?> callback = null!;

        //Act
        var action = () => Instance.Subscribe(identifier, callback);

        //Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void Subscribe_Always_Subscribe()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        var i = 0;
        Action<object?> callback = _ => i++;

        //Act
        Instance.Subscribe(identifier, callback);

        //Assert
        Instance.Trigger(identifier);
        i.Should().Be(1);
    }


    [TestMethod]
    public void SubscribeRetroactively_WhenIdentifierNull_Throw()
    {
        //Arrange
        object identifier = null!;
        var callback = Dummy.Create<Action<object?>>();

        //Act
        var action = () => Instance.SubscribeRetroactively(identifier, callback);

        //Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void SubscribeRetroactively_WhenCallbackNull_Throw()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        Action<object?> callback = null!;

        //Act
        var action = () => Instance.SubscribeRetroactively(identifier, callback);

        //Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void SubscribeRetroactively_Always_Subscribe()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        var i = 0;
        Action<object?> callback = _ => i++;

        //Act
        Instance.SubscribeRetroactively(identifier, callback);

        //Assert
        Instance.Trigger(identifier);
        i.Should().Be(1);
    }

    [TestMethod]
    public void SubscribeRetroactively_WhenSignalTriggeredBeforeSubscribing_TriggerForCallerOnly()
    {
        //Arrange
        var identifier = Dummy.Create<object>();

        var i = 0;
        var y = 0;

        Instance.Trigger(identifier);

        Instance.Subscribe(identifier, _ => y++);

        //Act
        Instance.SubscribeRetroactively(identifier, _ => i++);

        //Assert
        i.Should().Be(1);
        y.Should().Be(0);
    }

    [TestMethod]
    public void Trigger_WhenIdentifierNull_Throw()
    {
        //Arrange
        object identifier = null!;

        //Act
        var action = () => Instance.Trigger(identifier);

        //Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void Trigger_WhenIdentifierIsNotSubscribed_DoNothing()
    {
        //Arrange
        var identifier = Dummy.Create<object>();

        //Act
        var action = () => Instance.Trigger(identifier);

        //Assert
        action.Should().NotThrow();
    }

    [TestMethod]
    public void Trigger_WhenSignalChangesSignalCollection_DoNotThrow()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        Action<object?> callback = _ =>
        {
            var someCallback = Dummy.Create<Action<object?>>();
            Instance.Subscribe(identifier, someCallback);
        };
        Instance.Subscribe(identifier, callback);
        Instance.Subscribe(identifier, Dummy.Create<Action<object?>>());
        Instance.Subscribe(identifier, Dummy.Create<Action<object?>>());

        //Act
        var action = () => Instance.Trigger(identifier);

        //Assert
        action.Should().NotThrow();
    }

    [TestMethod]
    public void Trigger_WhenSignalChangesSignalCollection_DoNotInvokeSignalOnCallbackThatWasJustAdded()
    {
        //Arrange
        var identifier = Dummy.Create<object>();

        var wasInvoked = false;
        Action<object?> callback = _ =>
        {
            Instance.Subscribe(identifier, _ => wasInvoked = true);
        };
        Instance.Subscribe(identifier, callback);
        Instance.Subscribe(identifier, Dummy.Create<Action<object?>>());
        Instance.Subscribe(identifier, Dummy.Create<Action<object?>>());

        //Act
        Instance.Trigger(identifier);

        //Assert
        wasInvoked.Should().BeFalse();
    }

    [TestMethod]
    public void Trigger_WhenSignalCalledAgainRightAfterItWasChanged_CallTheNewOneOnce()
    {
        //Arrange
        var identifier = Dummy.Create<object>();

        var invokeCount = 0;
        Action<object?> callback = _ =>
        {
            Instance.Subscribe(identifier, _ => invokeCount++);
        };
        Instance.Subscribe(identifier, callback);
        Instance.Subscribe(identifier, Dummy.Create<Action<object?>>());
        Instance.Subscribe(identifier, Dummy.Create<Action<object?>>());

        //Act
        Instance.Trigger(identifier);
        Instance.Trigger(identifier);

        //Assert
        invokeCount.Should().Be(1);
    }

    [TestMethod]
    public void Trigger_WhenSignalIsSubscribed_TriggerThatSignal()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        var invokeCount = 0;
        Action<object?> callback = _ => invokeCount++;
        Instance.Subscribe(identifier, callback);

        //Act
        Instance.Trigger(identifier);

        //Assert
        invokeCount.Should().Be(1);
    }

    [TestMethod]
    public void Trigger_Args_WhenIdentifierNull_Throw()
    {
        //Arrange
        object identifier = null!;
        var args = Dummy.Create<Garbage>();

        //Act
        var action = () => Instance.Trigger(identifier, args);

        //Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void Trigger_Args_WhenIdentifierIsNotSubscribed_DoNothing()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        var args = Dummy.Create<Garbage>();

        //Act
        var action = () => Instance.Trigger(identifier, args);

        //Assert
        action.Should().NotThrow();
    }

    [TestMethod]
    public void Trigger_Args_WhenSignalChangesSignalCollection_DoNotThrow()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        var args = Dummy.Create<Garbage>();

        Action<object?> callback = _ =>
        {
            var someCallback = Dummy.Create<Action<object?>>();
            Instance.Subscribe(identifier, someCallback);
        };
        Instance.Subscribe(identifier, callback);
        Instance.Subscribe(identifier, Dummy.Create<Action<object?>>());
        Instance.Subscribe(identifier, Dummy.Create<Action<object?>>());

        //Act
        var action = () => Instance.Trigger(identifier, args);

        //Assert
        action.Should().NotThrow();
    }

    [TestMethod]
    public void Trigger_Args_WhenSignalChangesSignalCollection_DoNotInvokeSignalOnCallbackThatWasJustAdded()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        var args = Dummy.Create<Garbage>();

        var wasInvoked = false;
        Action<object?> callback = _ =>
        {
            Instance.Subscribe(identifier, _ => wasInvoked = true);
        };
        Instance.Subscribe(identifier, callback);
        Instance.Subscribe(identifier, Dummy.Create<Action<object?>>());
        Instance.Subscribe(identifier, Dummy.Create<Action<object?>>());

        //Act
        Instance.Trigger(identifier, args);

        //Assert
        wasInvoked.Should().BeFalse();
    }

    [TestMethod]
    public void Trigger_Args_WhenSignalCalledAgainRightAfterItWasChanged_CallTheNewOneOnce()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        var args = Dummy.Create<Garbage>();

        var invokeCount = 0;
        Action<object?> callback = _ =>
        {
            Instance.Subscribe(identifier, _ => invokeCount++);
        };
        Instance.Subscribe(identifier, callback);
        Instance.Subscribe(identifier, Dummy.Create<Action<object?>>());
        Instance.Subscribe(identifier, Dummy.Create<Action<object?>>());

        //Act
        Instance.Trigger(identifier, args);
        Instance.Trigger(identifier, args);

        //Assert
        invokeCount.Should().Be(1);
    }

    [TestMethod]
    public void Trigger_Args_WhenSignalIsSubscribed_TriggerThatSignalWithArgs()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        var args = Dummy.Create<Garbage>();

        var invokeCount = 0;
        object invokedArgs = null!;
        Action<object?> callback = x =>
        {
            invokeCount++;
            invokedArgs = x!;
        };
        Instance.Subscribe(identifier, callback);

        //Act
        Instance.Trigger(identifier, args);

        //Assert
        invokeCount.Should().Be(1);
        invokedArgs.Should().Be(args);
    }

    [TestClass]
    public class Clear : Tester<SignalBus>
    {
        [TestMethod]
        public void WhenIsNotCurrentlyTriggering_ClearAllIdentifiers()
        {
            //Arrange
            var identifier1 = Dummy.Create<object>();
            Instance.Subscribe(identifier1, Dummy.Create<Action<object?>>());
            Instance.Subscribe(identifier1, Dummy.Create<Action<object?>>());
            Instance.Subscribe(identifier1, Dummy.Create<Action<object?>>());
            var identifier2 = Dummy.Create<object>();
            Instance.Subscribe(identifier2, Dummy.Create<Action<object?>>());
            Instance.Subscribe(identifier2, Dummy.Create<Action<object?>>());
            Instance.Subscribe(identifier2, Dummy.Create<Action<object?>>());
            var identifier3 = Dummy.Create<object>();
            Instance.Subscribe(identifier3, Dummy.Create<Action<object?>>());
            Instance.Subscribe(identifier3, Dummy.Create<Action<object?>>());
            Instance.Subscribe(identifier3, Dummy.Create<Action<object?>>());

            //Act
            Instance.Clear();

            //Assert
            Instance.IsSubscribed(identifier1).Should().BeFalse();
            Instance.IsSubscribed(identifier2).Should().BeFalse();
            Instance.IsSubscribed(identifier3).Should().BeFalse();
        }

        [TestMethod]
        public void WhenClearIsCalledByASignal_InvokeAllSignalsBeforeClearing()
        {
            //Arrange
            var identifier1 = Dummy.Create<object>();

            var invokeCount = 0;
            Action<object?> callback = _ => invokeCount++;

            Action<object?> clearingCallback = _ =>
            {
                invokeCount++;
                Instance.Clear();
            };

            Instance.Subscribe(identifier1, clearingCallback);
            Instance.Subscribe(identifier1, callback);
            Instance.Subscribe(identifier1, callback);
            var identifier2 = Dummy.Create<object>();
            Instance.Subscribe(identifier2, callback);
            Instance.Subscribe(identifier2, callback);
            Instance.Subscribe(identifier2, callback);
            var identifier3 = Dummy.Create<object>();
            Instance.Subscribe(identifier3, callback);
            Instance.Subscribe(identifier3, callback);
            Instance.Subscribe(identifier3, callback);

            //Act
            Instance.Trigger(identifier1);

            //Assert
            invokeCount.Should().Be(3);
        }

        [TestMethod]
        public void WhenClearIsCalledByASignal_Clear()
        {
            //Arrange
            var identifier1 = Dummy.Create<object>();

            var callback = Dummy.Create<Action<object?>>();

            Action<object?> clearingCallback = _ =>
            {
                Instance.Clear();
            };

            Instance.Subscribe(identifier1, clearingCallback);
            Instance.Subscribe(identifier1, callback);
            Instance.Subscribe(identifier1, callback);
            var identifier2 = Dummy.Create<object>();
            Instance.Subscribe(identifier2, callback);
            Instance.Subscribe(identifier2, callback);
            Instance.Subscribe(identifier2, callback);
            var identifier3 = Dummy.Create<object>();
            Instance.Subscribe(identifier3, callback);
            Instance.Subscribe(identifier3, callback);
            Instance.Subscribe(identifier3, callback);

            //Act
            Instance.Trigger(identifier1);

            //Assert
            Instance.IsSubscribed(identifier1).Should().BeFalse();
            Instance.IsSubscribed(identifier2).Should().BeFalse();
            Instance.IsSubscribed(identifier3).Should().BeFalse();
        }
    }

    [TestMethod]
    public void ClearIdentifier_WhenIdentifierNull_Throw()
    {
        //Arrange
        object identifier = null!;

        //Act
        var action = () => Instance.Clear(identifier);

        //Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void ClearIdentifier_WhenIsNotCurrentlyTriggering_ClearAllSubsWithIdentifierOnly()
    {
        //Arrange
        var identifier1 = Dummy.Create<object>();
        Instance.Subscribe(identifier1, Dummy.Create<Action<object?>>());
        Instance.Subscribe(identifier1, Dummy.Create<Action<object?>>());
        Instance.Subscribe(identifier1, Dummy.Create<Action<object?>>());
        var identifier2 = Dummy.Create<object>();
        Instance.Subscribe(identifier2, Dummy.Create<Action<object?>>());
        Instance.Subscribe(identifier2, Dummy.Create<Action<object?>>());
        Instance.Subscribe(identifier2, Dummy.Create<Action<object?>>());
        var identifier3 = Dummy.Create<object>();
        Instance.Subscribe(identifier3, Dummy.Create<Action<object?>>());
        Instance.Subscribe(identifier3, Dummy.Create<Action<object?>>());
        Instance.Subscribe(identifier3, Dummy.Create<Action<object?>>());

        //Act
        Instance.Clear(identifier1);

        //Assert
        Instance.IsSubscribed(identifier1).Should().BeFalse();
        Instance.IsSubscribed(identifier2).Should().BeTrue();
        Instance.IsSubscribed(identifier3).Should().BeTrue();
    }

    [TestMethod]
    public void ClearIdentifier_WhenClearIsCalledByASignal_InvokeAllSignalsBeforeClearing()
    {
        //Arrange
        var identifier1 = Dummy.Create<object>();

        var invokeCount = 0;
        Action<object?> callback = _ => invokeCount++;

        Action<object?> clearingCallback = _ =>
        {
            invokeCount++;
            Instance.Clear(identifier1);
        };

        Instance.Subscribe(identifier1, clearingCallback);
        Instance.Subscribe(identifier1, callback);
        Instance.Subscribe(identifier1, callback);
        var identifier2 = Dummy.Create<object>();
        Instance.Subscribe(identifier2, callback);
        Instance.Subscribe(identifier2, callback);
        Instance.Subscribe(identifier2, callback);
        var identifier3 = Dummy.Create<object>();
        Instance.Subscribe(identifier3, callback);
        Instance.Subscribe(identifier3, callback);
        Instance.Subscribe(identifier3, callback);

        //Act
        Instance.Trigger(identifier1);

        //Assert
        invokeCount.Should().Be(3);
    }

    [TestMethod]
    public void ClearIdentifier_WhenClearIsCalledByASignal_Clear()
    {
        //Arrange
        var identifier1 = Dummy.Create<object>();

        var callback = Dummy.Create<Action<object?>>();

        Action<object?> clearingCallback = _ =>
        {
            Instance.Clear(identifier1);
        };

        Instance.Subscribe(identifier1, clearingCallback);
        Instance.Subscribe(identifier1, callback);
        Instance.Subscribe(identifier1, callback);
        var identifier2 = Dummy.Create<object>();
        Instance.Subscribe(identifier2, callback);
        Instance.Subscribe(identifier2, callback);
        Instance.Subscribe(identifier2, callback);
        var identifier3 = Dummy.Create<object>();
        Instance.Subscribe(identifier3, callback);
        Instance.Subscribe(identifier3, callback);
        Instance.Subscribe(identifier3, callback);

        //Act
        Instance.Trigger(identifier1);

        //Assert
        Instance.IsSubscribed(identifier1).Should().BeFalse();
        Instance.IsSubscribed(identifier2).Should().BeTrue();
        Instance.IsSubscribed(identifier3).Should().BeTrue();
    }

    [TestMethod]
    public void Unsubscribe_WhenIdentifierIsNull_Throw()
    {
        //Arrange
        object identifier = null!;
        var callback = Dummy.Create<Action<object?>>();

        //Act
        var action = () => Instance.Unsubscribe(identifier, callback);

        //Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void Unsubscribe_WhenCallbackIsNull_Throw()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        Action<object?> callback = null!;

        //Act
        var action = () => Instance.Unsubscribe(identifier, callback);

        //Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void Unsubscribe_WhenIdentifierNotSubscribed_DoNotThrow()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        var callback = Dummy.Create<Action<object?>>();

        //Act
        var action = () => Instance.Unsubscribe(identifier, callback);

        //Assert
        action.Should().NotThrow();
    }

    [TestMethod]
    public void Unsubscribe_WhenIdentifierIsSubscribedButNotCallback_DoNotThrow()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        var callback = Dummy.Create<Action<object?>>();

        var someOtherCallback = Dummy.Create<Action<object?>>();
        Instance.Subscribe(identifier, someOtherCallback);

        //Act
        var action = () => Instance.Unsubscribe(identifier, callback);

        //Assert
        action.Should().NotThrow();
    }

    [TestMethod]
    public void Unsubscribe_WhenIdentifierIsSubscribedButNotCallback_UnsubscribeNothing()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        var callback = Dummy.Create<Action<object?>>();

        var someOtherCallback = Dummy.Create<Action<object?>>();
        Instance.Subscribe(identifier, someOtherCallback);

        //Act
        Instance.Unsubscribe(identifier, callback);

        //Assert
        Instance.IsSubscribed(identifier).Should().BeTrue();
    }

    [TestMethod]
    public void Unsubscribe_WhenIsSubscribed_Unsubscribe()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        var callback = Dummy.Create<Action<object?>>();

        var someOtherCallback = Dummy.Create<Action<object?>>();
        Instance.Subscribe(identifier, callback);
        Instance.Subscribe(identifier, someOtherCallback);

        //Act
        Instance.Unsubscribe(identifier, callback);

        //Assert
        Instance.IsSubscribed(identifier, callback).Should().BeFalse();
    }

    [TestMethod]
    public void Unsubscribe_WhenIsSubscribed_DoNotUnsubscribeOtherCallbacks()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        var callback = Dummy.Create<Action<object?>>();

        var someOtherCallback = Dummy.Create<Action<object?>>();
        Instance.Subscribe(identifier, callback);
        Instance.Subscribe(identifier, someOtherCallback);

        //Act
        Instance.Unsubscribe(identifier, callback);

        //Assert
        Instance.IsSubscribed(identifier, someOtherCallback).Should().BeTrue();
    }

    [TestMethod]
    public void IsSubscribed_WhenIdentifierIsNull_Throw()
    {
        //Arrange
        object identifier = null!;

        //Act
        Action action = () => Instance.IsSubscribed(identifier);

        //Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void IsSubscribed_WhenIdentifierIsSubscribed_ReturnTrue()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        var callback = Dummy.Create<Action<object?>>();
        Instance.Subscribe(identifier, callback);

        //Act
        var result = Instance.IsSubscribed(identifier);

        //Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsSubscribed_WhenIdentifierIsNotSubscribed_ReturnFalse()
    {
        //Arrange
        var identifier = Dummy.Create<object>();

        //Act
        var result = Instance.IsSubscribed(identifier);

        //Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsSubscribedWithCallback_WhenIdentifierIsNull_Throw()
    {
        //Arrange
        object identifier = null!;
        var callback = Dummy.Create<Action<object?>>();

        //Act
        Action action = () => Instance.IsSubscribed(identifier, callback);

        //Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void IsSubscribedWithCallback_WhenCallbackIsNull_Throw()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        Action<object?> callback = null!;

        var someOtherCallback = Dummy.Create<Action<object?>>();
        Instance.Subscribe(identifier, someOtherCallback);

        //Act
        Action action = () => Instance.IsSubscribed(identifier, callback);

        //Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void IsSubscribedWithCallback_WhenCallbackIsSubscribedToIdentifier_ReturnTrue()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        var callback = Dummy.Create<Action<object?>>();
        Instance.Subscribe(identifier, callback);

        //Act
        var result = Instance.IsSubscribed(identifier, callback);

        //Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsSubscribedWithCallback_WhenCallbackIsSubscribedToAnotherIdentifier_ReturnFalse()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        var otherIdentifier = Dummy.Create<object>();
        var callback = Dummy.Create<Action<object?>>();
        Instance.Subscribe(otherIdentifier, callback);

        //Act
        var result = Instance.IsSubscribed(identifier, callback);

        //Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsSubscribedWithCallback_WhenIdentifierIsNotSubscribed_ReturnFalse()
    {
        //Arrange
        var identifier = Dummy.Create<object>();
        var callback = Dummy.Create<Action<object?>>();

        //Act
        var result = Instance.IsSubscribed(identifier, callback);

        //Assert
        result.Should().BeFalse();
    }
}