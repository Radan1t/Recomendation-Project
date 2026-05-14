using ApiGateway.Config;
using Microsoft.Extensions.Options;
using Shared.DTO.Auth;

namespace ApiGateway.Services.Auth;

public class AuthServiceClient : IAuthServiceClient
{
    private readonly HttpClient _http;
    private readonly ServiceUrls _urls;

    public AuthServiceClient(HttpClient http, IOptions<ServiceUrls> urls)
    {
        _http = http;
        _urls = urls.Value;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync(
            $"{_urls.AuthService}/auth/login", dto);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
    }

    public async Task RegisterAsync(RegisterRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync(
            $"{_urls.AuthService}/auth/register", dto);

        response.EnsureSuccessStatusCode();
    }
}

