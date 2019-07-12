using System.Threading.Tasks;

namespace fAzureHelper
{
    public interface IUserService
    {
         Task RegisterAsync(string email, string password, string name);
         JsonWebToken LoginAsync(string email, string password);
    }
}