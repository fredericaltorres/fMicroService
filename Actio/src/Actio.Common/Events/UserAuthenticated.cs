using System;
using System.Collections.Generic;
using System.Text;

namespace Actio.Common.Events
{
    public class UserAuthenticated : IEvent
    {
        public string Email { get; }

        protected UserAuthenticated() // For serializer
        {
        }

        public UserAuthenticated(string email)
        {
            this.Email = email;
        }
    }
}