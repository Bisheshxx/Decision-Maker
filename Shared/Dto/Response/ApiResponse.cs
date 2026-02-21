using System.Text.Json.Serialization;
using DecisionMaker.Shared.Pagination.Dto;

namespace DecisionMaker.Dtos.Response;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<string>? Errors { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ErrorType? ErrorType { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PaginationMeta? Meta { get; set; }

    public static ApiResponse<T> Ok(T? data, string message = "", PaginationMeta? meta = null) => new()
    {
        Success = true,
        Data = data,
        Message = message,
        Errors = null,
        ErrorType = null,
        Meta = meta
    };

    public static ApiResponse<T> Fail(
        string error,
        ErrorType type,
        string message = "")
        => new()
        {
            Success = false,
            Errors = [error],
            ErrorType = type,
            Message = message,
            Data = default
        };

    public static ApiResponse<T> Fail(
        IEnumerable<string> errors,
        ErrorType type,
        string message = "")
        => new()
        {
            Success = false,
            Errors = errors,
            ErrorType = type,
            Message = message,
            Data = default
        };
}


public enum ErrorType
{
    None,
    Validation,
    Unauthorized,
    NotFound,
    Conflict,
    Forbidden,
    ServerError
}