using Donation.Model;
using DynamicSugar;
using fAzureHelper;
using System;
using System.Threading.Tasks;

namespace Donation.Table.Lib
{
    public class DonationAzureTableRecord : TableRecordManager
    {
        public Guid Guid;
        public string FirstName;
        public string LastName;
        public string Email;
        public Gender Gender;
        public string Phone;
        public string Country;
        public string ZipCode;

        public string CC_Number;
        public int CC_ExpMonth;
        public int CC_ExpYear;
        public int CC_SecCode;

        public string IpAddress;
        public string Amount; //$15.92
        public DateTime UtcCreationDate;

        public DonationDataProcessState ProcessState = DonationDataProcessState.New;

        public DonationDTOValidationErrors Set(DonationDTO fromDonationDTO)
        {
            var r = new DonationDTOValidationErrors();
            var dic = ReflectionHelper.GetDictionary(fromDonationDTO);
            foreach(var e in dic)
            {
                try
                {
                    ReflectionHelper.SetProperty(this, e.Key, e.Value);
                }
                catch(System.Exception ex)
                {
                    r.Add(new DonationDTOValidationError(fromDonationDTO.Guid, e.Key, $"Cannot copy property {e.Key} from DonationDTO to DonationAzureTableRecord - ex:{ex}"));
                }
            }
            return r;
        }

        public void SetIdentification()
        {
            base.SetIdentification(this.Country, this.Guid.ToString());
        }
    }

    public class DonationTableManager
    {
        public const string TABLE_NAME = "Donation";

        TableManager _tableManager;
        public DonationTableManager(string storageAccountName, string storageAccessKey) 
        {
            _tableManager = new TableManager(storageAccountName, storageAccessKey, TABLE_NAME);
        }

        public async Task<DonationDTOValidationErrors> InsertAsync(DonationAzureTableRecord entity)
        {
            var r = new DonationDTOValidationErrors();
            try
            {
                await _tableManager.InsertAsync(entity);
            }
            catch(System.Exception ex)
            {
                r.Add(new DonationDTOValidationError(entity.Guid, $"Cannot insert donation {entity.Guid} in azure table ${TABLE_NAME} - ex:{ex}"));
            }
            return r;
        }
    }
}
