package cp.entities;

public class EntityA {
    
    
    private int intProperty;
    
    public int getIntProperty() {    
        return intProperty;
    }
    
    public void setIntProperty(final int intProperty) {    
        this.intProperty = intProperty;
    }
    
    
    private int intProperty2;
    
    public int getIntProperty2() {    
        return intProperty2;
    }
    
    private void setIntProperty2(final int intProperty2) {    
        this.intProperty2 = intProperty2;
    }
    
    
    private String stringProperty;
    
    public String getStringProperty() {    
        return stringProperty;
    }
    
    public void setStringProperty(final String stringProperty) {    
        if (stringProperty == null) {
            throw new IllegalArgumentException("stringProperty");
        }
        this.stringProperty = stringProperty;
    }
    
    
    private cp.entities.EntityB entityBProperty;
    
    public cp.entities.EntityB getEntityBProperty() {    
        return entityBProperty;
    }
    
    public void setEntityBProperty(final cp.entities.EntityB entityBProperty) {    
        if (entityBProperty == null) {
            throw new IllegalArgumentException("entityBProperty");
        }
        this.entityBProperty = entityBProperty;
    }
    
    
    private cp.FuncT<Integer, Integer> doubleFunc;
    
    public cp.FuncT<Integer, Integer> getDoubleFunc() {    
        return doubleFunc;
    }
    
    public void setDoubleFunc(final cp.FuncT<Integer, Integer> doubleFunc) {    
        if (doubleFunc == null) {
            throw new IllegalArgumentException("doubleFunc");
        }
        this.doubleFunc = doubleFunc;
    }
    
    
    private cp.Optional<cp.entities.EntityB> optionalEntityBProperty;
    
    public cp.Optional<cp.entities.EntityB> getOptionalEntityBProperty() {    
        return optionalEntityBProperty;
    }
    
    public void setOptionalEntityBProperty(final cp.Optional<cp.entities.EntityB> optionalEntityBProperty) {    
        if (optionalEntityBProperty == null) {
            throw new IllegalArgumentException("optionalEntityBProperty");
        }
        this.optionalEntityBProperty = optionalEntityBProperty;
    }
    
    
    private cp.Seq<cp.entities.EntityB> listEntityBProperty;
    
    public cp.Seq<cp.entities.EntityB> getListEntityBProperty() {    
        return listEntityBProperty;
    }
    
    public void setListEntityBProperty(final cp.Seq<cp.entities.EntityB> listEntityBProperty) {    
        if (listEntityBProperty == null) {
            throw new IllegalArgumentException("listEntityBProperty");
        }
        this.listEntityBProperty = listEntityBProperty;
    }
    
    
    private cp.Seq<Integer> listIntProperty;
    
    public cp.Seq<Integer> getListIntProperty() {    
        return listIntProperty;
    }
    
    public void setListIntProperty(final cp.Seq<Integer> listIntProperty) {    
        if (listIntProperty == null) {
            throw new IllegalArgumentException("listIntProperty");
        }
        this.listIntProperty = listIntProperty;
    }
    
    
    
    public String method1(final int intParam, final cp.Action a1, final cp.ActionT<Integer> a2, final cp.Func<Integer> f1, final cp.FuncT<Integer, Integer> f2) {    
        if (a1 == null) {
            throw new IllegalArgumentException("a1");
        }
        if (a2 == null) {
            throw new IllegalArgumentException("a2");
        }
        if (f1 == null) {
            throw new IllegalArgumentException("f1");
        }
        if (f2 == null) {
            throw new IllegalArgumentException("f2");
        }
        return method1(intParam, new cp.Action() {
                        @Override
                        public void invoke() {    
                            m();
                        }
                        
                        }, new cp.ActionT<Integer>() {
                        @Override
                        public void invoke(final Integer i) {    
                            m();
                        }
                        
                        }, new cp.Func<Integer>() {
                        @Override
                        public Integer invoke() {    
                            return 1;
                        }
                        
                        }, new cp.FuncT<Integer, Integer>() {
                        @Override
                        public Integer invoke(final Integer i) {    
                            final int k = 3;
                            return k;
                        }
                        
                        });
    }
    
    public void m() {    
        final double g = 9.84;
    }
    
}