using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SmartQueue.Application.Auth.Commands.Register;

namespace SmartQueue.Test;

public class TokenTest: IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;

    public TokenTest(IntegrationTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsToken()
    {
        // arrange
        var command = new RegisterCommand("test@test.com", "password123");

        // act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", command);
        var result = await response.Content.ReadFromJsonAsync<AuthResult>();

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithInvalidEmail_Returns400()
    {
        // arrange
        var command = new RegisterCommand("notanemail", "password123");

        // act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", command);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        // arrange
        var command = new RegisterCommand("test@test.com", "password123");

        // act
        await _client.PostAsJsonAsync("/api/v1/auth/register", command);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", command);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}

public record AuthResult(Guid UserId, string Email, string Role, string Token);