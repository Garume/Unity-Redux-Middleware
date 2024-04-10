using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityReduxMiddleware.Tests.Runtime.Utility;
using Action = Unity.AppUI.Redux.Action;

namespace UnityReduxMiddleware.Tests.Runtime
{
    [TestFixture]
    public class MiddlewareStoreTest
    {
        [Test]
        public async Task MiddlewareStore_WhenMiddlewareAdded_ExecutedOnce()
        {
            // Arrange
            var store = new MiddlewareStore();
            var count = 0;
            var incrementMiddleware = MockMiddleware.Create(() => count++);
            store.AddMiddleware(incrementMiddleware);
            // Act
            await store.DispatchAsync(new Action(""));
            // Assert
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void MiddlewareStore_DispatchedDuringMiddlewareSetup_ThrowArgumentException()
        {
            // Arrange
            var store = new MiddlewareStore();
            var incrementMiddleware = MockMiddleware.CreateDispatchDuringSetup();
            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => store.AddMiddleware(incrementMiddleware));
        }

        [Test]
        public async Task MiddlewareStore_MultipleMiddleware_Pass()
        {
            // Arrange
            var store = new MiddlewareStore();
            var count1 = 0;
            var count2 = 0;
            var incrementMiddleware1 = MockMiddleware.Create(() => count1++);
            var incrementMiddleware2 = MockMiddleware.Create(() => count2++);
            store.AddMiddleware(incrementMiddleware1);
            store.AddMiddleware(incrementMiddleware2);
            // Act
            await store.DispatchAsync(new Action(""));
            // Assert
            Assert.That(count1, Is.EqualTo(1));
            Assert.That(count2, Is.EqualTo(1));
        }

        [Test]
        public async Task MiddlewareStore_MiddlewareExecution_AddedInOrder()
        {
            // Arrange
            var store = new MiddlewareStore();
            var executionSequence = new List<int>();
            var incrementMiddleware1 = MockMiddleware.Create(() => executionSequence.Add(1));
            var incrementMiddleware2 = MockMiddleware.Create(() => executionSequence.Add(2));

            // Adding middleware in the order they should be executed
            store.AddMiddleware(incrementMiddleware1);
            store.AddMiddleware(incrementMiddleware2);

            // Act
            await store.DispatchAsync(new Action(""));

            // Assert
            Assert.That(executionSequence, Is.EqualTo(new List<int> { 1, 2 }),
                "Middleware should execute in the order they were added.");
        }

        [Test]
        public async Task MiddlewareStore_ConcurrentDispatches_HandleCorrectly()
        {
            var store = new MiddlewareStore();
            var actionCount = 0;
            var syncLock = new object();
            var middleware = MockMiddleware.Create(() =>
            {
                lock (syncLock)
                {
                    actionCount++;
                }
            });

            store.AddMiddleware(middleware);

            var tasks = Enumerable.Range(0, 100)
                .Select(_ => store.DispatchAsync(new Action("")))
                .ToArray();
            await Task.WhenAll(tasks);

            Assert.That(actionCount, Is.EqualTo(100));
        }
    }
}