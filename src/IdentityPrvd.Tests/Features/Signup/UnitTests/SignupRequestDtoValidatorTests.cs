using FluentAssertions;
using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Features.Authentication.Signup.Dtos.Validators;
using IdentityPrvd.Options;
using NSubstitute;
using Xunit;

namespace IdentityPrvd.Tests.Features.Signup.UnitTests;

public class SignupRequestDtoValidatorTests
{
    private readonly SignupRequestDtoValidator _sut;
    private readonly IdentityPrvdOptions _options;
    private readonly IUsersQuery _usersQuery;

    public SignupRequestDtoValidatorTests()
    {
        _options = Substitute.For<IdentityPrvdOptions>();
        _options.User = new UserOptions();
        _options.Password = new PasswordOptions();
        _usersQuery = Substitute.For<IUsersQuery>();
        _sut = new(_usersQuery, _options);
    }

    [Fact]
    public async Task Validate_WhenLoginIsInvalidEmail_ShouldThrowBadRequestException()
    {
        // Arrange
        _options.User.LoginType = LoginType.Email;
        _usersQuery.IsExistUserByLoginAsync(Arg.Any<string>()).Returns(false);
        _usersQuery.IsExistUserByUserNameAsync(Arg.Any<string>()).Returns(false);
        
        var signupDto = SignupBuilder.NewDefaultBuilder().With(s =>
        {
            s.UserName = "testuser";
            s.FirstName = "Test";
            s.LastName = "User";
            s.Login = "invalid-email";
            s.Password = "ValidPassword123!";
        }).Build();

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(signupDto);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.Message.Should().Be("Login must be a valid email address");
    }

    [Fact]
    public async Task Validate_WhenLoginIsValidEmail_ShouldNotThrowException()
    {
        // Arrange
        _options.User.LoginType = LoginType.Email;
        _usersQuery.IsExistUserByLoginAsync(Arg.Any<string>()).Returns(false);
        _usersQuery.IsExistUserByUserNameAsync(Arg.Any<string>()).Returns(false);
        
        var signupDto = SignupBuilder.NewDefaultBuilder().With(s =>
        {
            s.UserName = "testuser";
            s.FirstName = "Test";
            s.LastName = "User";
            s.Login = "test@example.com";
            s.Password = "ValidPassword123!";
        }).Build();

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(signupDto);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Validate_WhenLoginIsInvalidPhone_ShouldThrowBadRequestException()
    {
        // Arrange
        _options.User.LoginType = LoginType.Phone;
        _usersQuery.IsExistUserByLoginAsync(Arg.Any<string>()).Returns(false);
        _usersQuery.IsExistUserByUserNameAsync(Arg.Any<string>()).Returns(false);
        
        var signupDto = SignupBuilder.NewDefaultBuilder().With(s =>
        {
            s.UserName = "testuser";
            s.FirstName = "Test";
            s.LastName = "User";
            s.Login = "uywgf@mail.com";
            s.Password = "OUGF87h394g812345!";
        }).Build();

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(signupDto);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.Message.Should().Be("Login must be a valid phone number");
    }

    [Fact]
    public async Task Validate_WhenLoginIsValidPhone_ShouldNotThrowException()
    {
        // Arrange
        _options.User.LoginType = LoginType.Phone;
        _usersQuery.IsExistUserByLoginAsync(Arg.Any<string>()).Returns(false);
        _usersQuery.IsExistUserByUserNameAsync(Arg.Any<string>()).Returns(false);
        
        var signupDto = SignupBuilder.NewDefaultBuilder().With(s =>
        {
            s.UserName = "testuser";
            s.FirstName = "Test";
            s.LastName = "User";
            s.Login = "+380501234567";
            s.Password = "ValidPassword123!";
        }).Build();

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(signupDto);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Validate_WhenLoginIsEmpty_ShouldThrowBadRequestException()
    {
        // Arrange
        _options.User.LoginType = LoginType.Any;
        _usersQuery.IsExistUserByLoginAsync(Arg.Any<string>()).Returns(false);
        _usersQuery.IsExistUserByUserNameAsync(Arg.Any<string>()).Returns(false);
        
        var signupDto = SignupBuilder.NewDefaultBuilder().With(s =>
        {
            s.UserName = "testuser";
            s.FirstName = "Test";
            s.LastName = "User";
            s.Login = "";
            s.Password = "ValidPassword123!";
        }).Build();

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(signupDto);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.Message.Should().Be("Login is required");
    }

    [Fact]
    public async Task Validate_WhenLoginIsLessThan4Characters_ShouldThrowBadRequestException()
    {
        // Arrange
        _options.User.LoginType = LoginType.Any;
        _usersQuery.IsExistUserByLoginAsync(Arg.Any<string>()).Returns(false);
        _usersQuery.IsExistUserByUserNameAsync(Arg.Any<string>()).Returns(false);
        
        var signupDto = SignupBuilder.NewDefaultBuilder().With(s =>
        {
            s.UserName = "testuser";
            s.FirstName = "Test";
            s.LastName = "User";
            s.Login = "abc";
            s.Password = "ValidPassword123!";
        }).Build();

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(signupDto);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.Message.Should().Be("Login must be at least 4 characters long");
    }

    [Fact]
    public async Task Validate_WhenLoginIsValid_ShouldNotThrowException()
    {
        // Arrange
        _options.User.LoginType = LoginType.Any;
        _usersQuery.IsExistUserByLoginAsync(Arg.Any<string>()).Returns(false);
        _usersQuery.IsExistUserByUserNameAsync(Arg.Any<string>()).Returns(false);
        
        var signupDto = SignupBuilder.NewDefaultBuilder().With(s =>
        {
            s.UserName = "testuser";
            s.FirstName = "Test";
            s.LastName = "User";
            s.Login = "validlogin";
            s.Password = "ValidPassword123!";
        }).Build();

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(signupDto);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Validate_WhenUserWithLoginAlreadyExists_ShouldThrowBadRequestException()
    {
        // Arrange
        _options.User.LoginType = LoginType.Email;
        _usersQuery.IsExistUserByLoginAsync(Arg.Any<string>()).Returns(true);
        _usersQuery.IsExistUserByUserNameAsync(Arg.Any<string>()).Returns(false);
        
        var signupDto = SignupBuilder.NewDefaultBuilder().With(s =>
        {
            s.UserName = "testuser";
            s.FirstName = "Test";
            s.LastName = "User";
            s.Login = "existing@example.com";
            s.Password = "ValidPassword123!";
        }).Build();

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(signupDto);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.Message.Should().Be("User with this login already exists");
    }

    [Fact]
    public async Task Validate_WhenUserWithUserNameAlreadyExists_ShouldThrowBadRequestException()
    {
        // Arrange
        _options.User.LoginType = LoginType.Email;
        _usersQuery.IsExistUserByLoginAsync(Arg.Any<string>()).Returns(false);
        _usersQuery.IsExistUserByUserNameAsync(Arg.Any<string>()).Returns(true);
        
        var signupDto = SignupBuilder.NewDefaultBuilder().With(s =>
        {
            s.UserName = "existingusername";
            s.FirstName = "Test";
            s.LastName = "User";
            s.Login = "newuser@example.com";
            s.Password = "ValidPassword123!";
        }).Build();

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(signupDto);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.Message.Should().Be("User with this userName already exists");
    }

    [Fact]
    public async Task Validate_WhenPasswordIsEmpty_ShouldThrowValidationException()
    {
        // Arrange
        _options.User.LoginType = LoginType.Email;
        _usersQuery.IsExistUserByLoginAsync(Arg.Any<string>()).Returns(false);
        _usersQuery.IsExistUserByUserNameAsync(Arg.Any<string>()).Returns(false);
        
        var signupDto = SignupBuilder.NewDefaultBuilder().With(s =>
        {
            s.UserName = "testuser";
            s.FirstName = "Test";
            s.LastName = "User";
            s.Login = "test@example.com";
            s.Password = "";
        }).Build();

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(signupDto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Validate_WhenPasswordIsLessThan6Characters_ShouldThrowValidationException()
    {
        // Arrange
        _options.User.LoginType = LoginType.Email;
        _usersQuery.IsExistUserByLoginAsync(Arg.Any<string>()).Returns(false);
        _usersQuery.IsExistUserByUserNameAsync(Arg.Any<string>()).Returns(false);
        
        var signupDto = SignupBuilder.NewDefaultBuilder().With(s =>
        {
            s.UserName = "testuser";
            s.FirstName = "Test";
            s.LastName = "User";
            s.Login = "test@example.com";
            s.Password = "12345";
        }).Build();

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(signupDto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Validate_WhenPasswordIsValid_ShouldNotThrowException()
    {
        // Arrange
        _options.User.LoginType = LoginType.Email;
        _usersQuery.IsExistUserByLoginAsync(Arg.Any<string>()).Returns(false);
        _usersQuery.IsExistUserByUserNameAsync(Arg.Any<string>()).Returns(false);
        
        var signupDto = SignupBuilder.NewDefaultBuilder().With(s =>
        {
            s.UserName = "testuser";
            s.FirstName = "Test";
            s.LastName = "User";
            s.Login = "test@example.com";
            s.Password = "ValidPassword123!";
        }).Build();

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(signupDto);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Validate_WhenPasswordDoesNotMatchRegex_ShouldThrowBadRequestException()
    {
        // Arrange
        _options.User.LoginType = LoginType.Email;
        _options.Password.Regex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
        _options.Password.RegexErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character";
        _usersQuery.IsExistUserByLoginAsync(Arg.Any<string>()).Returns(false);
        _usersQuery.IsExistUserByUserNameAsync(Arg.Any<string>()).Returns(false);
        
        var signupDto = SignupBuilder.NewDefaultBuilder().With(s =>
        {
            s.UserName = "testuser";
            s.FirstName = "Test";
            s.LastName = "User";
            s.Login = "test@example.com";
            s.Password = "simplepassword";
        }).Build();

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(signupDto);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.Message.Should().Be(_options.Password.RegexErrorMessage);
    }

    [Fact]
    public async Task Validate_WhenPasswordMatchesRegex_ShouldNotThrowException()
    {
        // Arrange
        _options.User.LoginType = LoginType.Email;
        _options.Password.Regex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
        _options.Password.RegexErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character";
        _usersQuery.IsExistUserByLoginAsync(Arg.Any<string>()).Returns(false);
        _usersQuery.IsExistUserByUserNameAsync(Arg.Any<string>()).Returns(false);
        
        var signupDto = SignupBuilder.NewDefaultBuilder().With(s =>
        {
            s.UserName = "testuser";
            s.FirstName = "Test";
            s.LastName = "User";
            s.Login = "test@example.com";
            s.Password = "ValidPassword123!";
        }).Build();

        // Act
        var act = async () => await _sut.ValidateAndThrowAsync(signupDto);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
