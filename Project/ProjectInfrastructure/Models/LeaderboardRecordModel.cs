namespace ProjectInfrastructure.Models;

public class LeaderboardRecordModel
{
    public int Id { get; set; }
    public int Place { get; set; }
    public string Name { get; set; }
    public int Value { get; set; }
    public DateTime UpdatedTime { get; set; }
    
    public LeaderboardModel Leaderboard { get; set; }
    public int LeaderboardId { get; set; }
}