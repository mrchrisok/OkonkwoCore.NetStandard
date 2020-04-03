namespace OkonkwoCore.Netx.Contracts
{
    public interface IRepositoryFactory
    {
        TEntity GetRepository<TEntity>() where TEntity : IRepository;
    }
}
