﻿using IntegrationTests;
using Jasper.Configuration;
using Jasper.Persistence.Marten;
using Jasper.Persistence.Testing.Marten.Persistence.Resiliency;
using Jasper.Tracking;
using Marten;

namespace Jasper.Persistence.Testing.Marten.Persistence
{
    public class ItemSender : JasperOptions
    {
        public ItemSender()
        {

            Endpoints.Publish(x =>
            {
                x.Message<ItemCreated>();
                x.Message<Question>();
                x.ToPort(2345).Durably();
            });

            Extensions.UseMarten(x =>
            {
                x.Connection(Servers.PostgresConnectionString);
                x.DatabaseSchemaName = "sender";
            });

            Extensions.UseMessageTrackingTestingSupport();

            Endpoints.ListenAtPort(2567);

        }
    }
}
