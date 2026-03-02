using System.Security.Claims;
using DecisionMaker.Dtos.DecisionItem;
using DecisionMaker.Helpers;
using DecisionMaker.Interfaces.Decision;
using DecisionMaker.Services.DecisionItemService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecisionMaker.Controllers;

[Authorize]
[Route("api/decisionItem")]
[ApiController]

public class DecisionItemController : ControllerBase
{

    private readonly IDecisionItemService _decisionItemsServices;

    public DecisionItemController(IDecisionItemService decisionItemService)
    {
        _decisionItemsServices = decisionItemService;
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(CreateDecisionItemDto createDecisionItemDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var result = await _decisionItemsServices.PostDecisionItemAsync(createDecisionItemDto, userId);
        return result.ToIActionResult(this);
    }
    [HttpPut("{decisionItemId}")]
    public async Task<IActionResult> UpdateAsync(UpdateDecisionItemDto updateDecisionItemDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var result = await _decisionItemsServices.UpdateDecisionItemAsync(updateDecisionItemDto, userId);
        return result.ToIActionResult(this);
    }
    [HttpDelete("{decisionItemId}")]
    public async Task<IActionResult> DeleteAsync(int decisionItemId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var result = await _decisionItemsServices.DeleteDecisionItemAsync(decisionItemId, userId);
        return result.ToIActionResult(this);
    }

}