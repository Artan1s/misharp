package cp.entities;

public class A01 {
    
    
    
    public String addTabExt(final String str) {    
        if (str == null) {
            throw new IllegalArgumentException("str");
        }
        return "\t"  + str;
    }
    
    public String addTab(final String str) {    
        if (str == null) {
            throw new IllegalArgumentException("str");
        }
        return "\t"  + str;
    }
    
}