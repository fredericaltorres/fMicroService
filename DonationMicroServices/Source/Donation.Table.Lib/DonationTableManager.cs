﻿using Donation.Model;
using fAzureHelper;
using System.Threading.Tasks;

namespace Donation.Table.Lib
{
    public class DonationTableManager
    {
        public const string TABLE_NAME = "DonationTable";

        TableManager _tableManager;
        public DonationTableManager(string storageAccountName, string storageAccessKey) 
        {
            _tableManager = new TableManager(storageAccountName, storageAccessKey, TABLE_NAME);
        }

        public async Task<Errors> InsertAsync(DonationAzureTableRecord entity)
        {
            var r = new Errors();
            try
            {
                await _tableManager.InsertAsync(entity);
            }
            catch(System.Exception ex)
            {
                r.Add(new Error($"Cannot insert donation {entity.Guid} in azure table ${TABLE_NAME} - ex:{ex}"));
            }
            return r;
        }
    }
}