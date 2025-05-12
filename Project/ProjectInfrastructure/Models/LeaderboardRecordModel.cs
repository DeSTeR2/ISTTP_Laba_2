using System.Text.Json.Serialization;

namespace ProjectInfrastructure.Models;

public class LeaderboardRecordModel
{
    public int Id { get; set; }
    public int? Place { get; set; }
    public string? Name { get; set; }
    public int? Value { get; set; }
    public DateTime UpdatedTime { get; set; }
    
    public int LeaderboardId { get; set; }
    
    [JsonIgnore]
    public LeaderboardModel? Leaderboard { get; set; }

    public void Update(LeaderboardRecordModel record)
    {
        if (string.IsNullOrEmpty(record.Name))
        {
            Name = record.Name;
        }

        if (record.Value != 0)
        {
            Value = record.Value;
        }

        UpdatedTime = record.UpdatedTime;
    }
}