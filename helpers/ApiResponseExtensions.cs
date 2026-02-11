using DecisionMaker.Dtos.Response;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DecisionMaker.Helpers;

public static class ApiResponseExtension
{
    public static IActionResult ToIActionResult<T>(this ApiResponse<T> response, ControllerBase controller)
    {
        if (response.Success)
        {
            return controller.Ok(response);
        }

        return response.ErrorType switch
        {
            ErrorType.Validation => controller.BadRequest(response),
            ErrorType.Unauthorized => controller.Unauthorized(response),
            ErrorType.NotFound => controller.NotFound(response),
            ErrorType.Conflict => controller.Conflict(response),
            ErrorType.Forbidden => controller.Forbid(),
            _ => controller.StatusCode(500, response)
        };
    }
}

