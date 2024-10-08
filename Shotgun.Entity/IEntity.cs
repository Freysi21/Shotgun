namespace Shotgun.Entity
{
    public abstract class IEntity<T>
    {
        public abstract T Id { get; set; }
    }
}