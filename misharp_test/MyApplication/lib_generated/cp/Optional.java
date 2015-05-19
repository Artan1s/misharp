package cp;

public class Optional<T> {
    
    public Optional() {    
    }
    
    public Optional(final T value) {    
            setHasValue(true);
            setValue(value);
            int valA = 9;
            final cp.FuncT<Integer, Integer> f = new cp.FuncT<Integer, Integer>() {
                        @Override
                        public Integer invoke(final Integer i) {    
                            return 2;
                        }
                        
                        };
            int valB = f.invoke(2);
            int valC = 4;
    }
    
    
    private boolean hasValue;
    
    public boolean getHasValue() {    
        return hasValue;
    }
    
    private void setHasValue(final boolean hasValue) {    
        this.hasValue = hasValue;
    }
    
    
    private T value;
    
    public T getValue() {    
        return value;
    }
    
    private void setValue(final T value) {    
        this.value = value;
    }
    
    
    
}