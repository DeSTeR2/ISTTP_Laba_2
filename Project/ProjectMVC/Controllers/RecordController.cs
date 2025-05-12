using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectInfrastructure.Context;
using ProjectInfrastructure.Models;
using ProjectMVC.Utils.Errors;
using ProjectMVC.Utils.Extensions;
using ProjectMVC.Utils.Sorting;

namespace ProjectMVC.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RecordController : Controller
{
    private readonly LeaderboardDbContext _leaderboardDbContext;

    public RecordController(LeaderboardDbContext leaderboardDbContext)
    {
        _leaderboardDbContext = leaderboardDbContext;
    }

    [HttpPatch("update-positions/{leaderboardId}")]
    public async Task<IActionResult> UpdatePositions(int leaderboardId)
    {
        var leaderboard = await _leaderboardDbContext.FindLeaderboardAsync(leaderboardId);

        if (leaderboard == null)
        {
            return BadRequest(new LeaderboardError().Error(leaderboardId));
        }

        try
        {
            List<LeaderboardRecordModel> records =
                new AscendingSort(SortingParametr.Value).Sort(leaderboard.Records.ToList());

            for (int i = 0; i < records.Count; i++)
            {
                records[i].Place = i + 1;
            }

            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e);
        }
    }

    [HttpPatch("update-record")]
    public async Task<IActionResult> UpdateRecord([FromBody] LeaderboardRecordModel record)
    {
        LeaderboardRecordModel updatedRecord = await FindRecordAsync(record.Id);
        if (updatedRecord is null)
        {
            return BadRequest(new RecordError().Error(record.Id));
        }

        updatedRecord.Update(record);

        _leaderboardDbContext.LeaderboardsRecords.Update(updatedRecord);
        await _leaderboardDbContext.SaveChangesAsync();

        return Ok(updatedRecord);
    }

    [HttpGet("get-records/{leaderboardId}&{strategy}")]
    public async Task<IActionResult> GetRecords(int leaderboardId, int strategy)
    {
        var leaderboard = await _leaderboardDbContext.FindLeaderboardAsync(leaderboardId);

        if (leaderboard == null)
        {
            return BadRequest(new LeaderboardError().Error(leaderboardId));
        }

        await UpdatePositions(leaderboardId);
        
        SortingStrategy sortingStrategy = new SortingFactory().GetStrategy(strategy, SortingParametr.Place);
        leaderboard.Records = sortingStrategy.Sort(leaderboard.Records.ToList());
        
        return Ok(leaderboard.Records);
    }

    private async Task<LeaderboardRecordModel?> FindRecordAsync(int id)
    {
        return await _leaderboardDbContext.LeaderboardsRecords
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}