using System;
using System.Threading.Tasks;

namespace fAzureHelper
{
    public class UserService : IUserService
    {
        //private readonly IUserRepository _repository;
        private readonly IEncrypter _encrypter;
        private readonly IJwtHandler _jwtHandler;

        public UserService(// IUserRepository repository,
            IEncrypter encrypter,
            IJwtHandler jwtHandler)
        {
            // _repository = repository;
            _encrypter = encrypter;
            _jwtHandler = jwtHandler;
        }

        public async Task RegisterAsync(string email, string password, string name)
        {
            //var user = await _repository.GetAsync(email);
            //if (user != null)
            //{
            //    throw new Exception($"email_in_use - Email: '{email}' is already in use.");
            //}
            //user = new User(email, name);
            //user.SetPassword(password, _encrypter);
            //await _repository.AddAsync(user);
        }
   
        public User Login(string email, string password)
        {
            var user = new User(email, email);
            user.SetPassword(password, _encrypter);
            //var user = await _repository.GetAsync(email);
            //if (user == null)
            //    throw new Exception("invalid_credentials- Invalid credentials.");

            if (!user.ValidatePassword(password, _encrypter))
                throw new Exception("invalid_credential - Invalid credentials.");

            return user;
        }

        public JsonWebToken GetWebToken(User user)
        {
            return _jwtHandler.Create(user.Id);
        }
    }
}