using System.Net.Http.Headers;
using System.Security.Claims;
using DecisionMaker.Dtos.Decision;
using DecisionMaker.Dtos.DecisionItem;
using DecisionMaker.Helpers;
using DecisionMaker.Interfaces.Decision;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace DecisionMaker.Controllers;

[Authorize]
[Route("api/decisions")]
[ApiController]
public class DecisionController : BaseApiController
{
    private readonly IDecisionService _decisionServices;

    public DecisionController(IDecisionService decisionServices)
    {
        _decisionServices = decisionServices;
    }


    [HttpPost]

    public async Task<IActionResult> PostAsync(CreateDecisionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationErrorResponse<CreateDecisionDto>();
        }
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var results = await _decisionServices.PostDecisionAsync(userId, dto);

        return results.ToIActionResult(this);
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync(int page = 1, int pageSize = 10, string searchTerm = "")
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var results = await _decisionServices.GetDecisionsAsync(userId, page, pageSize, searchTerm);
        return results.ToIActionResult(this);
    }
    [HttpGet("/api/decision/{decisionId}")]
    public async Task<IActionResult> GetByIdAsync(int decisionId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var results = await _decisionServices.GetDecisionById(userId, decisionId);
        return results.ToIActionResult(this);
    }

    [HttpDelete("/api/decision/{decisionId}")]
    public async Task<IActionResult> DeleteAsync(int decisionId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var results = await _decisionServices.DeleteDecisionAsync(userId, decisionId);
        return results.ToIActionResult(this);

    }

    [HttpPut("/api/decision/{decisionId}")]
    public async Task<IActionResult> UpdateAsync(int decisionId, CreateDecisionDto updateDecisionDto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationErrorResponse<CreateDecisionDto>();
        }
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var results = await _decisionServices.UpdateDecisionAsync(userId, decisionId, updateDecisionDto);
        return results.ToIActionResult(this);
    }

    [HttpGet("/api/decision/{id}/decision-items")]
    public async Task<IActionResult> GetDecisionAsync(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var results = await _decisionServices.GetDecisionItemAsync(userId, id);
        return results.ToIActionResult(this);

    }

    [HttpPost("/api/decision/{id}/decision-item")]
    public async Task<IActionResult> PostDecisionItemsAsync(CreateDecisionItemDto createDecisionItemDto, int id)
    {
        if (!ModelState.IsValid)
        {
            return ValidationErrorResponse<CreateDecisionDto>();
        }
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var result = await _decisionServices.PostDecisionItemAsync(createDecisionItemDto, userId, id);
        return result.ToIActionResult(this);
    }


    [HttpPut("/api/decision/{id}/decision-items")]
    public async Task<IActionResult> UpsertMultipleDecisionItemsAsync([FromBody] List<UpsertDecisionItemDto> upsertDecisionItemDto, int id)
    {
        if (!ModelState.IsValid)
        {
            return ValidationErrorResponse<CreateDecisionDto>();
        }
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var result = await _decisionServices.UpsertDecisionItemAsync(upsertDecisionItemDto, userId, id);
        // var result = await _decisionServices.PostDecisionItemAsync(createDecisionItemDto, userId, id);
        return result.ToIActionResult(this);
    }

    [HttpPut("/api/decision/{id}/decision-item/{itemId}")]
    public async Task<IActionResult> UpdateDecisionItemAsync(UpdateDecisionItemDto updateDecisionItemDto, int id, int itemId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var result = await _decisionServices.UpdateDecisionItemAsync(updateDecisionItemDto, userId, id, itemId);
        return result.ToIActionResult(this);
    }
    [HttpDelete("/api/decision/{id}/decision-item/{itemId}")]
    public async Task<IActionResult> DeleteDecisionItemAsync(int id, int itemId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var result = await _decisionServices.DeleteDecisionItemAsync(id, itemId, userId);
        return result.ToIActionResult(this);
    }

}