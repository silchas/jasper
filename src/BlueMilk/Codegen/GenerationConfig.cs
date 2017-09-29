﻿using System.Collections.Generic;
using System.Reflection;
using BlueMilk.IoC;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.Codegen
{
    public class GenerationConfig
    {
        public GenerationConfig(string applicationNamespace)
        {
            ApplicationNamespace = applicationNamespace;
        }

        public string ApplicationNamespace { get; }

        public readonly IList<IVariableSource> Sources = new List<IVariableSource>();

        public readonly IList<Assembly> Assemblies = new List<Assembly>();

        public void ReadServices(IServiceCollection services)
        {
            Services = new ServiceGraph(services);
        }

        public ServiceGraph Services { get; private set; } = new ServiceGraph(new ServiceRegistry());
    }


}