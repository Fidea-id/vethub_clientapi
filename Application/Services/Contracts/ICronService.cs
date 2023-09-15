namespace Application.Services.Contracts
{
    public interface ICronService
    {
        Task<string> CheckAppointmentsStatusDaily(string dbName);
        //Task<InvoicesDTO> SetupInvoice(InvoicesDTO invoice);
    }
}
