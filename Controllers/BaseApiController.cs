using DecisionMaker.Dtos.Response;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace DecisionMaker.Controllers
{
    public class BaseApiController : ControllerBase
    {
        protected IActionResult ValidationErrorResponse<T>()
        {
            var errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList();
            var response = ApiResponse<T>.Fail(errors, ErrorType.Validation, "Validation failed");
            return BadRequest(response);
        }
    }
}
