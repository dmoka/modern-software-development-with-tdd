﻿namespace MutationTestingTDD.Domain
{
    public interface IProductRepository
    {
        Task<Product> GetAsync(Guid id);

        Task<List<Product>> GetAllAsync(ProductCategory category);
        Product Create(Product product);
        bool Exists(string name);
    }
}
