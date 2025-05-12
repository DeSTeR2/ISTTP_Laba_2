using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectInfrastructure.Context;
using ProjectInfrastructure.Models;
using ProjectMVC.Utils.Errors;
using ProjectMVC.Utils.Extensions;

namespace ProjectMVC.Controllers;


[Route("api/[controller]")]
[ApiController]
public class LeaderboardsController : ControllerBase
{
    private readonly LeaderboardDbContext _leaderboardDbContext;

    public LeaderboardsController(LeaderboardDbContext leaderboardDbContext)
    {
        _leaderboardDbContext = leaderboardDbContext;
    }    
    
    [HttpPost("get-all")]
    public async Task<JsonResult> GetAll()
    {
        List<LeaderboardModel?> leaderboards = await _leaderboardDbContext.Leaderboards.ToListAsync();
        return new JsonResult(leaderboards);
    }

    [HttpGet("get-leaderboard/{id}")]
    public async Task<IActionResult> GetLeaderboard(int id)
    {
        var leaderboard = await _leaderboardDbContext.FindLeaderboardAsync(id);
        if (leaderboard is null)
        {
            return BadRequest($"Leaderboard with Id: {id} is not present in database");
        }
        
        return Ok(leaderboard);
    }
    
    [HttpPut("upload")]
    public async Task<IActionResult> Upload([FromBody] LeaderboardModel? leaderboard)
    {
        try
        {
            if (leaderboard.Records != null)
            {
                foreach (var record in leaderboard.Records)
                {
                    record.Leaderboard = leaderboard;
                    record.Id = leaderboard.Id; 
                    await _leaderboardDbContext.LeaderboardsRecords.AddAsync(record);
                }
            }

            await _leaderboardDbContext.Leaderboards.AddAsync(leaderboard);
            await _leaderboardDbContext.SaveChangesAsync();
            return Ok(leaderboard); 
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpPut("add-record")]
    public async Task<IActionResult> AddRecord([FromBody] LeaderboardRecordModel record)
    {
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
    
    [HttpPatch("update")]
    public async Task<IActionResult> UpdateLeaderboard([FromBody] LeaderboardModel leaderboard)
    {
        var leaderboardInDb = await _leaderboardDbContext.FindLeaderboardAsync(leaderboard.Id);
        if (leaderboardInDb is null)
        {
            return BadRequest($"Leaderboard with Id: {leaderboard.Id} is not present in database");
        }
        
        leaderboardInDb.Update(leaderboard);
        
        _leaderboardDbContext.Leaderboards.Update(leaderboardInDb);
        await _leaderboardDbContext.SaveChangesAsync();

        return Ok(leaderboardInDb);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var leaderboard = await _leaderboardDbContext.FindLeaderboardAsync(id);
        if (leaderboard is null)
        {
            return BadRequest(new LeaderboardError().Error(id));
        }

        _leaderboardDbContext.Leaderboards.Remove(leaderboard);
        await _leaderboardDbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("delete-record-from-leaderboard/{recordId}")]
    public async Task<IActionResult> AddRecord(int recordId)
    {
        var record = await _leaderboardDbContext.LeaderboardsRecords.FirstOrDefaultAsync(r => r.Id == recordId);
        if (record is null)
        {
            return BadRequest(new RecordError().Error(recordId));
        }
        
        int leaderboardId = record.LeaderboardId;

        var leaderboard = await _leaderboardDbContext.FindLeaderboardAsync(leaderboardId);
        leaderboard?.RemoveRecord(record);

        _leaderboardDbContext.Update<LeaderboardModel>(leaderboard);
        await _leaderboardDbContext.SaveChangesAsync();

        return Ok(leaderboard);
    }
}