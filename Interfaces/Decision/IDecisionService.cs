using DecisionMaker.Dtos.Decision;
using DecisionMaker.Dtos.DecisionItem;
using DecisionMaker.Dtos.Response;
using Microsoft.OpenApi.Any;

namespace DecisionMaker.Interfaces.Decision;

public interface IDecisionService
{

    Task<ApiResponse<DecisionDto>> PostDecisionAsync(string userId, CreateDecisionDto dto);
    Task<ApiResponse<IEnumerable<DecisionListDto>>> GetDecisionsAsync(string userId, int page, int pageSize, string searchTerm);

    Task<ApiResponse<object>> DeleteDecisionAsync(string userId, int decisionId);
    Task<ApiResponse<object>> UpdateDecisionAsync(string userId, int decisionId, CreateDecisionDto createDecisionDto);

    Task<ApiResponse<DecisionListResponseDto>> GetDecisionById(string userId, int id);

    Task<ApiResponse<List<DecisionItemResponseDto>>> GetDecisionItemAsync(string userId, int id);
    Task<ApiResponse<PostDecisionItemResponseDto>> PostDecisionItemAsync(CreateDecisionItemDto createDecisionItemDto, string userId, int id);
    Task<ApiResponse<object>> UpsertDecisionItemAsync(List<UpsertDecisionItemDto> upsertDecisionItemDto, string userId, int id);

    Task<ApiResponse<object>> UpdateDecisionItemAsync(UpdateDecisionItemDto updateDecisionItemDto, string userId, int id, int itemId);

    Task<ApiResponse<object>> DeleteDecisionItemAsync(int id, int itemId, string userId);
}