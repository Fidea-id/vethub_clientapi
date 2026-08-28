using Application.Services.Contracts;
using Domain.Entities.Responses.Masters;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Application.Services.Implementations
{
    public class MasterSessionVersionValidator : ISessionVersionValidator
    {
        private readonly HttpClient _httpClient;
        private readonly string _masterApiUri;

        public MasterSessionVersionValidator(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _masterApiUri = configuration["MyAppSettings:Environment"] switch
            {
                "LOCAL" => "https://localhost:44382/api/",
                "STAGING" => "https://mastersg.vethub.id/api/",
                "PRODUCTION" => "https://master.vethub.id/api/",
                _ => throw new InvalidOperationException("Environment invalid")
            };
        }

        public async Task<bool> IsValidAsync(int userId, int sessionVersion, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"{_masterApiUri}Auth/Session/Version/{userId}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var currentSession = await response.Content.ReadFromJsonAsync<SessionVersionResponse>(cancellationToken: cancellationToken);
            return currentSession?.SessionVersion == sessionVersion;
        }
    }
}
