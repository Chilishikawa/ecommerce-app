namespace ECommerceApp.Core.Interfaces
{
    public interface IPaymentService
    {
        Task<bool> ValidatePaymentAsync(string orderId);
    }
}