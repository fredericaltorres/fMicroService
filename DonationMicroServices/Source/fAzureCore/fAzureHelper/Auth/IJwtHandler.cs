using System;

namespace fAzureHelper
{
    public interface IJwtHandler
    {
        JsonWebToken Create(Guid userId);     
    }
}