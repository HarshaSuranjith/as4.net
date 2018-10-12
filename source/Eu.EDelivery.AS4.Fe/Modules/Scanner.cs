﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Modules
{
    public class Scanner
    {
        public Scanner Register(Type baseType, IServiceCollection services, IList<Type> baseAssembly, IList<TypeInfo> modules, IDictionary<string, string> settings = null, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
        {
            var localModules = baseAssembly
               .Where(x => baseType.IsAssignableFrom(x) && x.IsClass && x != baseType)
               .Select(x => new
               {
                   Iface = baseType,
                   LocalImplementation = x,
                   FromConfig = settings == null ? string.Empty : settings.Where(cfg => cfg.Key == x.FullName).Select(cfg => cfg.Value).FirstOrDefault()
               });

            foreach (var local in localModules)
            {
                if (string.IsNullOrEmpty(local.FromConfig) || local.FromConfig == "default")
                {
                    // Check if there is a moduleType
                    var moduleType = modules.GetImplementation(local.Iface);
                    if (moduleType != null)
                    {
                        // Found a moduleType, register it
                        RegisterType(services, local.Iface, moduleType.AsType(), lifeTime);
                        continue;
                    }

                    RegisterType(services, local.Iface, local.LocalImplementation, lifeTime);
                }
                else
                {
                    var fromConfig = local.FromConfig;
                    var assembly = modules.FirstOrDefault(mod => mod.Assembly.FullName.Split(',')[0] == fromConfig);
                    if (assembly == null) throw new Exception($"Could not find assembly {fromConfig}, please check the configuration");
                    RegisterType(services, local.Iface, baseAssembly, lifeTime);
                }
            }

            return this;
        }

        public Scanner Register<TType>(IServiceCollection services, IList<TypeInfo> baseAssembly, IList<TypeInfo> modules, IDictionary<string, string> settings = null, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
        {
            return Register(typeof(TType), services, baseAssembly.Select(i => i.AsType()).ToList(), modules, settings, lifeTime);
        }

        private void RegisterType(IServiceCollection services, Type baseType, Type implementation, ServiceLifetime lifetime)
        {
            services.Add(new ServiceDescriptor(baseType, implementation, lifetime));

            // If the implementation has lifecycle hooks implemented then register these
            foreach (var hook in implementation.GetInterfaces().Where(iface => typeof(ILifeCycleHook).IsAssignableFrom(iface) && iface != typeof(ILifeCycleHook) && iface != baseType && iface != typeof(IModular)))
            {
                services.Add(new ServiceDescriptor(hook, implementation, ServiceLifetime.Transient));
            }
        }

        private bool RegisterType(IServiceCollection services, Type baseType, IList<Type> types, ServiceLifetime lifetime)
        {
            var searchFor = types.FirstOrDefault(typ => typ.GetInterfaces().Any(baseType.IsAssignableFrom));
            if (searchFor == null) return false;

            RegisterType(services, baseType, searchFor, lifetime);
            return true;
        }
    }
}