using System;
using System.Net.Http;
using System.Threading.Tasks;
using ECommerceApp.Core.Interfaces;

namespace ECommerceApp.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        public async Task<bool> ValidatePaymentAsync(string orderId)
        {
            Console.WriteLine($"Simulando validación de pago para orden: {orderId}");
            await Task.Delay(500); // Simula latencia de red
            return true; // Simula pago exitoso
        }
    }
}

