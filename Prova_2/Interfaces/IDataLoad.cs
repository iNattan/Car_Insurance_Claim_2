using Prova_2.Model;

namespace Prova_2.Interfaces;
public interface IDataLoad
{
    List<DriverData> Search();
    List<T> Load<T>(string local);
}