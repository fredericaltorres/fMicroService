using System;
using System.Collections.Generic;
using System.Text;

namespace Actio.Common.Events
{
    public class UserCreated : IEvent
    {
        public string Email { get;  }
        public string Name { get;  }


        protected UserCreated() // Just created for the serializer
        {

        }
        public UserCreated(string email, string name)
        {
            this.Email = email;
            this.Name = name;
        }
    }
}
