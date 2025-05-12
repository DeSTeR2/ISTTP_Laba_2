namespace ProjectMVC.Utils.Errors;

public class RecordError : IDbError
{
    public string Error(int objectId)
    {
        return $"Record with ID: {objectId} is not present in db";
    }
}