using System.Net.Http.Headers;
using IdentityModel.Client;

using var identityClient = new HttpClient()
{
    BaseAddress = new Uri(Environment.GetEnvironmentVariable("AUTHENTICATION__AUTHORITY")!)
};

var discoveryDocument = await identityClient.GetDiscoveryDocumentAsync();

Console.WriteLine(discoveryDocument.TokenEndpoint);

var tokenResponse = await identityClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest()
{
    Address = discoveryDocument.TokenEndpoint,
    ClientId = Environment.GetEnvironmentVariable("AUTHENTICATION__CLIENTID")!,
    ClientSecret = Environment.GetEnvironmentVariable("AUTHENTICATION__CLIENTSECRET")!,
    Scope = "https://www.example.com/api"
});

Console.WriteLine(tokenResponse.AccessToken);

using var httpClient = new HttpClient()
{
    DefaultRequestHeaders =
    {
        Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken)
    }
};

Console.WriteLine(await httpClient.GetStringAsync("https://api:7001/WeatherForecast"));