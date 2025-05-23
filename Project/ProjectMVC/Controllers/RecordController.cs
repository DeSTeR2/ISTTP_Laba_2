using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
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

    private async Task<IActionResult> UpdatePositions(string leaderboardId)
    {
        var leaderboard = await _leaderboardDbContext.FindLeaderboardAsync(leaderboardId);

        if (leaderboard == null)
        {
            return BadRequest(new LeaderboardError().Error(leaderboardId));
        }

        try
        {
            List<LeaderboardRecordModel> records =
                new DescendingSort(SortingParametr.Value).Sort(leaderboard.Records.ToList());

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

    [HttpPatch("/{leaderboardId}")]
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

    [HttpGet("get-records/{leaderboardId}/")]
    [EnableQuery]
    public async Task<IActionResult> GetRecords(string leaderboardId, ODataQueryOptions<LeaderboardRecordModel> query)
    {
        var leaderboard = await _leaderboardDbContext.FindLeaderboardAsync(leaderboardId);

        if (leaderboard == null)
        {
            return BadRequest(new LeaderboardError().Error(leaderboardId));
        }

        await UpdatePositions(leaderboardId);

        var records = _leaderboardDbContext.LeaderboardsRecords.AsQueryable();
        var result = (IQueryable<LeaderboardRecordModel>)query.ApplyTo(records);
        
        return Ok(result);
    }

    [HttpPost("/{id}")]
    public async Task<IActionResult> AddRecord([FromBody] LeaderboardRecordModel record, string leaderboardId)
    {
        record.LeaderboardId = leaderboardId;
        
        var leaderboard = await _leaderboardDbContext.FindLeaderboardAsync(record.LeaderboardId);
        if (leaderboard is null)
        {
            return BadRequest(new LeaderboardError().Error(record.LeaderboardId));
        }

        leaderboard.AddRecord(record);
        
        _leaderboardDbContext.LeaderboardsRecords.Add(record);
        _leaderboardDbContext.Leaderboards.Update(leaderboard);
        await _leaderboardDbContext.SaveChangesAsync();

        return Ok(leaderboard);
    }
    
    [HttpDelete("records/{recordId}")]
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