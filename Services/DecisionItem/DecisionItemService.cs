// using DecisionMaker.Data;
// using DecisionMaker.Dtos.DecisionItem;
// using DecisionMaker.Dtos.Response;
// using DecisionMaker.Interfaces.Decision;
// using DecisionMaker.Models;
// using Microsoft.EntityFrameworkCore;

// namespace DecisionMaker.Services.DecisionItemService;

// public class DecisionItemService : IDecisionItemService
// {
//     private readonly ApplicationDbContext _context;

//     public DecisionItemService(ApplicationDbContext context)
//     {
//         _context = context;
//     }



//     // public async Task<ApiResponse<object>> UpdateDecisionItemAsync(UpdateDecisionItemDto updateDecisionItemDto, string userId)
//     // {

//     //     var query = _context.DecisionItem;
//     //     var decisionItem = await query.FirstOrDefaultAsync(d => d.Id == updateDecisionItemDto.Id && d.DecisionId == updateDecisionItemDto.DecisionId && d.CreatedById == userId);
//     //     if (decisionItem == null)
//     //     {
//     //         return ApiResponse<object>.Fail("Decision item not found.", ErrorType.NotFound);
//     //     }

//     //     decisionItem.Title = updateDecisionItemDto.Title;
//     //     await _context.SaveChangesAsync();
//     //     return ApiResponse<object>.Ok(null, "Decision Item successfully updated");
//     // }

//     // public async Task<ApiResponse<object>> DeleteDecisionItemAsync(int decisionItemId, string userId)
//     // {
//     //     var rowsAffected = await _context.DecisionItem.Where(d => d.Id == decisionItemId && d.CreatedById == userId).ExecuteDeleteAsync();
//     //     if (rowsAffected == 0)
//     //     {
//     //         return ApiResponse<object>.Fail("Decision Item Not Found!", ErrorType.NotFound);
//     //     }
//     //     return ApiResponse<object>.Ok(null, "Successfully Deleted Decision Item!");
//     // }


// }