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
    public async Task<IActionResult> GetAsync(int page = 1, int pageSize = 10)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var results = await _decisionServices.GetDecisionsAsync(userId, page, pageSize);
        return results.ToIActionResult(this);
    }
    [HttpGet("{decisionId}")]
    public async Task<IActionResult> GetByIdAsync(int decisionId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var results = await _decisionServices.GetDecisionById(userId, decisionId);
        return results.ToIActionResult(this);
    }

    [HttpDelete("{decisionId}")]
    public async Task<IActionResult> DeleteAsync(int decisionId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var results = await _decisionServices.DeleteDecisionAsync(userId, decisionId);
        return results.ToIActionResult(this);

    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, CreateDecisionDto updateDecisionDto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationErrorResponse<CreateDecisionDto>();
        }
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var results = await _decisionServices.UpdateDecisionAsync(userId, id, updateDecisionDto);
        return results.ToIActionResult(this);
    }

    [HttpPost("{id}/decision-item")]
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

    [HttpPut("{id}/decision-item/{itemId}")]
    public async Task<IActionResult> UpdateDecisionItemAsync(UpdateDecisionItemDto updateDecisionItemDto, int id, int itemId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var result = await _decisionServices.UpdateDecisionItemAsync(updateDecisionItemDto, userId, id, itemId);
        return result.ToIActionResult(this);
    }
    [HttpDelete("{id}/decision-item/{itemId}")]
    public async Task<IActionResult> DeleteDecisionItemAsync(int id, int itemId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var result = await _decisionServices.DeleteDecisionItemAsync(id, itemId, userId);
        return result.ToIActionResult(this);
    }

}