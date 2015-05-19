package cp.entities;

public class A1Test {
    
    
    
    public void ifTest(final boolean condition, final cp.Action trueAction, final cp.Action falseAction) {    
        if (trueAction == null) {
            throw new IllegalArgumentException("trueAction");
        }
        if (falseAction == null) {
            throw new IllegalArgumentException("falseAction");
        }
        int valI = 3;
        if (condition && valI > 3) {
            final String str = "privet";
            final String str2 = cp.entities.A01.addTab(str);
            final String str3 = cp.entities.A01.addTabExt(str);
            final cp.entities.EntityB b = new cp.entities.EntityB();
            final cp.entities.EntityA a = b.getEntityAProperty();
            b.getEntityAProperty().m();
            final cp.entities.EntityB bb = b.getEntityAProperty().getEntityBProperty().getEntityAProperty().getEntityBProperty();
            a.m();
            final int c = a.getDoubleFunc().invoke(2);
            final cp.Seq<Integer> list = new cp.Seq<Integer>();
            final int k = list.get(2);
            list.set(k, 3);
            list.add(1);
        } else if (valI > 10) {
            final int k = 4;
        } else {
            falseAction.invoke();
        }
    }
    
}