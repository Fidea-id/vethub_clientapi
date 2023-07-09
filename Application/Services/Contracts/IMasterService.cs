namespace Application.Services.Contracts
{
    public interface IMasterService
    {
        Task GenerateTables(string dbName, int? version = null);
        Task GenerateTableField(string dbName);
    }
}
