using Actio.Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Actio.Api.Handlers
{
    public class ActivityCreateHandler : IEventHandler<ActivityCreated>
    {
        public async Task HandleAsync(ActivityCreated @event)
        {
            await Task.CompletedTask;
            Console.WriteLine($"Activity Created {@event.Name}");
        }
    }
}
