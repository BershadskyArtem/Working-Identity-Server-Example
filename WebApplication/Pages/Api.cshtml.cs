using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication.Pages;

[Authorize]
public class ApiModel : PageModel
{
    private IHttpClientFactory _httpClientFactory;

    public ApiModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public string? Data { get; set; } 
    
    public async Task OnGet()
    {
        using var httpClient = _httpClientFactory.CreateClient();

        httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", await HttpContext.GetTokenAsync("access_token"));

        Data = await httpClient.GetStringAsync("https://api:7001/WeatherForecast");
    }
}