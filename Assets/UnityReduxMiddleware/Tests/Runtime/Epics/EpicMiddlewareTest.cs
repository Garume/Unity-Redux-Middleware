using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityReduxMiddleware.Epic;
using UnityReduxMiddleware.Tests.Runtime.Utility;
using Action = Unity.AppUI.Redux.Action;

namespace UnityReduxMiddleware.Tests.Runtime.Epics
{
    [TestFixture]
    public class EpicMiddlewareTest
    {
        [Test]
        public void EpicMiddleware_EpicAdded_Execute()
        {
            // Arrange
            var count = 0;
            var epic = MockEpic.Create(() => count++);
            var epicMiddleware = EpicMiddleware.Default<MockState>();
            var store = new MiddlewareStore();
            store.CreateSlice("app", new MockState(), _ => { });
            store.AddMiddleware(epicMiddleware.Create());
            epicMiddleware.Run(epic);
            // Act
            store.Dispatch(new Action(""));
            // Assert
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void EpicMiddlewareWithDependency_EpicAdded_Execute()
        {
            // Arrange
            var count = 0;
            var dependency = new MockDependency.Int { Value = 2 };
            var epic = MockEpic.CreateWithIntDependency(i => count += i.Value);
            var epicMiddleware = EpicMiddleware.Default<MockState, MockDependency.Int>(dependency);
            var store = new MiddlewareStore();
            store.CreateSlice("app", new MockState(), _ => { });
            store.AddMiddleware(epicMiddleware.Create());
            epicMiddleware.Run(epic);
            // Act
            store.Dispatch(new Action(""));
            // Assert
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public void EpicMiddleware_EpicNoReturnObservable_ThrowInvalidException()
        {
            // Arrange
            var epic = MockEpic.CreateReturnNull();
            var epicMiddleware = EpicMiddleware.Default<MockState>();
            var store = new MiddlewareStore();
            store.CreateSlice("app", new MockState(), _ => { });
            store.AddMiddleware(epicMiddleware.Create());
            epicMiddleware.Run(epic);
            // Act
            // Assert
            LogAssert.Expect(LogType.Exception, "Exception: Epic must return an observable.");
            store.Dispatch(new Action(""));
        }
    }
}