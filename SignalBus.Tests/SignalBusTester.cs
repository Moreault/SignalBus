namespace ToolBX.SignalBus.Tests;

[TestClass]
public class SignalBusTester
{
    [TestClass]
    public class Subscribe : Tester<SignalBus>
    {
        [TestMethod]
        public void WhenIdentifierNull_Throw()
        {
            //Arrange
            object identifier = null!;
            var callback = Fixture.Create<Action<object?>>();

            //Act
            var action = () => Instance.Subscribe(identifier, callback);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenCallbackNull_Throw()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            Action<object?> callback = null!;

            //Act
            var action = () => Instance.Subscribe(identifier, callback);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Always_Subscribe()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            var i = 0;
            Action<object?> callback = _ => i++;

            //Act
            Instance.Subscribe(identifier, callback);

            //Assert
            Instance.Trigger(identifier);
            i.Should().Be(1);
        }
    }

    [TestClass]
    public class Trigger : Tester<SignalBus>
    {
        [TestMethod]
        public void WhenIdentifierNull_Throw()
        {
            //Arrange
            object identifier = null!;

            //Act
            var action = () => Instance.Trigger(identifier);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenIdentifierIsNotSubscribed_DoNothing()
        {
            //Arrange
            var identifier = Fixture.Create<object>();

            //Act
            var action = () => Instance.Trigger(identifier);

            //Assert
            action.Should().NotThrow();
        }

        [TestMethod]
        public void WhenSignalChangesSignalCollection_DoNotThrow()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            Action<object?> callback = _ =>
            {
                var someCallback = Fixture.Create<Action<object?>>();
                Instance.Subscribe(identifier, someCallback);
            };
            Instance.Subscribe(identifier, callback);
            Instance.Subscribe(identifier, Fixture.Create<Action<object?>>());
            Instance.Subscribe(identifier, Fixture.Create<Action<object?>>());

            //Act
            var action = () => Instance.Trigger(identifier);

            //Assert
            action.Should().NotThrow();
        }

        [TestMethod]
        public void WhenSignalChangesSignalCollection_DoNotInvokeSignalOnCallbackThatWasJustAdded()
        {
            //Arrange
            var identifier = Fixture.Create<object>();

            var wasInvoked = false;
            Action<object?> callback = _ =>
            {
                Instance.Subscribe(identifier, _ => wasInvoked = true);
            };
            Instance.Subscribe(identifier, callback);
            Instance.Subscribe(identifier, Fixture.Create<Action<object?>>());
            Instance.Subscribe(identifier, Fixture.Create<Action<object?>>());

            //Act
            Instance.Trigger(identifier);

            //Assert
            wasInvoked.Should().BeFalse();
        }

        [TestMethod]
        public void WhenSignalCalledAgainRightAfterItWasChanged_CallTheNewOneOnce()
        {
            //Arrange
            var identifier = Fixture.Create<object>();

            var invokeCount = 0;
            Action<object?> callback = _ =>
            {
                Instance.Subscribe(identifier, _ => invokeCount++);
            };
            Instance.Subscribe(identifier, callback);
            Instance.Subscribe(identifier, Fixture.Create<Action<object?>>());
            Instance.Subscribe(identifier, Fixture.Create<Action<object?>>());

            //Act
            Instance.Trigger(identifier);
            Instance.Trigger(identifier);

            //Assert
            invokeCount.Should().Be(1);
        }

        [TestMethod]
        public void WhenSignalIsSubscribed_TriggerThatSignal()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            var invokeCount = 0;
            Action<object?> callback = _ => invokeCount++;
            Instance.Subscribe(identifier, callback);

            //Act
            Instance.Trigger(identifier);

            //Assert
            invokeCount.Should().Be(1);
        }
    }

    [TestClass]
    public class Trigger_Args : Tester<SignalBus>
    {
        [TestMethod]
        public void WhenIdentifierNull_Throw()
        {
            //Arrange
            object identifier = null!;
            var args = Fixture.Create<Dummy>();

            //Act
            var action = () => Instance.Trigger(identifier, args);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenIdentifierIsNotSubscribed_DoNothing()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            var args = Fixture.Create<Dummy>();

            //Act
            var action = () => Instance.Trigger(identifier, args);

            //Assert
            action.Should().NotThrow();
        }

        [TestMethod]
        public void WhenSignalChangesSignalCollection_DoNotThrow()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            var args = Fixture.Create<Dummy>();

            Action<object?> callback = _ =>
            {
                var someCallback = Fixture.Create<Action<object?>>();
                Instance.Subscribe(identifier, someCallback);
            };
            Instance.Subscribe(identifier, callback);
            Instance.Subscribe(identifier, Fixture.Create<Action<object?>>());
            Instance.Subscribe(identifier, Fixture.Create<Action<object?>>());

            //Act
            var action = () => Instance.Trigger(identifier, args);

            //Assert
            action.Should().NotThrow();
        }

        [TestMethod]
        public void WhenSignalChangesSignalCollection_DoNotInvokeSignalOnCallbackThatWasJustAdded()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            var args = Fixture.Create<Dummy>();

            var wasInvoked = false;
            Action<object?> callback = _ =>
            {
                Instance.Subscribe(identifier, _ => wasInvoked = true);
            };
            Instance.Subscribe(identifier, callback);
            Instance.Subscribe(identifier, Fixture.Create<Action<object?>>());
            Instance.Subscribe(identifier, Fixture.Create<Action<object?>>());

            //Act
            Instance.Trigger(identifier, args);

            //Assert
            wasInvoked.Should().BeFalse();
        }

        [TestMethod]
        public void WhenSignalCalledAgainRightAfterItWasChanged_CallTheNewOneOnce()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            var args = Fixture.Create<Dummy>();

            var invokeCount = 0;
            Action<object?> callback = _ =>
            {
                Instance.Subscribe(identifier, _ => invokeCount++);
            };
            Instance.Subscribe(identifier, callback);
            Instance.Subscribe(identifier, Fixture.Create<Action<object?>>());
            Instance.Subscribe(identifier, Fixture.Create<Action<object?>>());

            //Act
            Instance.Trigger(identifier, args);
            Instance.Trigger(identifier, args);

            //Assert
            invokeCount.Should().Be(1);
        }

        [TestMethod]
        public void WhenSignalIsSubscribed_TriggerThatSignalWithArgs()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            var args = Fixture.Create<Dummy>();

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
    }

    [TestClass]
    public class Clear : Tester<SignalBus>
    {
        [TestMethod]
        public void WhenIsNotCurrentlyTriggering_ClearAllIdentifiers()
        {
            //Arrange
            var identifier1 = Fixture.Create<object>();
            Instance.Subscribe(identifier1, Fixture.Create<Action<object?>>());
            Instance.Subscribe(identifier1, Fixture.Create<Action<object?>>());
            Instance.Subscribe(identifier1, Fixture.Create<Action<object?>>());
            var identifier2 = Fixture.Create<object>();
            Instance.Subscribe(identifier2, Fixture.Create<Action<object?>>());
            Instance.Subscribe(identifier2, Fixture.Create<Action<object?>>());
            Instance.Subscribe(identifier2, Fixture.Create<Action<object?>>());
            var identifier3 = Fixture.Create<object>();
            Instance.Subscribe(identifier3, Fixture.Create<Action<object?>>());
            Instance.Subscribe(identifier3, Fixture.Create<Action<object?>>());
            Instance.Subscribe(identifier3, Fixture.Create<Action<object?>>());

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
            var identifier1 = Fixture.Create<object>();

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
            var identifier2 = Fixture.Create<object>();
            Instance.Subscribe(identifier2, callback);
            Instance.Subscribe(identifier2, callback);
            Instance.Subscribe(identifier2, callback);
            var identifier3 = Fixture.Create<object>();
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
            var identifier1 = Fixture.Create<object>();

            var callback = Fixture.Create<Action<object?>>();

            Action<object?> clearingCallback = _ =>
            {
                Instance.Clear();
            };

            Instance.Subscribe(identifier1, clearingCallback);
            Instance.Subscribe(identifier1, callback);
            Instance.Subscribe(identifier1, callback);
            var identifier2 = Fixture.Create<object>();
            Instance.Subscribe(identifier2, callback);
            Instance.Subscribe(identifier2, callback);
            Instance.Subscribe(identifier2, callback);
            var identifier3 = Fixture.Create<object>();
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

    [TestClass]
    public class Clear_Identifier : Tester<SignalBus>
    {
        [TestMethod]
        public void WhenIdentifierNull_Throw()
        {
            //Arrange
            object identifier = null!;

            //Act
            var action = () => Instance.Clear(identifier);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenIsNotCurrentlyTriggering_ClearAllSubsWithIdentifierOnly()
        {
            //Arrange
            var identifier1 = Fixture.Create<object>();
            Instance.Subscribe(identifier1, Fixture.Create<Action<object?>>());
            Instance.Subscribe(identifier1, Fixture.Create<Action<object?>>());
            Instance.Subscribe(identifier1, Fixture.Create<Action<object?>>());
            var identifier2 = Fixture.Create<object>();
            Instance.Subscribe(identifier2, Fixture.Create<Action<object?>>());
            Instance.Subscribe(identifier2, Fixture.Create<Action<object?>>());
            Instance.Subscribe(identifier2, Fixture.Create<Action<object?>>());
            var identifier3 = Fixture.Create<object>();
            Instance.Subscribe(identifier3, Fixture.Create<Action<object?>>());
            Instance.Subscribe(identifier3, Fixture.Create<Action<object?>>());
            Instance.Subscribe(identifier3, Fixture.Create<Action<object?>>());

            //Act
            Instance.Clear(identifier1);

            //Assert
            Instance.IsSubscribed(identifier1).Should().BeFalse();
            Instance.IsSubscribed(identifier2).Should().BeTrue();
            Instance.IsSubscribed(identifier3).Should().BeTrue();
        }

        [TestMethod]
        public void WhenClearIsCalledByASignal_InvokeAllSignalsBeforeClearing()
        {
            //Arrange
            var identifier1 = Fixture.Create<object>();

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
            var identifier2 = Fixture.Create<object>();
            Instance.Subscribe(identifier2, callback);
            Instance.Subscribe(identifier2, callback);
            Instance.Subscribe(identifier2, callback);
            var identifier3 = Fixture.Create<object>();
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
            var identifier1 = Fixture.Create<object>();

            var callback = Fixture.Create<Action<object?>>();

            Action<object?> clearingCallback = _ =>
            {
                Instance.Clear(identifier1);
            };

            Instance.Subscribe(identifier1, clearingCallback);
            Instance.Subscribe(identifier1, callback);
            Instance.Subscribe(identifier1, callback);
            var identifier2 = Fixture.Create<object>();
            Instance.Subscribe(identifier2, callback);
            Instance.Subscribe(identifier2, callback);
            Instance.Subscribe(identifier2, callback);
            var identifier3 = Fixture.Create<object>();
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
    }

    [TestClass]
    public class Unsubscribe : Tester<SignalBus>
    {
        [TestMethod]
        public void WhenIdentifierIsNull_Throw()
        {
            //Arrange
            object identifier = null!;
            var callback = Fixture.Create<Action<object?>>();

            //Act
            var action = () => Instance.Unsubscribe(identifier, callback);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenCallbackIsNull_Throw()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            Action<object?> callback = null!;

            //Act
            var action = () => Instance.Unsubscribe(identifier, callback);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenIdentifierNotSubscribed_DoNotThrow()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            var callback = Fixture.Create<Action<object?>>();

            //Act
            var action = () => Instance.Unsubscribe(identifier, callback);

            //Assert
            action.Should().NotThrow();
        }

        [TestMethod]
        public void WhenIdentifierIsSubscribedButNotCallback_DoNotThrow()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            var callback = Fixture.Create<Action<object?>>();

            var someOtherCallback = Fixture.Create<Action<object?>>();
            Instance.Subscribe(identifier, someOtherCallback);

            //Act
            var action = () => Instance.Unsubscribe(identifier, callback);

            //Assert
            action.Should().NotThrow();
        }

        [TestMethod]
        public void WhenIdentifierIsSubscribedButNotCallback_UnsubscribeNothing()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            var callback = Fixture.Create<Action<object?>>();

            var someOtherCallback = Fixture.Create<Action<object?>>();
            Instance.Subscribe(identifier, someOtherCallback);

            //Act
            Instance.Unsubscribe(identifier, callback);

            //Assert
            Instance.IsSubscribed(identifier);
        }

        [TestMethod]
        public void WhenIsSubscribed_Unsubscribe()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            var callback = Fixture.Create<Action<object?>>();

            var someOtherCallback = Fixture.Create<Action<object?>>();
            Instance.Subscribe(identifier, callback);
            Instance.Subscribe(identifier, someOtherCallback);

            //Act
            Instance.Unsubscribe(identifier, callback);

            //Assert
            Instance.IsSubscribed(identifier, callback).Should().BeFalse();
        }

        [TestMethod]
        public void WhenIsSubscribed_DoNotUnsubscribeOtherCallbacks()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            var callback = Fixture.Create<Action<object?>>();

            var someOtherCallback = Fixture.Create<Action<object?>>();
            Instance.Subscribe(identifier, callback);
            Instance.Subscribe(identifier, someOtherCallback);

            //Act
            Instance.Unsubscribe(identifier, callback);

            //Assert
            Instance.IsSubscribed(identifier, someOtherCallback).Should().BeTrue();
        }
    }

    [TestClass]
    public class IsSubscribed : Tester<SignalBus>
    {
        [TestMethod]
        public void WhenIdentifierIsNull_Throw()
        {
            //Arrange
            object identifier = null!;

            //Act
            Action action = () => Instance.IsSubscribed(identifier);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenIdentifierIsSubscribed_ReturnTrue()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            var callback = Fixture.Create<Action<object?>>();
            Instance.Subscribe(identifier, callback);

            //Act
            var result = Instance.IsSubscribed(identifier);

            //Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenIdentifierIsNotSubscribed_ReturnFalse()
        {
            //Arrange
            var identifier = Fixture.Create<object>();

            //Act
            var result = Instance.IsSubscribed(identifier);

            //Assert
            result.Should().BeFalse();
        }
    }

    [TestClass]
    public class IsSubscribed_WithCallback : Tester<SignalBus>
    {
        [TestMethod]
        public void WhenIdentifierIsNull_Throw()
        {
            //Arrange
            object identifier = null!;
            var callback = Fixture.Create<Action<object?>>();

            //Act
            Action action = () => Instance.IsSubscribed(identifier, callback);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenCallbackIsNull_Throw()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            Action<object?> callback = null!;

            var someOtherCallback = Fixture.Create<Action<object?>>();
            Instance.Subscribe(identifier, someOtherCallback);

            //Act
            Action action = () => Instance.IsSubscribed(identifier, callback);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenCallbackIsSubscribedToIdentifier_ReturnTrue()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            var callback = Fixture.Create<Action<object?>>();
            Instance.Subscribe(identifier, callback);

            //Act
            var result = Instance.IsSubscribed(identifier, callback);

            //Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenCallbackIsSubscribedToAnotherIdentifier_ReturnFalse()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            var otherIdentifier = Fixture.Create<object>();
            var callback = Fixture.Create<Action<object?>>();
            Instance.Subscribe(otherIdentifier, callback);

            //Act
            var result = Instance.IsSubscribed(identifier, callback);

            //Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenIdentifierIsNotSubscribed_ReturnFalse()
        {
            //Arrange
            var identifier = Fixture.Create<object>();
            var callback = Fixture.Create<Action<object?>>();

            //Act
            var result = Instance.IsSubscribed(identifier, callback);

            //Assert
            result.Should().BeFalse();
        }
    }
}