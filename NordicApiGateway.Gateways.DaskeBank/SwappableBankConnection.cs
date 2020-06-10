using SomeGatewayAPI.Gateways.DaskeBank;
using SomeGatewayAPI.Interfaces;

namespace SomeGatewayAPI.Gateways.DanskeBank
{
    public class SwappableBankConnection
    {
        private IBankConnection bankConnection;

        public SwappableBankConnection() => bankConnection = new BankConnection();

        public object Object => bankConnection;
    }
}
