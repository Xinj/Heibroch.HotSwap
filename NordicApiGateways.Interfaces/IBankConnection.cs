namespace NordicApiGateway.Interfaces
{
    public interface IBankConnection
    {
        int TransferCash();
        int Authenticate();
        int GetPayments();
    }
}
