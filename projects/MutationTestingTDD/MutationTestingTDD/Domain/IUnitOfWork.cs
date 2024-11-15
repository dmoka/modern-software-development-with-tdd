namespace MutationTestingTDD.Domain
{
    public interface IUnitOfWork
    {
        IProductRepository Products { get; }

        Task CommitAsync();
    }
}
