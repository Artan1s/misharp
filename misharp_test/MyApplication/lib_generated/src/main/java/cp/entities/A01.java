package cp.entities;

public class A01 {
    
    
    
    public static String addTabExt(final String str, final int n) {    
        if (str == null) {
            throw new IllegalArgumentException("str");
        }
        if (n > 1) {
            return "\t"  + cp.entities.A01.addTabExt(str, n - 1);
        } else {
            return "\t"  + str;
        }
    }
    
    public static String addTab(final String str) {    
        if (str == null) {
            throw new IllegalArgumentException("str");
        }
        return "\t"  + str;
    }
    
}