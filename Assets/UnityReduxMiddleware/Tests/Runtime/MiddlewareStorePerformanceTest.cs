using NUnit.Framework;
using Unity.AppUI.Redux;
using Unity.PerformanceTesting;
using UnityReduxMiddleware.Tests.Runtime.Utility;

namespace UnityReduxMiddleware.Tests.Runtime
{
    [TestFixture]
    public class MiddlewareStorePerformanceTest
    {
        [Test]
        [Performance]
        public void Dispatch_Performance_Measurement()
        {
            var store = new MiddlewareStore();
            var middleware = MockMiddleware.Create(() => { });
            store.AddMiddleware(middleware);
            
            Measure.Method(() => { store.Dispatch(new Action("TestAction")); })
                .WarmupCount(5) // 初期のウォームアップ実行回数
                .MeasurementCount(30) // 測定を行う実行回数
                .IterationsPerMeasurement(10) // 1回の測定あたりの繰り返し回数
                .Run();

            PerformanceTest.Active.CalculateStatisticalValues();
        }
    }
}