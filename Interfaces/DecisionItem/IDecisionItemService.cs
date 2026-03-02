using DecisionMaker.Data;
using DecisionMaker.Dtos.DecisionItem;
using DecisionMaker.Dtos.Response;

namespace DecisionMaker.Interfaces.Decision;

public interface IDecisionItemService
{
    Task<ApiResponse<PostDecisionItemResponseDto>> PostDecisionItemAsync(CreateDecisionItemDto createDecisionItemDto, string userId);

    Task<ApiResponse<object>> UpdateDecisionItemAsync(UpdateDecisionItemDto updateDecisionItemDto, string userId);

    Task<ApiResponse<object>> DeleteDecisionItemAsync(int decisionItemId, string userId);
}