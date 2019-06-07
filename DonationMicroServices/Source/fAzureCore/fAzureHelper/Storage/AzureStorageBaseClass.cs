using Microsoft.WindowsAzure.Storage;
using System;

namespace fAzureHelper
{
    public class AzureStorageBaseClass
    {
        protected const string ConnectionStringFormat = "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}";
        protected string _storageAccountName;
        protected string _storageAccessKey;

        protected CloudStorageAccount _storageAccount = null;

        protected string GetConnectString()
        {
            return string.Format(ConnectionStringFormat, this._storageAccountName, this._storageAccessKey);
        }
        public AzureStorageBaseClass(string storageAccountName, string storageAccessKey)
        {
            this._storageAccountName = storageAccountName.ToLowerInvariant();
            this._storageAccessKey = storageAccessKey;

            if (!CloudStorageAccount.TryParse(GetConnectString(), out _storageAccount))
                throw new ApplicationException("Cannot parse connection string");

        }
    }
}
