using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ProductsApi.DTOs;

namespace ProductsApi.Tests.IntegrationTests;

public class ProductsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly string _authToken;
    
    public ProductsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        
        var tokenResponse = _client.PostAsync("/get-token", null).Result;
        var content = tokenResponse.Content.ReadAsStringAsync().Result;
        var token = JsonSerializer.Deserialize<TokenResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        _authToken = token?.Token ?? string.Empty;
    }
    
    [Fact]
    public async Task GetToken_ReturnsToken()
    {
        // Act
        var response = await _client.PostAsync("/get-token", null);
        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(tokenResponse);
        Assert.NotEmpty(tokenResponse!.Token);
    }
    
    [Fact]
    public async Task CreateProduct_WithoutAuthentication_Returns401()
    {
        // Arrange
        var product = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 19.99m,
            Colour = "Red"
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/products", product);
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateProduct_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var product = new CreateProductDto
        {
            Name = "", 
            Description = "AB", 
            Price = -10, 
            Colour = ""
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/products");
        request.Headers.Add("Authorization", $"Bearer {_authToken}");
        request.Content = JsonContent.Create(product);
        
        // Act
        var response = await _client.SendAsync(request);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Name", content);
        Assert.Contains("Description", content);
        Assert.Contains("Price", content);
        Assert.Contains("Colour", content);
    }
    
    [Fact]
    public async Task GetProducts_WithoutAuthentication_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/products");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    private class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
    }
} 
