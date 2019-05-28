using System;
using System.Reflection;
using System.Threading.Tasks;
using Actio.Common.Commands;
using Actio.Common.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RawRabbit;
using RawRabbit.Instantiation;
using RawRabbit.Pipe;

namespace Actio.Common.RabbitMq
{
    public static class Extensions
    {
        public static Task WithCommandHandlerAsync<TCommand>(
            this IBusClient bus,

            ICommandHandler<TCommand> handler) where TCommand : ICommand
        {
            return bus.SubscribeAsync<TCommand>(
                msg => { return handler.HandleAsync(msg); },
                context =>
                {
                    context.UseConsumerConfiguration(
                        config => config.FromDeclaredQueue(
                            queue => queue.WithName(GetQueueName<TCommand>())
                        )
                    );
                }
            );
        }

        public static Task WithEventHandlerAsync<TEvent>(
            this IBusClient bus,
            IEventHandler<TEvent> handler) where TEvent : IEvent
        {
            return bus.SubscribeAsync<TEvent>(
                msg => { return handler.HandleAsync(msg); },
                context =>
                {
                    context.UseConsumerConfiguration(
                        config => config.FromDeclaredQueue(
                            queue => queue.WithName(GetQueueName<TEvent>())
                        )
                    );
                }
            );
        }

        /// <summary>
        /// Everybody use the same queue, 2 queues one for event one for command
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static string GetQueueName<T>()
        {
            return $"{Assembly.GetEntryAssembly().GetName()}/{typeof(T).Name}";
        }

        /// <summary>
        /// https://rawrabbit.readthedocs.io/en/master/configuration.html
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            var options = new RabbitMqOptions();
            var section = configuration.GetSection("rabbitmq");
                                                 
            section.Bind(options);            
            var client = RawRabbitFactory.CreateSingleton( // Only one instance manager the connection to RabbitMQ
                new RawRabbitOptions
                {
                    ClientConfiguration = options
                }
            );
            Console.WriteLine($"RabbitMQ Folder {client.GetType().Assembly.Location}");

            var newtonsoftJsonAssembly = (new JObject()).GetType().Assembly;
            Console.WriteLine($" Newtonsoft.Json folder {newtonsoftJsonAssembly.Location}");
            
            services.AddSingleton<IBusClient>( _ => client );
        }
    }
}