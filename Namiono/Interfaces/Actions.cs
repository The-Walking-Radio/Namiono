namespace Namiono.Interfaces
{
    public interface IActions
    {
        void Add<T>(Types type, T value);
        void Update<T>(Types type, T value);
        void Remove<T>(Types type, T value);
        void Notify(Actions action, Types type);
    }
}
