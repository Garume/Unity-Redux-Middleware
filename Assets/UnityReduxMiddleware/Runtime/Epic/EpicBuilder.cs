using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace UnityReduxMiddleware.Epic
{
    public ref struct EpicBuilder<TState>
    {
        private Epic<TState>[] _epics;
        private int _count;
        private const int InitialCapacity = 8;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Epic<TState> epic)
        {
            if (epic == null)
                throw new ArgumentNullException(nameof(epic));
            if (_count == 0)
            {
                _epics = ArrayPool<Epic<TState>>.Shared.Rent(InitialCapacity);
            }
            else if (_epics.Length == _count)
            {
                var newEpics = ArrayPool<Epic<TState>>.Shared.Rent(_count * 2);
                Array.Copy(_epics, newEpics, _count);
                ArrayPool<Epic<TState>>.Shared.Return(_epics, true);
                _epics = newEpics;
            }

            _epics[_count++] = epic;
        }


        public Epic<TState> Build()
        {
            if (_count == 0)
                throw new InvalidOperationException("No epics added.");
            return Epic.Combine(_epics);
        }
    }

    public ref struct EpicBuilder<TState, TDependencies>
    {
        private Epic<TState, TDependencies>[] _epics;
        private int _count;
        private const int InitialCapacity = 8;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Epic<TState, TDependencies> epic)
        {
            if (epic == null)
                throw new ArgumentNullException(nameof(epic));
            if (_count == 0)
            {
                _epics = ArrayPool<Epic<TState, TDependencies>>.Shared.Rent(InitialCapacity);
            }
            else if (_epics.Length == _count)
            {
                var newEpics = ArrayPool<Epic<TState, TDependencies>>.Shared.Rent(_count * 2);
                Array.Copy(_epics, newEpics, _count);
                ArrayPool<Epic<TState, TDependencies>>.Shared.Return(_epics, true);
                _epics = newEpics;
            }

            _epics[_count++] = epic;
        }

        public Epic<TState, TDependencies> Build()
        {
            if (_count == 0)
                throw new InvalidOperationException("No epics added.");
            return Epic.Combine(_epics);
        }
    }
}