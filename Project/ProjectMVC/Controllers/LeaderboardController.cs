using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectInfrastructure.Context;
using ProjectInfrastructure.Models;

namespace ProjectMVC.Controllers;


[Route("api/[controller]")]
[ApiController]
public class LeaderboardController : ControllerBase
{
    private readonly LeaderboardDbContext _leaderboardDbContext;

    public LeaderboardController(LeaderboardDbContext leaderboardDbContext)
    {
        _leaderboardDbContext = leaderboardDbContext;
    }    
    
    [HttpPost]
    public async Task<JsonResult> GetAll()
    {
        var leaderboards = await _leaderboardDbContext.Leaderboards.FindAsync();
        return new JsonResult(leaderboards);
    }
}