using NordicApiGateway.Interfaces;
using System;
using System.Reflection;
using System.Threading;

namespace NordicApiGateway.Gateways.DaskeBank
{
    public class BankConnection : IBankConnection
    {
        private int authenticationRequests = 0;
        private int paymentRequests = 0;
        private int cashTransfers = 0;

        public int Authenticate() => DoStuff(ref authenticationRequests, $"{ExecutingAssemblyName}: Authentication operation performed");
        public int GetPayments() => DoStuff(ref paymentRequests, $"{ExecutingAssemblyName}: payments operation performed");
        public int TransferCash() => DoStuff(ref cashTransfers, $"{ExecutingAssemblyName}: transfer operation performed");

        private int DoStuff(ref int param, string message)
        {
            param++;
            //Thread.Sleep(10); //Simulate load
            return param;
        }

        private static string ExecutingAssemblyName => Assembly.GetExecutingAssembly().GetName().Name;
    }
}
