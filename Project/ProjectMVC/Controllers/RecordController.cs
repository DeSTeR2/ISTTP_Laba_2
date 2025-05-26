using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectInfrastructure.Context;
using ProjectInfrastructure.Models;
using ProjectMVC.Utils.Errors;
using ProjectMVC.Utils.Extensions;
using ProjectMVC.Utils.Sorting;

namespace ProjectMVC.Controllers;

[Route("records")]
[ApiController]
public class RecordController : Controller
{
    private readonly LeaderboardDbContext _leaderboardDbContext;

    public RecordController(LeaderboardDbContext leaderboardDbContext)
    {
        _leaderboardDbContext = leaderboardDbContext;
    }

    [HttpGet("/Record/Index")]
    public async Task<IActionResult> Index(string leaderboardId)
    {
        ViewData["LeaderboardId"] = leaderboardId;
        var leaderboard = await _leaderboardDbContext.FindLeaderboardAsync(leaderboardId);

        if (leaderboard == null)
        {
            return BadRequest(new LeaderboardError().Error(leaderboardId));
        }

        await UpdatePositions(leaderboardId);
        var records = leaderboard.Records ?? Enumerable.Empty<LeaderboardRecordModel>();

        return View(records);
    }


    [HttpPatch]
    public async Task<IActionResult> UpdatePositions(
        string leaderboardId,
        SortingParameter sortBy = SortingParameter.Value,
        SortingType direction = SortingType.Ascending,
        int? take = null)
    {
        var leaderboard = await _leaderboardDbContext.FindLeaderboardAsync(leaderboardId);

        if (leaderboard == null)
        {
            return BadRequest(new LeaderboardError().Error(leaderboardId));
        }

        try
        {
            IEnumerable<LeaderboardRecordModel> records = leaderboard.Records;
            SortingStrategy sortingStrategy = new SortingFactory().GetStrategy(sortBy, direction);

            if (take.HasValue)
            {
                records = records.Take(take.Value);
            }

            sortingStrategy.Sort(records.ToList());
            var sortedList = records.ToList();
            for (int i = 0; i < sortedList.Count; i++)
            {
                sortedList[i].Place = i + 1;
            }
            
            return Ok(sortedList);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }


    [HttpPut]
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

    [HttpGet]
    public async Task<IActionResult> GetRecords(string leaderboardId)
    {
        var leaderboard = await _leaderboardDbContext.FindLeaderboardAsync(leaderboardId);

        if (leaderboard == null)
        {
            return BadRequest(new LeaderboardError().Error(leaderboardId));
        }

        await UpdatePositions(leaderboardId);
        return Ok(leaderboard.Records);
    }

    [HttpPost("{leaderboardId}")]
    public async Task<IActionResult> AddRecord([FromBody] LeaderboardRecordModel record, [FromRoute(Name = "leaderboardId")] string leaderboardId)
    {
        record.LeaderboardId = leaderboardId;
        record.Place = -1;
        record.Id = Guid.NewGuid().ToString();
    
        var leaderboard = await _leaderboardDbContext.FindLeaderboardAsync(leaderboardId);
        if (leaderboard is null)
        {
            return BadRequest(new LeaderboardError().Error(leaderboardId));
        }

        leaderboard.AddRecord(record);
    
        _leaderboardDbContext.LeaderboardsRecords.Add(record);
        _leaderboardDbContext.Leaderboards.Update(leaderboard);
        await _leaderboardDbContext.SaveChangesAsync();
        await UpdatePositions(leaderboardId);

        return Ok(leaderboard);
    }


    [HttpDelete("{recordId}")]
    public async Task<IActionResult> AddRecord(string recordId)
    {
        var record = await _leaderboardDbContext.LeaderboardsRecords.FirstOrDefaultAsync(r => r.Id == recordId);
        if (record is null)
        {
            return BadRequest(new RecordError().Error(recordId));
        }
        
        string leaderboardId = record.LeaderboardId;

        var leaderboard = await _leaderboardDbContext.FindLeaderboardAsync(leaderboardId);
        leaderboard?.RemoveRecord(record);

        _leaderboardDbContext.Update<LeaderboardModel>(leaderboard);
        await _leaderboardDbContext.SaveChangesAsync();

        return Ok(leaderboard);
    }

    private async Task<LeaderboardRecordModel?> FindRecordAsync(string id)
    {
        return await _leaderboardDbContext.LeaderboardsRecords
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}