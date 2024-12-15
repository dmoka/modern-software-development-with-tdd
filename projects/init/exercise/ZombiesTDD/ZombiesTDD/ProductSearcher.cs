namespace ZombiesTDD;


public class ProductSearcher
{
    public static IEnumerable<Product> Search(List<Product> products, string searchTerm, QualityStatus damaged,
        Ordering ordering)
    {
        if (products == null)
        {
            throw new ApplicationException("Products cannot be null");
        }

        var searchResults = FilterBySearchTerm(products, searchTerm);
        searchResults = FilterByQualityStatus(damaged, searchResults);
        searchResults = OrderResults(ordering, searchResults);

        return searchResults;
    }


    private static IEnumerable<Product> FilterBySearchTerm(List<Product> products, string searchTerm)
    {
        var searchResults = products.Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()) || p.Description.ToLower().Contains(searchTerm.ToLower()));
        return searchResults;
    }

    private static IEnumerable<Product> FilterByQualityStatus(QualityStatus damaged, IEnumerable<Product> searchResults)
    {
        searchResults = searchResults.Where(p => p.StockLevel.QualityStatus == damaged);
        return searchResults;
    }

    private static IEnumerable<Product> OrderResults(Ordering ordering, IEnumerable<Product> searchResults)
    {
        return ordering switch
        {
            Ordering.ByNameAscending => searchResults.OrderBy(p => p.Name),
            Ordering.ByNameDescending => searchResults.OrderByDescending(p => p.Name),
            Ordering.ByPriceAscending => searchResults.OrderBy(p => p.Price),
            Ordering.ByPriceDescending => searchResults.OrderByDescending(p => p.Price),
            _ => searchResults
        };
    }
}

public enum Ordering
{
    ByNameAscending,
    ByNameDescending,
    ByPriceAscending,
    ByPriceDescending
}