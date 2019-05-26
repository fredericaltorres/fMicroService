using System;
using System.Collections.Generic;
using System.Text;

namespace Actio.Common.Events
{
    // Just a marker interface
    public interface IAuthenticatedEvent : IEvent
    {
        Guid UserId { get; set; }
    }
}
