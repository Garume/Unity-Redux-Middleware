using NUnit.Framework;
using R3;
using Unity.AppUI.Redux;
using UnityReduxMiddleware.Epic;
using UnityReduxMiddleware.Tests.Runtime.Utility;

namespace UnityReduxMiddleware.Tests.Runtime.Epics
{
    [TestFixture]
    public class CombineEpicTest
    {
        [Test]
        public void CombineEpic_OnNext_ExecutedAction()
        {
            // Arrange
            var count1 = 0;
            var count2 = 0;
            var epic1 = MockEpic.Create(() => count1++);
            var epic2 = MockEpic.Create(() => count2++);
            var rootEpic = Epic.Epic.Combine(epic1, epic2);

            var stateProperty = new ReactiveProperty<MockState>();
            var actionSubject = new Subject<Action>();
            var result = rootEpic.Invoke(actionSubject, stateProperty);

            // Act
            result.Subscribe(_ => { });
            actionSubject.OnNext(new Action(""));

            // Assert
            Assert.That(count1, Is.EqualTo(1));
            Assert.That(count2, Is.EqualTo(1));
        }

        [Test]
        public void CombineEpicByBuilder_OnNext_ExecutedAction()
        {
            // Arrange
            var builder = Epic.Epic.CreateBuilder<MockState>();
            var count1 = 0;
            var count2 = 0;
            MockEpic.Create(() => count1++).AddTo(ref builder);
            MockEpic.Create(() => count2++).AddTo(ref builder);
            var rootEpic = builder.Build();

            var stateProperty = new ReactiveProperty<MockState>();
            var actionSubject = new Subject<Action>();
            var result = rootEpic.Invoke(actionSubject, stateProperty);

            // Act
            result.Subscribe(_ => { });
            actionSubject.OnNext(new Action(""));

            // Assert
            Assert.That(count1, Is.EqualTo(1));
            Assert.That(count2, Is.EqualTo(1));
        }

        [Test]
        public void CombineEpicByBuilderWithDependency_OnNext_ExecutedAction()
        {
            // Arrange
            var builder = Epic.Epic.CreateBuilder<MockState, MockDependency.Int>();
            var count1 = 0;
            var count2 = 0;
            var dependency = new MockDependency.Int { Value = 2 };
            MockEpic.CreateWithIntDependency(i => count1 += i.Value).AddTo(ref builder);
            MockEpic.CreateWithIntDependency(i => count2 += i.Value).AddTo(ref builder);
            var rootEpic = builder.Build();

            var stateProperty = new ReactiveProperty<MockState>();
            var actionSubject = new Subject<Action>();
            var result = rootEpic.Invoke(actionSubject, stateProperty, dependency);

            // Act
            result.Subscribe(_ => { });
            actionSubject.OnNext(new Action(""));

            // Assert
            Assert.That(count1, Is.EqualTo(2));
            Assert.That(count2, Is.EqualTo(2));
        }
    }
}