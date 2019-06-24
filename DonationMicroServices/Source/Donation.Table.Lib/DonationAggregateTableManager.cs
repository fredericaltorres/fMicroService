using Donation.Model;
using fAzureHelper;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;

namespace Donation.Table.Lib
{
    public class DonationAggregateTableManager
    {
        public const string TABLE_NAME = "DonationAggregateTable";

        TableManager _tableManager;
        public DonationAggregateTableManager(string storageAccountName, string storageAccessKey) 
        {
            _tableManager = new TableManager(storageAccountName, storageAccessKey, TABLE_NAME);
        }

        public async Task DeleteAsync()
        {
            await this._tableManager.DeleteAsync();
        }

        public async Task<Errors> InsertAsync(DonationAggregateAzureTableRecord entity)
        {
            var r = new Errors();
            try
            {
                await _tableManager.InsertAsync(entity);
            }
            catch(System.Exception ex)
            {
                r.Add(new Error($"Cannot insert DonationAggregateAzureTableRecord {entity.Guid} in azure table ${TABLE_NAME} - ex:{ex.Message}", ex));
            }
            return r;
        }
    }
}
