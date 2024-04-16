using UnityReduxMiddleware;

namespace Sandbox.ApiMock
{
    public interface IStoreService
    {
        public MiddlewareStore Store { get; }
    }
}