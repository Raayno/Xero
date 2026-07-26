namespace AxonGenesis
{
    public interface IPropertyAccessor
    {
        T GetValue<T>();
        void SetValue<T>(T value);
    }
}