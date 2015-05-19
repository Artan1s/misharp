package cp;

import java.util.ArrayList;

/**
 * Created by Mikhail on 19.05.2015.
 */
public class Seq<T> {
    private ArrayList<T> internal;

    public Seq() {
        internal = new ArrayList<T>();
    }

    public T get(int index) {
        return internal.get(index);
    }

    public void set(int index, T value) {
        internal.set(index, value);
    }

    public void add(T item) {
        internal.add(item);
    }

    public ArrayList<T> toArrayList() {
        return internal;
    }
}
