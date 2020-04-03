using OkonkwoCore.Netx.Contracts;
using System;
using System.ComponentModel.Composition;
using System.Composition.Hosting;
using System.Threading.Tasks;

namespace OkonkwoCore.Netx.Ioc
{
    [Export("CompositionHost", typeof(IComponentResolver))]
    [PartCreationPolicy(CreationPolicy.Shared)] // singleton
    public class MefCompositionHost : IComponentResolver, IServiceProvider
    {
        protected CompositionHost _container;

        public MefCompositionHost()
        {
        }

        public MefCompositionHost(CompositionHost container)
        {
            _container = container;
        }

        public void RegisterContainer(object container)
        {
            _container = (CompositionHost)container;
        }

        public TEntity Resolve<TEntity>()
        {
            return _container.GetExport<TEntity>();
        }

        public TEntity Resolve<TEntity>(string contractName)
        {
            return _container.GetExport<TEntity>(contractName);
        }

        public Task<TEntity> ResolveAsync<TEntity>()
        {
            return Task.FromResult(_container.GetExport<TEntity>());
        }

        public Task<TEntity> ResolveAsync<TEntity>(string contractName)
        {
            return Task.FromResult(_container.GetExport<TEntity>(contractName));
        }

        public object GetService(Type serviceType)
        {
            return _container.GetExport(serviceType);
        }
    }
}
