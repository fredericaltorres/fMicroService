using System.Threading.Tasks;

namespace fAzureHelper
{
    public interface IUserService
    {
        Task RegisterAsync(string email, string password, string name);
        User Login(string email, string password);
        JsonWebToken GetWebToken(User user);
    }
}