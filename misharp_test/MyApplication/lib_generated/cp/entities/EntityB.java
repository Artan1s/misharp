package cp.entities;

public class EntityB {
    
    
    private int intProperty;
    
    public int getIntProperty() {    
        return intProperty;
    }
    
    public void setIntProperty(final int intProperty) {    
        this.intProperty = intProperty;
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
    
    
    private cp.entities.EntityA entityAProperty;
    
    public cp.entities.EntityA getEntityAProperty() {    
        return entityAProperty;
    }
    
    public void setEntityAProperty(final cp.entities.EntityA entityAProperty) {    
        if (entityAProperty == null) {
            throw new IllegalArgumentException("entityAProperty");
        }
        this.entityAProperty = entityAProperty;
    }
    
    
    
}