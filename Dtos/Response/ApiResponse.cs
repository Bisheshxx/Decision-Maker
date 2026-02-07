namespace DecisionMaker.Dtos.Response;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public IEnumerable<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T? data, string message = "") => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public static ApiResponse<T> Fail(IEnumerable<string> errors, string message = "") => new()
    {
        Success = false,
        Message = message,
        Errors = errors
    };
    public static ApiResponse<T> Fail(string error, string message = "")
    => new() { Success = false, Errors = [error], Message = message };
}