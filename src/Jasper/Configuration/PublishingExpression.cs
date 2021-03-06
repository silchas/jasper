﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using Jasper.Runtime.Routing;
using Jasper.Transports;
using Jasper.Transports.Local;
using Jasper.Transports.Tcp;
using Jasper.Util;

namespace Jasper.Configuration
{
    public class PublishingExpression : IPublishToExpression
    {
        private readonly IList<Subscription> _subscriptions = new List<Subscription>();

        private readonly IList<Subscriber> _subscribers = new List<Subscriber>();

        internal PublishingExpression(TransportCollection parent)
        {
            Parent = parent;
        }

        public TransportCollection Parent { get; }

        internal bool AutoAddSubscriptions { get; set; }


        public PublishingExpression Message<T>()
        {
            return Message(typeof(T));
        }

        public PublishingExpression Message(Type type)
        {
            _subscriptions.Add(Subscription.ForType(type));
            return this;
        }

        public PublishingExpression MessagesFromNamespace(string @namespace)
        {
            _subscriptions.Add(new Subscription
            {
                Match = @namespace,
                Scope = RoutingScope.Namespace
            });

            return this;
        }

        public PublishingExpression MessagesFromNamespaceContaining<T>()
        {
            return MessagesFromNamespace(typeof(T).Namespace);
        }

        public PublishingExpression MessagesFromAssembly(Assembly assembly)
        {
            _subscriptions.Add(new Subscription(assembly));
            return this;
        }

        public PublishingExpression MessagesFromAssemblyContaining<T>()
        {
            return MessagesFromAssembly(typeof(T).Assembly);
        }


        /// <summary>
        /// All matching records are to be sent to the configured subscriber
        /// by Uri
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public ISubscriberConfiguration To(Uri uri)
        {
            var endpoint = Parent.GetOrCreateEndpoint(uri);

            _subscribers.Add(endpoint);

            if (AutoAddSubscriptions)
            {
                endpoint.Subscriptions.AddRange(_subscriptions);
            }

            return new SubscriberConfiguration(endpoint);
        }

        /// <summary>
        /// Use a routing rule "Subscriber" as a recipient. This is used
        /// by Jasper's topic routing
        /// </summary>
        /// <param name="subscriber"></param>
        public void ViaRouter(Subscriber subscriber)
        {
            if (AutoAddSubscriptions)
            {
                subscriber.Subscriptions.AddRange(_subscriptions);
            }

            Parent.Subscribers.Fill(subscriber);
            _subscribers.Add(subscriber);
        }

        /// <summary>
        /// Send all the matching messages to the designated Uri string
        /// </summary>
        /// <param name="uriString"></param>
        /// <returns></returns>
        public ISubscriberConfiguration To(string uriString)
        {
            return To(uriString.ToUri());
        }

        /// <summary>
        ///     Publishes the matching messages locally to the default
        ///     local queue
        /// </summary>
        public IListenerConfiguration Locally()
        {

            var settings = Parent.Get<LocalTransport>().QueueFor(TransportConstants.Default);
            settings.Subscriptions.AddRange(_subscriptions);

            return new ListenerConfiguration(settings);
        }

        /// <summary>
        ///     Publish the designated message types using Jasper's lightweight
        ///     TCP transport locally to the designated port number
        /// </summary>
        /// <param name="port"></param>
        public ISubscriberConfiguration ToPort(int port)
        {
            var uri = TcpEndpoint.ToUri(port);
            return To(uri);
        }

        /// <summary>
        ///     Publish the designated message types to the named
        ///     local queue
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public IListenerConfiguration ToLocalQueue(string queueName)
        {
            var settings = Parent.Get<LocalTransport>().QueueFor(queueName);

            if (AutoAddSubscriptions)
            {
                settings.Subscriptions.AddRange(_subscriptions);
            }

            _subscribers.Add(settings);

            return new ListenerConfiguration(settings);
        }

        /// <summary>
        ///     Publish messages using the TCP transport to the specified
        ///     server name and port
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="port"></param>
        public ISubscriberConfiguration ToServerAndPort(string hostName, int port)
        {
            var uri = TcpEndpoint.ToUri(port, hostName);
            return To(uri);
        }

        internal void AttachSubscriptions()
        {
            if (!_subscribers.Any())
            {
                throw new InvalidOperationException("No subscriber endpoint(s) are specified!");
            }

            foreach (var endpoint in _subscribers)
            {
                endpoint.Subscriptions.AddRange(_subscriptions);
            }
        }

        internal void AddSubscriptionForAllMessages()
        {
            _subscriptions.Add(Subscription.All());
        }
    }
}
