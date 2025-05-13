using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using ProjectInfrastructure.Context;
using ProjectInfrastructure.Models;
using ProjectMVC.Utils.Errors;
using ProjectMVC.Utils.Extensions;

namespace ProjectMVC.Controllers;


/*
 /Leaderboards  - all collection
    get - get all    (query)
    post - upload    (body)
    delete - delete all  (query)
    put - replace (body)
 
  /Leaderboards/{id} - item 
    get - get by id  (query)
    patch - update by id (body)
    delete - delete by id (query)
    

    Records!
    get - /Leaderboards/{id}/{recordId} (query)
     
 */

[Route("api")]
[ApiController]
public class LeaderboardsController : ODataController
{
    private readonly LeaderboardDbContext _leaderboardDbContext;

    public LeaderboardsController(LeaderboardDbContext leaderboardDbContext)
    {
        _leaderboardDbContext = leaderboardDbContext;
    }    
    
    [HttpGet]
    [EnableQuery]
    public async Task<JsonResult> Get([FromQuery] string? query)
    {
        List<LeaderboardModel?> leaderboards = await _leaderboardDbContext.Leaderboards.ToListAsync();
        return new JsonResult(leaderboards); 
    }

    [HttpPost]
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

    [HttpPatch]
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

    [HttpGet("/{id}")]
    public async Task<IActionResult> GetLeaderboard(int id)
    {
        var leaderboard = await _leaderboardDbContext.FindLeaderboardAsync(id);
        if (leaderboard is null)
        {
            return BadRequest(new LeaderboardError().Error(id));
        }
        
        return Ok(leaderboard);
    }

    [HttpDelete("/{id}")]
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
}