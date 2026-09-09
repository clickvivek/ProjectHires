namespace DataAccessLayer.Common
{
    public interface IOutParam<T>
    {
        T Value { get; }
    }
}
