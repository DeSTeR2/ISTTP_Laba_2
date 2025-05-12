namespace ProjectMVC.Utils.Sorting;

public class SortingFactory
{
    public SortingStrategy GetStrategy(int strategy, SortingParametr sortingParametr)
    {
        if (strategy == 1)
        {
            return new AscendingSort(sortingParametr);
        }
        else return new DescendingSort(sortingParametr);
    }
}