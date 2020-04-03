using OkonkwoCore.Netx.Contracts;
using System.ComponentModel.Composition;

namespace OkonkwoCore.Netx.Data
{
    [Export(typeof(IRepositoryFactory))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryFactory : IRepositoryFactory
    {
        private IComponentResolver _container;

        [ImportingConstructor]
        public RepositoryFactory(IComponentResolver container)
        {
            _container = container;
        }

        TEntity IRepositoryFactory.GetRepository<TEntity>()
        {
            return _container.Resolve<TEntity>();
        }
    }
}
