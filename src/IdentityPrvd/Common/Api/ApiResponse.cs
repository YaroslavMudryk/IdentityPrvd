namespace IdentityPrvd.Common.Api;

public class ApiResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public Dictionary<string, string[]> ValidationErrors { get; set; } = [];
    public string ErrorId { get; set; }

    public ApiResponse()
    {

    }

    public ApiResponse(bool isSuccess, string message, Dictionary<string, string[]> validationErrors, string errorId)
    {
        IsSuccess = isSuccess;
        Message = message;
        ValidationErrors = validationErrors;
        ErrorId = errorId;
    }

    public static ApiResponse Ok()
    {
        return new ApiResponse(true, default!, default!, default!);
    }

    public static ApiResponse Fail(string message)
    {
        return new ApiResponse(false, message, default!, default!);
    }

    public static ApiResponse ValidationFail(Dictionary<string, string[]> validationErrors)
    {
        return new ApiResponse(false, "Validation errors", validationErrors, default!);
    }
}

public class ApiResponse<T> : ApiResponse
{
    public T Data { get; set; }

    public static ApiResponse Ok(T data)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Data = data,
            ErrorId = default!,
            Message = default!,
            ValidationErrors = []
        };
    }
}

public static class ApiResponseExtensions
{
    public static ApiResponse MapToResponse<T>(this T data)
    {
        return ApiResponse<T>.Ok(data);
    }
}
