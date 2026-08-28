namespace Application.Services.Contracts
{
    public interface ISessionVersionValidator
    {
        Task<bool> IsValidAsync(int userId, int sessionVersion, CancellationToken cancellationToken = default);
    }
}
