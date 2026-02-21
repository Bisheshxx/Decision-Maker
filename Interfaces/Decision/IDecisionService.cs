using DecisionMaker.Dtos.Decision;
using DecisionMaker.Dtos.Response;
using Microsoft.OpenApi.Any;

namespace DecisionMaker.Interfaces.Decision;

public interface IDecisionService
{
    Task<ApiResponse<DecisionDto>> PostDecisionAsync(string userId, CreateDecisionDto dto);
    Task<ApiResponse<IEnumerable<DecisionListDto>>> GetDecisionsAsync(string userId, int page, int pageSize);

    Task<ApiResponse<object>> DeleteDecisionAsync(string userId, int decisionId);
    Task<ApiResponse<object>> UpdateDecisionAsync(string userId, int decisionId, CreateDecisionDto createDecisionDto);
}