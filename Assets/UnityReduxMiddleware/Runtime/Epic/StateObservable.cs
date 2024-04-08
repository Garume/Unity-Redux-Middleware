using System;
using R3;

namespace UnityReduxMiddleware.Epic
{
    public class StateObservable<T> : Observable<T>
    {
        private readonly Subject<T> _notifySubject = new();

        public StateObservable(Observable<T> input, T initialState)
        {
            Value = initialState;
            input.Where(this, (state, self) => !state.Equals(self.Value))
                .Subscribe(this, (state, self) =>
                {
                    self.Value = state;
                    self._notifySubject.OnNext(state);
                });
        }

        public T Value { get; private set; }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            var subscription = _notifySubject.Subscribe(observer);
            // observer.OnNext(Value);
            return subscription;
        }
    }
}