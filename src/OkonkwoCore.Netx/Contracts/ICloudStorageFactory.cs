using Microsoft.WindowsAzure.Storage.Table;

namespace OkonkwoCore.Netx.Contracts
{
    public interface ICloudStorageFactory
    {
        CloudTable GetCloudTable(string tableName);
    }

}
