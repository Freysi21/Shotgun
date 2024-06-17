using System;

[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public class DefaultSortPropertyAttribute : Attribute
{
    public DefaultSortPropertyAttribute()
    {
    }
}