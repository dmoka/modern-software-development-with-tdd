namespace DomainTDD;

public class ProductSearcher
{
    public enum Sorting
    {
        ByNameAscending,
        ByNameDescending,
        ByPriceAscending,
        ByPriceDescending
    }

    public static IList<Product> Search(List<Product> products, string searchText, decimal? minPrice = null,
        decimal? maxPrice = null, QualityStatus qualityStatus = QualityStatus.Available, Sorting sorting = Sorting.ByNameAscending)
    {
        if (products == null)
        {
            throw new ApplicationException("The provided list cannot be null");
        }

        var searchResult = FilterByTextSearch(products, searchText);
        searchResult = FilterByPrice(minPrice, maxPrice, searchResult);
        searchResult = FilterByQualityStatus(qualityStatus, searchResult);
        searchResult = SortResults(sorting, searchResult);

        return searchResult;
    }

    private static List<Product> FilterByTextSearch(List<Product> products, string searchText)
    {
        var searchResult = products.Where(p => p.Name.Contains(searchText) || p.Description.Contains(searchText)).ToList();
        return searchResult;
    }


    private static List<Product> FilterByPrice(decimal? minPrice, decimal? maxPrice, List<Product> searchResult)
    {
        if (minPrice < 0)
        {
            throw new ApplicationException("The MinPrice be bigger or equal to 0");
        }

        if (maxPrice < 0)
        {
            throw new ApplicationException("The MaxPrice be bigger or equal to 0");
        }

        if (minPrice > maxPrice)
        {
            throw new ApplicationException("The MinPrice should be smaller than MaxPrice");
        }

        if (minPrice.HasValue)
        {
            searchResult = searchResult.Where(p => p.Price >= minPrice).ToList();
        }

        if (maxPrice.HasValue)
        {
            searchResult = searchResult.Where(p => p.Price <= maxPrice).ToList();
        }

        return searchResult;
    }

    private static List<Product> FilterByQualityStatus(QualityStatus qualityStatus, List<Product> searchResult)
    {
        if (qualityStatus != QualityStatus.Available)
        {
            searchResult = searchResult.Where(p => p.StockLevel.QualityStatus != QualityStatus.Available).ToList();
        }

        return searchResult;
    }

    private static List<Product> SortResults(Sorting sorting, List<Product> searchResult)
    {
        searchResult = sorting switch
        {
            Sorting.ByNameAscending => searchResult.OrderBy(p => p.Name).ToList(),
            Sorting.ByNameDescending => searchResult.OrderByDescending(p => p.Name).ToList(),
            Sorting.ByPriceAscending => searchResult.OrderBy(p => p.Price).ToList(),
            Sorting.ByPriceDescending => searchResult.OrderByDescending(p => p.Price).ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(sorting), sorting, null)
        };

        return searchResult;
    }

}