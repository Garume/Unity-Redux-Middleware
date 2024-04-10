using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnityReduxMiddleware.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public struct SimpleListCore<T>
    {
        private const int DefaultCapacity = 8;
        private T[] _values;
        private int _lastIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T element)
        {
            if (_values == null)
                _values = new T[DefaultCapacity];
            else if (_lastIndex == _values.Length) Array.Resize(ref _values, _lastIndex * 2);

            _values[_lastIndex++] = element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (_values == null) return;

            _values = null;
            _lastIndex = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsSpan()
        {
            return _values == null ? ReadOnlySpan<T>.Empty : _values.AsSpan(0, _lastIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> AsMemory()
        {
            return _values?.AsMemory(0, _lastIndex) ?? ReadOnlyMemory<T>.Empty;
        }
    }
}