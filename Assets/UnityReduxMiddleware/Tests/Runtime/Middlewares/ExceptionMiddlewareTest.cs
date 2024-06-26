﻿#if UNITYREDUXMIDDLEWARETEST_UNITASK_INTEGRATION
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
using System;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityReduxMiddleware.Middlewares;
using Action = Unity.AppUI.Redux.Action;

namespace UnityReduxMiddleware.Tests.Runtime.Middlewares
{
    [TestFixture]
    public class ExceptionMiddlewareTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _store = new MiddlewareStore();
        }

        private MiddlewareStore _store;

        [Test]
        public void CreateMiddleware_IsNotNull()
        {
            // Arrange
            // Act
            var middleware = ExceptionMiddleware.Create();
            // Assert
            Assert.IsNotNull(middleware);
        }

        [Test]
        public void CreateMiddleware_WhenCalled_Pass()
        {
            // Arrange
            // Act
            var middleware = ExceptionMiddleware.Create();
#if UNITYREDUXMIDDLEWARETEST_UNITASK_INTEGRATION
            var dispatch = middleware(_store)((_, _) => UniTask.CompletedTask);
#else
            var dispatch = middleware(_store)((_, _) => Task.CompletedTask);
#endif
            // Assert
            dispatch(new Action(""));
            Assert.Pass();
        }

        [Test]
        public void CreateMiddleware_WhenCalledWithException_Throw()
        {
            // Arrange
            // Act
            var middleware = ExceptionMiddleware.Create();
            var dispatch = middleware(_store)((_, _) => throw new Exception());
            // Assert
            LogAssert.ignoreFailingMessages = true;
            Assert.ThrowsAsync<Exception>(async () => await dispatch(new Action("")));
        }

        [Test]
        public void CreateMiddleware_WhenCalledWithOperationCanceledException_Pass()
        {
            // Arrange
            // Act
            var middleware = ExceptionMiddleware.Create();
            var dispatch = middleware(_store)((_, _) => throw new OperationCanceledException());
            // Assert
            dispatch(new Action(""));
            Assert.Pass();
        }
    }
}