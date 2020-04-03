using OkonkwoCore.Netx.Contracts;
using Autofac;
using System.Threading.Tasks;

namespace OkonkwoCore.Netx.Ioc
{
    public class AutofacResolver : IComponentResolver
    {
        protected ILifetimeScope _container;

        public AutofacResolver()
        {
        }
        public AutofacResolver(ILifetimeScope container)
        {
            _container = container;
        }

        public void RegisterContainer(object container)
        {
            _container = (ILifetimeScope)container;
        }

        public TEntity Resolve<TEntity>()
        {
            return _container.Resolve<TEntity>();
        }

        public TEntity Resolve<TEntity>(string contractName)
        {
            return _container.ResolveNamed<TEntity>(contractName);
        }

        public Task<TEntity> ResolveAsync<TEntity>()
        {
            return Task.FromResult(_container.Resolve<TEntity>());
        }

        public Task<TEntity> ResolveAsync<TEntity>(string contractName)
        {
            return Task.FromResult(_container.ResolveNamed<TEntity>(contractName));
        }
    }
}
