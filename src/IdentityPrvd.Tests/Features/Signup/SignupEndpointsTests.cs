using FluentAssertions;
using FluentAssertions.Common;
using FluentAssertions.Extensions;
using IdentityPrvd.Common.Api;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Features.Authentication.Signup.Dtos;
using IdentityPrvd.Tests.IntegrationInfra;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace IdentityPrvd.Tests.Features.Signup;

public class SignupEndpointsTests(ITestOutputHelper output, PostgresTestWithRedisWebApplicationFactory factory)
    : PostgresTestWithRedisIntegrationTestBase(output, factory)
{
    private readonly SignupEndpoints _endpoints = new(factory.Client);

    [Fact]
    public async Task PostSignup_ShouldReturn201WithSignupResponseDto()
    {
        // Arrange
        var dto = SignupBuilder.NewDefaultBuilder().With(p =>
        {
            p.FirstName = Guid.NewGuid().ToString();
            p.LastName = Guid.NewGuid().ToString();
            p.UserName = Guid.NewGuid().ToString("N")[..7];
            p.Login = "testuser@gmail.com";
            p.Password = "TestPassword1234!";
        }).Build();

        // Act
        var apiResponse = await _endpoints.Signup(dto);
        var createdResponse = apiResponse.GetBody<ApiResponse<SignupResponseDto>>();

        // Assert
        apiResponse.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task PostSignup_ShouldCreateUserWithRolesPasswordsAndContactsInDatabase()
    {
        // Arrange
        var dbContext = Factory.CreateDbContext();
        var createdAt = 21.November(2025).At(12, 12, 12).AsUtc();
        Factory.FakeTimeProvider.SetUtcNow(createdAt.ToDateTimeOffset());
        var createdBy = "Api";

        var dto = SignupBuilder.NewDefaultBuilder().With(p =>
        {
            p.FirstName = Guid.NewGuid().ToString();
            p.LastName = Guid.NewGuid().ToString();
            p.UserName = Guid.NewGuid().ToString("N")[..7];
            p.Login = "testuser@gmail.com";
            p.Password = "TestPassword1234!";
        }).Build();

        // Act
        var apiResponse = await _endpoints.Signup(dto);
        var createdResponse = apiResponse.GetBody<ApiResponse<SignupResponseDto>>();

        // Assert
        var createdUser = await dbContext
            .Users.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == createdResponse.Data.UserId.GetIdAsGuid());

        createdUser.Should().NotBeNull();
        createdUser.FirstName.Should().Be(dto.FirstName);
        createdUser.LastName.Should().Be(dto.LastName);
        createdUser.UserName.Should().Be(dto.UserName);
        createdUser.Login.Should().Be(dto.Login);
        createdUser.PasswordHash.Should().NotBe(dto.Password);
        createdUser.CreatedAt.Should().Be(createdAt);
        createdUser.CreatedBy.Should().Be(createdBy);
        createdUser.UpdatedAt.Should().Be(createdAt);
        createdUser.UpdatedBy.Should().Be(createdBy);
        createdUser.IsConfirmed.Should().BeTrue();
        createdUser.ConfirmedAt.Should().Be(createdAt);
        createdUser.ConfirmedBy.Should().Be(createdBy);

        var userRoles = await dbContext.UserRoles.AsNoTracking().Where(s => s.UserId == createdUser.Id).ToListAsync();
        userRoles.Count.Should().Be(1);

        var userPasswords = await dbContext.Passwords.AsNoTracking().Where(s => s.UserId == createdUser.Id).ToListAsync();
        userPasswords.Count.Should().Be(1);
        var userPassword = userPasswords.First();
        userPassword.PasswordHash.Should().Be(createdUser.PasswordHash);
        userPassword.IsActive.Should().BeTrue();
        userPassword.ActivatedAt.Should().Be(createdAt);

        var userContacts = await dbContext.Contacts.AsNoTracking().Where(s => s.UserId == createdUser.Id).ToListAsync();
        userContacts.Count.Should().Be(1);
        var userContact = userContacts.First();
        userContact.Title.Should().Be("Email");
        userContact.IsMain.Should().BeTrue();
        userContact.IsConfirmed.Should().BeTrue();
        userContact.CanBeDeleted.Should().BeFalse();

        var userSessions = await dbContext.Sessions.AsNoTracking().Where(s => s.UserId == createdUser.Id).ToListAsync();
        userSessions.Count.Should().Be(0);
        userSessions.AddRange(userSessions);
    }
}
