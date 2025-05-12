namespace ProjectMVC.Utils.Errors;

public class LeaderboardError : IDbError
{
    public string Error(int objectId)
    {
        return $"Leaderboard with ID: {objectId} is not present in db";
    }
}