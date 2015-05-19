package cp;

/**
 * Created by Mikhail on 18.05.2015.
 */
public interface FuncT<TRes, T> {
    TRes invoke(T arg);
}
