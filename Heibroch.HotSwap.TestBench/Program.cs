using Heibroch.HotSwapper;
using SomeGatewayAPI.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Heibroch.HotSwap.TestBench
{
    class Program
    {
        static int authenticationCounter = 0;
        static int cashTransferCounter = 0;
        static int secondInterval = 5;

        private static HotSwappedObject<IBankConnection> hotSwappedObject;

        static void Main(string[] args)
        {
            Console.WriteLine("Subscribing to path");            
            var currentDirectory = Path.Combine(Environment.CurrentDirectory, "Gateways\\DanskeBank");

            if (!Directory.Exists(currentDirectory))
                Console.WriteLine("ERROR: Directory location does not exist");

            hotSwappedObject = new HotSwappedObject<IBankConnection>(currentDirectory, 1);

            Console.WriteLine("\r\nStarting test run...\r\n");

            Thread.Sleep(2000);

            Task.Run(() =>
            {
                while(true)
                {
                    hotSwappedObject.PerformOperation(x => x.Authenticate());
                    authenticationCounter++;
                }                
            });

            Task.Run(() =>
            {
                while (true)
                {
                    hotSwappedObject.PerformOperation(x => x.TransferCash());
                    cashTransferCounter++;
                }
            });

            var timer = new Timer(OnTimerTick, null, 0, secondInterval * 1000);

            Console.ReadLine();
        }

        private static void OnTimerTick(object state)
        {
            Console.WriteLine($"---------------------{DateTime.Now}-------------------------");
            Console.WriteLine($"Authentications: {authenticationCounter / secondInterval}/s");
            Console.WriteLine($"Cash transfers:  {cashTransferCounter / secondInterval}/s");

            authenticationCounter = 0;
            cashTransferCounter = 0;
        }
    }
}
