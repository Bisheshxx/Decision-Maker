using System.Security.Claims;
using DecisionMaker.Dtos.Decision;
using DecisionMaker.Helpers;
using DecisionMaker.Interfaces.Decision;
using DecisionMaker.Services.DecisionService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecisionMaker.Controllers;

[Authorize]
[Route("api/decisions")]
[ApiController]
public class DecisionController : ControllerBase
{
    private readonly IDecisionService _decisionServices;

    public DecisionController(IDecisionService decisionServices)
    {
        _decisionServices = decisionServices;
    }


    [HttpPost]

    public async Task<IActionResult> PostAsync(CreateDecisionDto dto)
    {
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

    [HttpDelete]
    public async Task<IActionResult> DeleteAsync(int decisionId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var results = await _decisionServices.DeleteDecisionAsync(userId, decisionId);
        return results.ToIActionResult(this);

    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, CreateDecisionDto updateDecisionDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var results = await _decisionServices.UpdateDecisionAsync(userId, id, updateDecisionDto);
        return results.ToIActionResult(this);
    }

}