using DecisionMaker.Data;
using DecisionMaker.Models;
using DecisionMaker.Dtos.Decision;
using DecisionMaker.Dtos.Response;
using DecisionMaker.Interfaces.Decision;
using Microsoft.EntityFrameworkCore;
using DecisionMaker.Shared.Pagination.Dto;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DecisionMaker.Services.DecisionService;

public class DecisionServices : IDecisionService
{
    private readonly ApplicationDbContext _context;
    public DecisionServices(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<DecisionDto>> PostDecisionAsync(string userId, CreateDecisionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return ApiResponse<DecisionDto>.Fail("Title is required",
                ErrorType.Validation);
        }

        var decision = new Decision
        {
            Title = dto.Title,
            Description = dto.Description,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Decision.Add(decision);
        await _context.SaveChangesAsync();

        var createDecision = new DecisionDto
        {
            Id = decision.Id,
            Title = decision.Title,
            Description = decision.Description,
            CreatedAt = decision.CreatedAt,
            UpdatedAt = decision.UpdatedAt,
            // UserId = userId
        };
        return ApiResponse<DecisionDto>.Ok(createDecision, "Added a New Decision Successfully");
    }

    public async Task<ApiResponse<IEnumerable<DecisionListDto>>> GetDecisionsAsync(string userId, int page, int pageSize)
    {
        if (page <= 0)
            page = 1;
        if (pageSize <= 0)
            pageSize = 10;
        if (pageSize >= 50)
            pageSize = 50;
        var query = _context.Decision.Where(d => d.UserId == userId).OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync();

        var decision = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(d => new DecisionListDto
        {
            Id = d.Id,
            Title = d.Title,
            Description = d.Description,
            CreatedAt = d.CreatedAt
        }).ToListAsync();

        var meta = new PaginationMeta(page, pageSize, totalCount);

        return ApiResponse<IEnumerable<DecisionListDto>>.Ok(decision, "Successfully fetched Decisions", meta);
    }

    public async Task<ApiResponse<object>> DeleteDecisionAsync(string userId, int decisionId)
    {
        var rowsAffected = await _context.Decision.Where(d => d.Id == decisionId && d.UserId == userId).ExecuteDeleteAsync();

        if (rowsAffected == 0)
        {
            return ApiResponse<object>.Fail("Decision not Found.", ErrorType.NotFound);
        }
        return ApiResponse<object>.Ok("Successfully Deleted Decision");

    }

    public async Task<ApiResponse<object>> UpdateDecisionAsync(string userId, int decisionId, CreateDecisionDto createDecisionDto)
    {
        var query = _context.Decision;
        var decision = await query.FirstOrDefaultAsync(d => d.Id == decisionId && d.UserId == userId);

        if (decision == null)
        {
            return ApiResponse<object>.Fail("Decision not found.", ErrorType.NotFound);
        }
        decision.Title = createDecisionDto.Title;
        decision.Description = createDecisionDto.Description;
        decision.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ApiResponse<object>.Ok("Decision updated Successfully");
    }

}