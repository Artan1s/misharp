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
        }

        public bool HasValue { get; private set; }

        public T Value { get; private set; }
    }
}
