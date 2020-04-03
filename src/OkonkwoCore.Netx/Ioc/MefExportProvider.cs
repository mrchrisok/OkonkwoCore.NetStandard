using OkonkwoCore.Netx.Contracts;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Threading.Tasks;

namespace OkonkwoCore.Netx.Ioc
{
    [Export("ExportProvider", typeof(IComponentResolver))]
    [PartCreationPolicy(CreationPolicy.Shared)] // singleton
    public class MefExportProvider : IComponentResolver
    {
        protected ExportProvider _container;

        public MefExportProvider()
        {
        }

        public MefExportProvider(ExportProvider container)
        {
            _container = container;
        }

        public void RegisterContainer(object container)
        {
            _container = (ExportProvider)container;
        }

        public TEntity Resolve<TEntity>()
        {
            return _container.GetExportedValue<TEntity>();
        }

        public TEntity Resolve<TEntity>(string contractName)
        {
            return _container.GetExportedValue<TEntity>(contractName);
        }

        public Task<TEntity> ResolveAsync<TEntity>()
        {
            return Task.FromResult(_container.GetExportedValue<TEntity>());
        }

        public Task<TEntity> ResolveAsync<TEntity>(string contractName)
        {
            return Task.FromResult(_container.GetExportedValue<TEntity>(contractName));
        }
    }
}
