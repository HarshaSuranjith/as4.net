﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.ServiceHandler.Agents;

namespace Eu.EDelivery.AS4.ServiceHandler.ConsoleHost
{
    public class Program
    {
        public static void Main()
        {
            Kernel kernel = CreateKernel();

            var cancellationTokenSource = new CancellationTokenSource();
            Task task = kernel.StartAsync(cancellationTokenSource.Token);
            task.ContinueWith(
                x =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(x.Exception?.ToString());
                },
                TaskContinuationOptions.OnlyOnFaulted);

            Console.ReadLine();
            Console.WriteLine("Stopping...");
            cancellationTokenSource.Cancel();

            task.GetAwaiter().GetResult();
            Console.WriteLine($"Stopped: {task.Status}");

            if (task.IsFaulted && task.Exception != null)
                Console.WriteLine(task.Exception.ToString());

            Console.ReadLine();
        }

        private static Kernel CreateKernel()
        {
            Config config = Config.Instance;
            Registry registry = Registry.Instance;

            config.Initialize();
            registry.CertificateRepository = new CertificateRepository(config);
            registry.DatastoreRepository = new DatastoreRepository(() => new DatastoreContext(config));

            var agentProvider = new AgentFactory(config);
            return new Kernel(agentProvider.CreateAgents());
        }
    }
}