using System;
using System.Collections.Generic;
using System.Text;

namespace Actio.Common.Commands
{
    // Just a marker interface
    interface IAuthenticatedComand: ICommand
    {
        Guid UserId { get; set; }
    }
}
