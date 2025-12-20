namespace Application.Services.Contracts
{
    public interface ICurrentUserService
    {
        public Task<int> UserId { get; }
    }
}
