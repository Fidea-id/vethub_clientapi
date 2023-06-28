namespace Application.Services.Contracts
{
    public interface IMasterService
    {
        Task GenerateTables(string dbName);
        Task GenerateTableField(string dbName, Dictionary<string, object> fields);
        Task CheckTableColumn(string dbName, Dictionary<string, object> fields);
    }
}
