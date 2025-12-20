namespace Application.Services.Contracts
{
    public interface ICacheService
    {
        Task SetAsync<T>(string tenantId, string key, T value, TimeSpan? expiry = null);
        Task<T?> GetAsync<T>(string tenantId, string key);
        Task RemoveAsync(string tenantId, string key);
    }
}
