package cp;

/**
 * Created by Mikhail on 18.05.2015.
 */
public interface ActionT<T> {
    void invoke(T arg);
}
