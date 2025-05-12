using ProjectInfrastructure.Models;
using ProjectMVC.Controllers;

namespace ProjectMVC.Utils.Sorting;

public abstract class SortingStrategy
{
    private readonly SortingParametr _sortingParametr;

    public SortingStrategy(SortingParametr sortingParametr)
    {
        _sortingParametr = sortingParametr;
    }

    public abstract List<LeaderboardRecordModel> Sort(List<LeaderboardRecordModel> records);

    protected int CompareBy(LeaderboardRecordModel rec1, LeaderboardRecordModel rec2)
    {
        switch (_sortingParametr)
        {
            case SortingParametr.Value:
                return rec1.Value < rec2.Value ? -1 : rec1.Value > rec2.Value ? 1 : 0;
            case SortingParametr.Place:
                return rec1.Place < rec2.Place ? -1 : rec1.Place > rec2.Place ? 1 : 0;
            case SortingParametr.UpdatedTime:
                return rec1.UpdatedTime.CompareTo(rec2.UpdatedTime);
            case SortingParametr.Name:
                return string.Compare(rec1.Name, rec2.Name, StringComparison.OrdinalIgnoreCase);
            default:
                throw new ArgumentOutOfRangeException(nameof(_sortingParametr), $"Unknown sort parameter: {_sortingParametr}");
        }
    }
}