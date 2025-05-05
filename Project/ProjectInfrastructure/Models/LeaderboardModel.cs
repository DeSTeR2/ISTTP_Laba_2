namespace ProjectInfrastructure.Models;

public class LeaderboardModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; }
    public ICollection<LeaderboardRecordModel> Records { get; set; }
    public User User { get; set; }
}