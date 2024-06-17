using System;

[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public class SingleNavigationPropertyAttribute : Attribute
{
    public SingleNavigationPropertyAttribute()
    {
    }
}