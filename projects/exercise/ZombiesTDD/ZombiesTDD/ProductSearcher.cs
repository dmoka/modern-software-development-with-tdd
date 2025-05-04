using ZombiesTDD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZombiesTDD
{
    public class ProductSearcher
    {
        public IList<Product> Search(IList<Product> products, string term, QualityStatus status)
        {
            if (products == null)
            {
                throw new ApplicationException("Null cannot be specified as input");
            }

            var searchResult = FilterBySearchTerm(products, term);
            searchResult = FilterByQualityStatus(searchResult, status);

            return searchResult;
        }


        private static List<Product> FilterBySearchTerm(IList<Product> products, string term)
        {
            return products.Where(p => p.Name.ToLower().Contains(term.ToLower()) || p.Description.ToLower().Contains(term.ToLower())).ToList();
        }

        private static List<Product> FilterByQualityStatus(List<Product> searchResult, QualityStatus status)
        {
            searchResult = searchResult.Where(p => p.StockLevel.QualityStatus == status).ToList();

            return searchResult;
        }
    }
}
