using Actio.Common.Commands;
using Actio.Common.Events;
using RawRabbit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Actio.Api.Handlers
{
    public class ActivityCreateHandler : ICommandHandler<CreateActivity>
    {
        private readonly IBusClient _busClient;
        public ActivityCreateHandler(IBusClient busClient)
        {
            this._busClient = busClient;
        }
        public async Task HandleAsync(CreateActivity command)
        {
            Console.WriteLine($"Creating activity for {command.Name}");
            await this._busClient.PublishAsync(
                new ActivityCreated(
                    command.Id, command.UserId, command.Category, command.Name, 
                    command.Description, command.CreatedAt
                )
            );
        }
    }
}
