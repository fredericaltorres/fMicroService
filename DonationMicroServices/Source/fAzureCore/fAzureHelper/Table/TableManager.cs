using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace fAzureHelper
{
    public class TableRecordManager : TableEntity
    {
        public void SetIdentification(string Id, string partition = "all")
        {
            this.RowKey = Id;
            this.PartitionKey = partition;
        }
    }

    public class FileLogHistoryAzureTableRecord : TableRecordManager
    {
        public string FileName { get; set; }
        public DateTime CreationTime { get; set; }
        public string ComputerOrigin { get; set; }

        public void SetIdentification()
        {
            base.SetIdentification(this.FileName, this.ComputerOrigin);
        }
    }

    // https://vkinfotek.com/azureqa/how-do-i-query-azure-table-storage-using-tablequery-class.html
    public class TableManager : AzureStorageBaseClass
    {
        public string TableName;

        CloudTableClient _tableClient;
        CloudTable _table;

        public TableManager(string storageAccountName, string storageAccessKey, string tabkeName) : base(storageAccountName, storageAccessKey)
        {
            this.TableName = tabkeName.ToLowerInvariant();
            _tableClient = _storageAccount.CreateCloudTableClient();

            _table = _tableClient.GetTableReference(this.TableName);
            _table.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        }

        public async Task InsertAsync(ITableEntity entity)
        {
            var op = TableOperation.Insert(entity);
            await _table.ExecuteAsync(op);
        }

        public class WhereClauseExpression
        {
            public string Comparator = QueryComparisons.Equal;
            public string Name;
            public string Value;

            public string GenerateFilterCondition()
            {
                var r = TableQuery.GenerateFilterCondition(this.Name, this.Comparator, this.Value);
                return r;
            }

            public string CombineFilters<T>(TableQuery<T> query) where T : ITableEntity, new()
            {
                return TableQuery.CombineFilters(
                        query.FilterString,
                        TableOperators.And,
                        this.GenerateFilterCondition()
                    );
                }
        }

        public async Task<IList<T>> GetRecords<T>(string partition, string rowKey = null, WhereClauseExpression extra = null) where T : ITableEntity, new()
        {
            var items = new List<T>();
            TableContinuationToken token = null;
            CancellationToken ct = default(CancellationToken);

            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partition));
            if(rowKey != null)
            {
                query = new TableQuery<T>().Where(
                    (new WhereClauseExpression() { Name = "RowKey", Value = rowKey }).CombineFilters(query)                 
                );
            }
            if(extra != null)
            {
                query = new TableQuery<T>().Where(
                    extra.CombineFilters(query)
                );
            }

            return await GetRecords(query);         
        }

        public async Task<IList<T>> GetRecords<T>(TableQuery<T> query) where T : ITableEntity, new()
        {
            var items = new List<T>();
            TableContinuationToken token = null;
            CancellationToken ct = default(CancellationToken);

            do
            {
                TableQuerySegment<T> seg = await _table.ExecuteQuerySegmentedAsync<T>(query, token);
                token = seg.ContinuationToken;
                items.AddRange(seg);

            } while (token != null && !ct.IsCancellationRequested);

            return items;
        }
    }
}
