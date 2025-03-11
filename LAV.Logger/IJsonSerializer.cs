namespace LAV.Logger
{
    public interface IJsonSerializer
    {
        T Deserialize<T>(string jsonResponse);
        string SerializeObject(object request);

        bool IsJson(string jsonResponse);

        T ConvertTo<T>(object obj);
    }
}