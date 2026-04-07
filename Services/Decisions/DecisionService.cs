using DecisionMaker.Data;
using DecisionMaker.Models;
using DecisionMaker.Dtos.Decision;
using DecisionMaker.Dtos.Response;
using DecisionMaker.Interfaces.Decision;
using Microsoft.EntityFrameworkCore;
using DecisionMaker.Shared.Pagination.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using DecisionMaker.Dtos.DecisionItem;

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
        if (userId == null)
        {
            return ApiResponse<DecisionDto>.Fail("You need to login", ErrorType.Unauthorized);
        }
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
            UserId = userId
        };
        return ApiResponse<DecisionDto>.Ok(createDecision, "Added a New Decision Successfully");
    }

    public async Task<ApiResponse<IEnumerable<DecisionListDto>>> GetDecisionsAsync(string userId, int page, int pageSize, string searchTerm)
    {
        if (userId == null)
        {
            return ApiResponse<IEnumerable<DecisionListDto>>.Fail("You need to login", ErrorType.Unauthorized);
        }
        if (page <= 0)
            page = 1;
        if (pageSize <= 0)
            pageSize = 10;
        if (pageSize >= 50)
            pageSize = 50;
        var query = _context.Decision.Where(d => d.UserId == userId).OrderByDescending(o => o.CreatedAt);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(d => d.UserId == userId && ((d.Title != null && d.Title.ToLower().Contains(searchTerm.ToLower())) ||
        (d.Description != null && d.Description.ToLower().Contains(searchTerm.ToLower())))).OrderByDescending(o => o.CreatedAt);
        }

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
        return ApiResponse<object>.Ok(null, "Successfully Deleted Decision");

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
        return ApiResponse<object>.Ok(null, "Decision updated Successfully");
    }

    public async Task<ApiResponse<DecisionListResponseDto>> GetDecisionById(string userId, int id)
    {
        var query = _context.Decision;
        var decision = await query.Where(d => d.Id == id && d.UserId == userId).Select(d => new DecisionListResponseDto
        {
            Id = d.Id,
            Title = d.Title,
            Description = d.Description,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt,
            DecisionItems = d.DecisionItems.Select(di => new DecisionItemResponseDto
            {
                Title = di.Title,
                Id = di.Id,
            }).ToList()
        }).FirstOrDefaultAsync();

        if (decision == null)
        {
            return ApiResponse<DecisionListResponseDto>.Fail("Decision not Found", ErrorType.NotFound);
        }
        return ApiResponse<DecisionListResponseDto>.Ok(decision, "Successfully Fetched Decision");
    }
    public async Task<ApiResponse<List<DecisionItemResponseDto>>> GetDecisionItemAsync(string userId, int id)
    {
        if (userId == null)
        {
            ApiResponse<List<DecisionItemResponseDto>>.Fail("You need to Login", ErrorType.Unauthorized);
        }
        // var decisionItem = _context.DecisionItem.Where(d => d.CreatedById == userId && d.DecisionId == id).Select(d => new DecisionItemResponseDto
        // {
        //     Id = d.Id,
        //     Title = d.Title
        // }).ToListAsync();
        var decisionItems = await _context.DecisionItem
       .Where(d => d.CreatedById == userId && d.DecisionId == id)
       .Select(d => new DecisionItemResponseDto
       {
           Id = d.Id,
           Title = d.Title
       })
       .ToListAsync();
        return ApiResponse<List<DecisionItemResponseDto>>.Ok(
      decisionItems,
      "Successfully fetched decision items"
  );


    }
    public async Task<ApiResponse<PostDecisionItemResponseDto>> PostDecisionItemAsync(CreateDecisionItemDto createDecisionItemDto, string userId, int decisionId)
    {
        var decisionItem = new DecisionItem()
        {
            Title = createDecisionItemDto.Title,
            DecisionId = decisionId,
            CreatedById = userId
        };
        _context.DecisionItem.Add(decisionItem);
        await _context.SaveChangesAsync();

        var postDecisionItemResponseDto = new PostDecisionItemResponseDto
        {
            Title = decisionItem.Title,
            Id = decisionItem.Id,
            DecisionId = decisionItem.DecisionId
        };

        return ApiResponse<PostDecisionItemResponseDto>.Ok(postDecisionItemResponseDto, "Successfully created a decision item.");
    }
    public async Task<ApiResponse<object>> UpsertDecisionItemAsync(List<UpsertDecisionItemDto> upsertDecisionItemDto, string userId, int id)
    {
        var existingDecisionItems = await _context.DecisionItem.Where(d => id == d.DecisionId).ToListAsync();

        var incomingIds = upsertDecisionItemDto.Select(d => d.Id).ToList();

        var toDelete = existingDecisionItems.Where(d => !incomingIds.Contains(d.Id));
        _context.DecisionItem.RemoveRange(toDelete);
        foreach (var item in upsertDecisionItemDto)
        {
            if (item.Id == 0)
            {
                _context.DecisionItem.Add(new DecisionItem
                {
                    DecisionId = id,
                    Title = item.Title,
                    CreatedById = userId
                });
            }
            else
            {
                var existing = existingDecisionItems.FirstOrDefault(d => d.Id == item.Id);
                if (existing != null)
                {
                    existing.Title = item.Title;
                }
            }
        }
        await _context.SaveChangesAsync();
        return ApiResponse<object>.Ok(null, "Updated Successfully!");
    }


    public async Task<ApiResponse<object>> UpdateDecisionItemAsync(UpdateDecisionItemDto updateDecisionItemDto, string userId, int decisionId, int decisionItemId)
    {

        var query = _context.DecisionItem;
        var decisionItem = await query.FirstOrDefaultAsync(d => d.Id == decisionItemId && d.DecisionId == decisionId && d.CreatedById == userId);
        if (decisionItem == null)
        {
            return ApiResponse<object>.Fail("Decision item not found.", ErrorType.NotFound);
        }

        decisionItem.Title = updateDecisionItemDto.Title;
        await _context.SaveChangesAsync();
        return ApiResponse<object>.Ok(null, "Decision Item successfully updated");
    }

    public async Task<ApiResponse<object>> DeleteDecisionItemAsync(int id, int itemId, string userId)
    {
        var rowsAffected = await _context.DecisionItem.Where(d => d.Id == itemId && d.CreatedById == userId && d.DecisionId == id).ExecuteDeleteAsync();
        if (rowsAffected == 0)
        {
            return ApiResponse<object>.Fail("Decision Item Not Found!", ErrorType.NotFound);
        }
        return ApiResponse<object>.Ok(null, "Successfully Deleted Decision Item!");
    }

}