using Microsoft.AspNetCore.Identity;

namespace ProjectInfrastructure.Models;


public class User : IdentityUser
{
    public ICollection<int>? LeaderboaradIds { get; set; }
    public ICollection<LeaderboardModel>? Leaderboards { get; set; }
}