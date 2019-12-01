﻿using System;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Tracking;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Messaging.Transports.Local
{
    public class local_integration_specs : IntegrationContext
    {
        private readonly MessageTracker theTracker = new MessageTracker();

        public local_integration_specs(DefaultApp @default) : base(@default)
        {
        }

        private void configure()
        {
            with(_ =>
            {
                _.Publish.Message<Message1>()
                    .ToLocalQueue("incoming");

                _.Services.AddSingleton(theTracker);

                _.Services.Scan(x =>
                {
                    x.TheCallingAssembly();
                    x.WithDefaultConventions();
                });
            });
        }


        [Fact]
        public async Task send_a_message_and_get_the_response()
        {
            configure();

            var bus = Host.Get<IMessageContext>();

            var waiter = theTracker.WaitFor<Message1>();

            await bus.Send(new Message1());

            await waiter;

            if (!waiter.IsCompleted) throw new Exception("Got no envelope!");

            var envelope = waiter.Result;

            envelope.ShouldNotBeNull();
        }
    }
}