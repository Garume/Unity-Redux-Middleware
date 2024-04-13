using NUnit.Framework;
using R3;
using Unity.AppUI.Redux;
using UnityReduxMiddleware.Tests.Runtime.Utility;
using Action = Unity.AppUI.Redux.Action;

namespace UnityReduxMiddleware.Tests.Runtime.Epics
{
    [TestFixture]
    public class EpicTest
    {
        [Test]
        public void Epic_OnNext_ExecuteAction()
        {
            // Arrange
            var count = 0;
            var epic = MockEpic.Create(() => count++);
            var stateProperty = new ReactiveProperty<MockState>();
            var actionSubject = new Subject<Action>();
            var result = epic.Invoke(actionSubject, stateProperty);

            // Act
            result.Subscribe(_ => { });
            actionSubject.OnNext(new Action(""));

            // Assert
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void Epic_OnNext_ExecuteActionWithPayload()
        {
            // Arrange
            var count = 0;
            var actionType = Store.CreateAction<int>("MockState");
            var epic = Epic.Epic.Create<MockState>(actionType, (action, state) =>
            {
                return action.Do(x =>
                {
                    var actionPayload = (Action<int>)x;
                    count += actionPayload.payload;
                });
            });
            var stateProperty = new ReactiveProperty<MockState>();
            var actionSubject = new Subject<Action>();
            var result = epic.Invoke(actionSubject, stateProperty);

            // Act
            result.Subscribe(_ => { });
            actionSubject.OnNext(actionType.Invoke(2));

            // Assert
            Assert.That(count, Is.EqualTo(2));
        }


        [Test]
        public void Epic_MultiOnNext_ExecuteAction()
        {
            // Arrange
            var count = 0;
            var epic = MockEpic.Create(() => count++);
            var stateProperty = new ReactiveProperty<MockState>();
            var actionSubject = new Subject<Action>();
            var result = epic.Invoke(actionSubject, stateProperty);

            // Act
            result.Subscribe(_ => { });
            actionSubject.OnNext(new Action(""));
            actionSubject.OnNext(new Action(""));

            // Assert
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public void EpicWithDependency_OnNext_ExecuteAction()
        {
            // Arrange
            var count = 0;
            var dependency = new MockDependency.Int { Value = 2 };
            var epic = MockEpic.CreateWithIntDependency(i => count += i.Value);
            var stateProperty = new ReactiveProperty<MockState>();
            var actionSubject = new Subject<Action>();
            var result = epic.Invoke(actionSubject, stateProperty, dependency);

            // Act
            result.Subscribe(_ => { });
            actionSubject.OnNext(new Action(""));

            // Assert
            Assert.That(count, Is.EqualTo(2));
        }
    }
}