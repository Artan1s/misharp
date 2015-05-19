using System;

namespace CP
{
    public class Optional<T>
    {
        public Optional()
        {
            
        }

        public Optional(T value)
        {
            HasValue = true;
            Value = value;
            int valA = 9;
            Func<int, int> f = (i) => { return 2; };
            int valB = f(2);
            int valC = 4;
        }

        public bool HasValue { get; private set; }

        public T Value { get; private set; }
    }
}