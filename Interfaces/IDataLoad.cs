public interface IDataLoad
{
    List<DriverData> Search();
    List<T> Load<T>(string local);
}