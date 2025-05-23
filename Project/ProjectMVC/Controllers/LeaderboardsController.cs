using Microsoft.AspNetCore.Identity;
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

[Route("leaderboards")]
[ApiController]
public class LeaderboardsController : ODataController
{
    private readonly LeaderboardDbContext _leaderboardDbContext;
    private readonly UserManager<User> _userManager;

    public LeaderboardsController(LeaderboardDbContext leaderboardDbContext, UserManager<User> userManager)
    {
        _leaderboardDbContext = leaderboardDbContext;
        _userManager = userManager;
    }

    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get([FromQuery] string? query)
    {
        List<LeaderboardModel?> leaderboards = await _leaderboardDbContext.Leaderboards.ToListAsync();
        return Ok(leaderboards);
    }

    [HttpPost]
    public async Task<IActionResult> Upload([FromBody] LeaderboardModel? leaderboard = null)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (string.IsNullOrEmpty(leaderboard?.Name))
            {
                Random rng = new Random();
                if (leaderboard != null)
                {
                    leaderboard.Name = "New leaderboard " + rng.Next(100000);
                }
                
                leaderboard.UserId = user.Id;
            }

            leaderboard.Id = Guid.NewGuid().ToString();
            
            await _leaderboardDbContext.Leaderboards.AddAsync(leaderboard);
            
            user.LeaderboaradIds.Add(leaderboard.Id);
            
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
    public async Task<IActionResult> GetLeaderboard(string id)
    {
        var leaderboard = await _leaderboardDbContext.FindLeaderboardAsync(id);
        if (leaderboard is null)
        {
            return BadRequest(new LeaderboardError().Error(id));
        }

        return Ok(leaderboard);
    }

    [HttpDelete("/{id}")]
    public async Task<IActionResult> Delete(string id)
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