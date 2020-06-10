namespace SomeGatewayAPI.Interfaces
{
    public interface IBankConnection
    {
        int TransferCash();
        int Authenticate();
        int GetPayments();
    }
}
