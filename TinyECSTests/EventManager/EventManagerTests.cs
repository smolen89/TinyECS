﻿using NUnit.Framework;
using TinyECS.Interfaces;
using TinyECS.Impls;
using System;
using NSubstitute;
using System.Collections.Generic;

namespace TinyECSTests
{
    [TestFixture]
    public class EventManagerTests
    {
        public struct TSimpleEvent: IEvent
        {
        }

        public struct TAnotherEvent: IEvent
        {
        }

        protected IEventManager mEventManager;

        [SetUp]
        public void Init()
        {
            mEventManager = new EventManager();
        }

        [Test]
        public void TestSubscribe_PassNullArgument_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                mEventManager.Subscribe<TSimpleEvent>(null);
            });
        }

        [Test]
        public void TestSubscribe_PassCorrectReference_ReturnsListenerIdentifier()
        {
            Assert.DoesNotThrow(() =>
            {
                IEventListener eventListener = Substitute.For<IEventListener>();

                mEventManager.Subscribe<TSimpleEvent>(eventListener);
            });
        }

        [Test]
        public void TestSubscribe_MultipleSubscriptionsCreatesNewListener_ReturnsListenerIdentifier()
        {
            Assert.DoesNotThrow(() =>
            {
                HashSet<uint> createdListenersIds = new HashSet<uint>();

                IEventListener eventListener = Substitute.For<IEventListener>();

                uint registeredListenerId = 0;

                for (int i = 0; i < 4; ++i)
                {
                    registeredListenerId = mEventManager.Subscribe<TSimpleEvent>(eventListener);

                    // all registered listeners have unique identifiers
                    Assert.IsFalse(createdListenersIds.Contains(registeredListenerId));

                    createdListenersIds.Add(registeredListenerId);
                }
            });
        }

        [Test]
        public void TestUnsubscribe_PassInexistingId_ThrowsListenerDoesntExistException()
        {
            Assert.Throws<ListenerDoesntExistException>(() =>
            {
                mEventManager.Unsubscribe(0);
            });
        }

        [Test]
        public void TestUnsubscribe_PassCorrectExistingId_RemovesListenerFromManager()
        {
            Assert.DoesNotThrow(() =>
            {
                // subscribe to some event
                IEventListener eventListener = Substitute.For<IEventListener>();

                uint registeredListenerId = mEventManager.Subscribe<TSimpleEvent>(eventListener);

                // try to unsubscribe the listener
                mEventManager.Unsubscribe(registeredListenerId);
            });
        }

        [Test]
        public void TestNotify_InvokeOnEmptyArrayOfListeners_DoNothing()
        {
            Assert.DoesNotThrow(() =>
            {
                //mEventManager has no subscribed listeners
                mEventManager.Notify<TSimpleEvent>(new TSimpleEvent());
            });
        }

        [Test]
        public void TestNotify_InvokeForEventThatHasNoListeners_DoNothing()
        {
            Assert.DoesNotThrow(() =>
            {
                int counter = 0;

                // subscribe to TSimpleEvent event
                IEventListener<TSimpleEvent> eventListener = Substitute.For<IEventListener<TSimpleEvent>>();

                eventListener.When(e => e.OnEvent(new TSimpleEvent())).Do(e => ++counter);

                uint registeredListenerId = mEventManager.Subscribe<TSimpleEvent>(eventListener);

                // but try to notify listeners of TAnotherEvent
                mEventManager.Notify<TAnotherEvent>(new TAnotherEvent());

                Assert.AreEqual(0, counter);
            });
        }

        [Test]
        public void TestNotify_InvokeForEventThatHasFewListeners_DoNothing()
        {
            Assert.DoesNotThrow(() =>
            {
                int expectedCallsCount = 5;

                const int numOfListeners = 4;

                int[] counters = new int[numOfListeners];

                IEventListener<TSimpleEvent>[] listeners = new IEventListener<TSimpleEvent>[numOfListeners];

                IEventListener<TSimpleEvent> currListener = null;

                for (int i = 0; i < numOfListeners; ++i)
                {
                    // subscribe to TSimpleEvent event
                    currListener = Substitute.For<IEventListener<TSimpleEvent>>();

                    int currIndex = i;

                    currListener.When(e => e.OnEvent(new TSimpleEvent())).Do(e => ++counters[currIndex]);

                    uint registeredListenerId = mEventManager.Subscribe<TSimpleEvent>(currListener);

                    listeners[i] = currListener;
                }

                
                for (int i = 0; i < expectedCallsCount; ++i)
                {
                    mEventManager.Notify<TSimpleEvent>(new TSimpleEvent());
                }

                for (int i = 0; i < numOfListeners; ++i)
                {
                    Assert.AreEqual(expectedCallsCount, counters[i]);
                }
            });
        }

        [Test]
        public void TestNotify_InvokeEventForSpecificListener_SendEventToSpecifiedListener()
        {
            Assert.DoesNotThrow(() =>
            {
                int expectedCallsCount = 5;

                const int numOfListeners = 4;

                int[] counters = new int[numOfListeners];

                IEventListener<TSimpleEvent>[] listeners = new IEventListener<TSimpleEvent>[numOfListeners];

                IEventListener<TSimpleEvent> currListener = null;

                uint destListenerId = 0;

                for (int i = 0; i < numOfListeners; ++i)
                {
                    // subscribe to TSimpleEvent event
                    currListener = Substitute.For<IEventListener<TSimpleEvent>>();

                    int currIndex = i;

                    currListener.When(e => e.OnEvent(new TSimpleEvent())).Do(e => ++counters[currIndex]);

                    uint registeredListenerId = mEventManager.Subscribe<TSimpleEvent>(currListener);

                    listeners[i] = currListener;

                    destListenerId = (i == 2) ? registeredListenerId : destListenerId;
                }

                for (int i = 0; i < expectedCallsCount; ++i)
                {
                    mEventManager.Notify<TSimpleEvent>(new TSimpleEvent(), destListenerId);
                }

                for (int i = 0; i < numOfListeners; ++i)
                {
                    Assert.AreEqual((i != destListenerId) ? 0 : expectedCallsCount, counters[i]);
                }
            });
        }

        [Test]
        public void TestNotify_InvokeSomeEventWhenSomeListenerDoesntProvideOnEventMethod_ShouldWorkCorrectWithoutExceptions()
        {
            Assert.DoesNotThrow(() => 
            {
                // create an event listener and subcribe it to both events
                IEventListener listener = new BrokenEventListenerMock(mEventManager);

                // try to notify the listener about TAnotherEvent event has happened
                // this method should correctly process listeners which does not provide all OnEvent's implementations
                mEventManager.Notify(new TAnotherEvent { });                
            });
        }

        [Test]
        public void TestNotify_NotifyPassInternalExceptionOutwards_NotifyThrowsInternalExceptionsAsItsOwn()
        {
            // create an event listener that throws NotImplementException within OnEvent
            IEventListener<TSimpleEvent> listener = Substitute.For<IEventListener<TSimpleEvent>>();

            listener.When(e => e.OnEvent(new TSimpleEvent())).Do(e => throw new NotImplementedException());

            mEventManager.Subscribe<TSimpleEvent>(listener);

            Assert.Throws<NotImplementedException>(() =>
            {
                mEventManager.Notify(new TSimpleEvent { });
            });
        }
    }
}           