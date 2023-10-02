using System;

namespace OrbitPOInts.Utils
{
    public struct Option<T>
    {
        private readonly T _value;
        private readonly bool _hasValue;

        public Option(T value)
        {
            _value = value;
            _hasValue = true;
        }

        public bool IsSome => _hasValue;
        public bool IsNone => !_hasValue;

        public T Value
        {
            get
            {
                if (!_hasValue)
                    throw new InvalidOperationException("Option does not have a value.");
                return _value;
            }
        }

        public static Option<T> None => new();

        public static implicit operator Option<T>(T value) => new(value);
    }
}
