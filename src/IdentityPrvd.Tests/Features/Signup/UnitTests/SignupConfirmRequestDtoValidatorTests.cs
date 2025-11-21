using FluentAssertions;
using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Features.Authentication.Signup.Dtos;
using IdentityPrvd.Features.Authentication.Signup.Dtos.Validators;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Xunit;

namespace IdentityPrvd.Tests.Features.Signup.UnitTests;

public class SignupConfirmRequestDtoValidatorTests
{
    private readonly SignupConfirmRequestDtoValidator _sut;
    private readonly FakeTimeProvider _timeProvider;
    private readonly IConfirmsQuery _confirmsQuery;

    public SignupConfirmRequestDtoValidatorTests()
    {
        _timeProvider = new FakeTimeProvider();
        _confirmsQuery = Substitute.For<IConfirmsQuery>();
        _sut = new(_timeProvider, _confirmsQuery);
    }

    [Fact]
    public async Task Validate_WhenCodeIsEmpty_ShouldThrowValidationException()
    {
        // Arrange
        var dto = new SignupConfirmRequestDto { Code = "" };

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Validate_WhenCodeIsNull_ShouldThrowValidationException()
    {
        // Arrange
        var dto = new SignupConfirmRequestDto { Code = null! };

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Validate_WhenConfirmDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _confirmsQuery.GetConfirmWithUserByCodeAsync(Arg.Any<string>()).Returns((IdentityCode?)null);
        
        var dto = new SignupConfirmRequestDto { Code = "nonexistent-code" };

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(dto);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().ContainEquivalentOf(new
        {
            PropertyName = "Code",
            ErrorMessage = "Confirm not found"
        });
    }

    [Fact]
    public async Task Validate_WhenUserIsAlreadyConfirmed_ShouldThrowBadRequestException()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        _timeProvider.SetUtcNow(utcNow);
        
        var user = new IdentityUser
        {
            Id = Guid.NewGuid(),
            IsConfirmed = true,
            Login = "test@example.com"
        };
        
        var confirm = new IdentityCode
        {
            Code = "valid-code",
            UserId = user.Id,
            User = user,
            ActiveFrom = utcNow.AddMinutes(-10),
            ActiveTo = utcNow.AddMinutes(10),
            IsActivated = false
        };
        
        _confirmsQuery.GetConfirmWithUserByCodeAsync(Arg.Any<string>()).Returns(confirm);
        
        var dto = new SignupConfirmRequestDto { Code = "valid-code" };

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(dto);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().ContainEquivalentOf(new
        {
            PropertyName = "Code",
            ErrorMessage = "User already confirmed"
        });
    }

    [Fact]
    public async Task Validate_WhenConfirmIsAlreadyActivated_ShouldThrowBadRequestException()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        _timeProvider.SetUtcNow(utcNow);
        
        var user = new IdentityUser
        {
            Id = Guid.NewGuid(),
            IsConfirmed = false,
            Login = "test@example.com"
        };
        
        var confirm = new IdentityCode
        {
            Code = "valid-code",
            UserId = user.Id,
            User = user,
            ActiveFrom = utcNow.AddMinutes(-10),
            ActiveTo = utcNow.AddMinutes(10),
            IsActivated = true
        };
        
        _confirmsQuery.GetConfirmWithUserByCodeAsync(Arg.Any<string>()).Returns(confirm);
        
        var dto = new SignupConfirmRequestDto { Code = "valid-code" };

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(dto);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().ContainEquivalentOf(new
        {
            PropertyName = "Code",
            ErrorMessage = "Confirm already activated"
        });
    }

    [Fact]
    public async Task Validate_WhenConfirmActiveFromIsInFuture_ShouldThrowBadRequestException()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        _timeProvider.SetUtcNow(utcNow);
        
        var user = new IdentityUser
        {
            Id = Guid.NewGuid(),
            IsConfirmed = false,
            Login = "test@example.com"
        };
        
        var confirm = new IdentityCode
        {
            Code = "valid-code",
            UserId = user.Id,
            User = user,
            ActiveFrom = utcNow.AddMinutes(10), // Future
            ActiveTo = utcNow.AddMinutes(20),
            IsActivated = false
        };
        
        _confirmsQuery.GetConfirmWithUserByCodeAsync(Arg.Any<string>()).Returns(confirm);
        
        var dto = new SignupConfirmRequestDto { Code = "valid-code" };

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(dto);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().ContainEquivalentOf(new
        {
            PropertyName = "Code",
            ErrorMessage = "Verify out of time"
        });
    }

    [Fact]
    public async Task Validate_WhenConfirmActiveToIsInPast_ShouldThrowBadRequestException()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        _timeProvider.SetUtcNow(utcNow);
        
        var user = new IdentityUser
        {
            Id = Guid.NewGuid(),
            IsConfirmed = false,
            Login = "test@example.com"
        };
        
        var confirm = new IdentityCode
        {
            Code = "valid-code",
            UserId = user.Id,
            User = user,
            ActiveFrom = utcNow.AddMinutes(-20),
            ActiveTo = utcNow.AddMinutes(-10), // Past
            IsActivated = false
        };
        
        _confirmsQuery.GetConfirmWithUserByCodeAsync(Arg.Any<string>()).Returns(confirm);
        
        var dto = new SignupConfirmRequestDto { Code = "valid-code" };

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(dto);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().ContainEquivalentOf(new
        {
            PropertyName = "Code",
            ErrorMessage = "Verify out of time"
        });
    }

    [Fact]
    public async Task Validate_WhenConfirmIsWithinValidTimeRange_ShouldNotThrowException()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        _timeProvider.SetUtcNow(utcNow);
        
        var user = new IdentityUser
        {
            Id = Guid.NewGuid(),
            IsConfirmed = false,
            Login = "test@example.com"
        };
        
        var confirm = new IdentityCode
        {
            Code = "valid-code",
            UserId = user.Id,
            User = user,
            ActiveFrom = utcNow.AddMinutes(-10),
            ActiveTo = utcNow.AddMinutes(10),
            IsActivated = false
        };
        
        _confirmsQuery.GetConfirmWithUserByCodeAsync(Arg.Any<string>()).Returns(confirm);
        
        var dto = new SignupConfirmRequestDto { Code = "valid-code" };

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(dto);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Validate_WhenConfirmIsAtActiveFromBoundary_ShouldNotThrowException()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        _timeProvider.SetUtcNow(utcNow);
        
        var user = new IdentityUser
        {
            Id = Guid.NewGuid(),
            IsConfirmed = false,
            Login = "test@example.com"
        };
        
        var confirm = new IdentityCode
        {
            Code = "valid-code",
            UserId = user.Id,
            User = user,
            ActiveFrom = utcNow, // Exactly now
            ActiveTo = utcNow.AddMinutes(10),
            IsActivated = false
        };
        
        _confirmsQuery.GetConfirmWithUserByCodeAsync(Arg.Any<string>()).Returns(confirm);
        
        var dto = new SignupConfirmRequestDto { Code = "valid-code" };

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(dto);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Validate_WhenConfirmIsAtActiveToBoundary_ShouldNotThrowException()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        _timeProvider.SetUtcNow(utcNow);
        
        var user = new IdentityUser
        {
            Id = Guid.NewGuid(),
            IsConfirmed = false,
            Login = "test@example.com"
        };
        
        var confirm = new IdentityCode
        {
            Code = "valid-code",
            UserId = user.Id,
            User = user,
            ActiveFrom = utcNow.AddMinutes(-10),
            ActiveTo = utcNow, // Exactly now
            IsActivated = false
        };
        
        _confirmsQuery.GetConfirmWithUserByCodeAsync(Arg.Any<string>()).Returns(confirm);
        
        var dto = new SignupConfirmRequestDto { Code = "valid-code" };

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(dto);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
