using System.Threading.Tasks;
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
            var epic2 = MockEpic.Create(() => count += 2);
            var epicMiddleware = EpicMiddleware.Default<MockState>();
            var store = new MiddlewareStore();
            store.CreateSlice("app", new MockState(), _ => { });
            var dispatch = epicMiddleware.Create().Invoke(store)((_, _) => Task.CompletedTask);
            epicMiddleware.Run(Epic.Epic.Combine(epic, epic2));

            // Act
            dispatch(new Action("")).Wait();
            dispatch(new Action("")).Wait();

            // Assert
            Assert.That(count, Is.EqualTo(6));
        }

        [Test]
        public async Task EpicMiddleware_EpicAdded_ExecuteAsync()
        {
            // Arrange
            var count = 0;
            var epic = MockEpic.Create(() => count++);
            var epic2 = MockEpic.Create(() => count += 2);
            var epicMiddleware = EpicMiddleware.Default<MockState>();
            var store = new MiddlewareStore();
            store.CreateSlice("app", new MockState(), _ => { });
            var dispatch = epicMiddleware.Create().Invoke(store)((_, _) => Task.CompletedTask);
            epicMiddleware.Run(Epic.Epic.Combine(epic, epic2));

            // Act
            await dispatch(new Action(""));
            await dispatch(new Action(""));

            // Assert
            Assert.That(count, Is.EqualTo(6));
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
            var dispatch = epicMiddleware.Create().Invoke(store)((_, _) => Task.CompletedTask);
            epicMiddleware.Run(epic);
            // Act
            dispatch(new Action(""));
            dispatch(new Action(""));
            //await Task.Delay(1000);
            // Assert
            Assert.That(count, Is.EqualTo(4));
        }

        [Test]
        public void EpicMiddleware_EpicNoReturnObservable_ThrowInvalidException()
        {
            // Arrange
            var epic = MockEpic.CreateReturnNull();
            var epicMiddleware = EpicMiddleware.Default<MockState>();
            var store = new MiddlewareStore();
            store.CreateSlice("app", new MockState(), _ => { });
            var dispatch = epicMiddleware.Create().Invoke(store)((_, _) => Task.CompletedTask);
            epicMiddleware.Run(epic);
            // Act
            // Assert
            LogAssert.Expect(LogType.Exception, "Exception: Epic must return an observable.");
            dispatch(new Action(""));
        }
    }
}