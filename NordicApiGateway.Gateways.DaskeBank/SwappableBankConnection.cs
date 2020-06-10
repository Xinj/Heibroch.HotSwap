using NordicApiGateway.Gateways.DaskeBank;
using NordicApiGateway.Interfaces;

namespace NordicApiGateway.Gateways.DanskeBank
{
    public class SwappableBankConnection
    {
        private IBankConnection bankConnection;

        public SwappableBankConnection() => bankConnection = new BankConnection();

        public object Object => bankConnection;
    }
}
