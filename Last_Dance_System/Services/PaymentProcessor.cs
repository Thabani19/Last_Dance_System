using System;
using System.Configuration;
using System.Threading.Tasks;
using Stripe;

namespace Last_Dance_System.Services
{
    public class PaymentProcessor
    {
        private readonly string _apiKey;
        public readonly bool Simulate;

        public PaymentProcessor()
        {
            Simulate = bool.TryParse(ConfigurationManager.AppSettings["Stripe:Simulate"], out var s) ? s : true;
            _apiKey = ConfigurationManager.AppSettings["Stripe:ApiKey"] ?? "";
            if (!Simulate && !string.IsNullOrWhiteSpace(_apiKey))
            {
                StripeConfiguration.ApiKey = _apiKey;
            }
        }

        // Returns (success, transactionId, message)
        public async Task<(bool, string, string)> ProcessPaymentAsync(decimal amount, string currency, string token, string description)
        {
            if (Simulate)
            {
                await Task.Delay(250);
                return (true, $"SIM-{Guid.NewGuid():N}", "Simulated payment succeeded");
            }

            try
            {
                var options = new ChargeCreateOptions
                {
                    Amount = (long)(amount * 100m),
                    Currency = currency ?? "usd",
                    Description = description,
                    Source = token
                };

                var service = new ChargeService();
                var charge = await service.CreateAsync(options);
                var succeeded = string.Equals(charge.Status, "succeeded", StringComparison.OrdinalIgnoreCase);
                return (succeeded, charge.Id, charge.FailureMessage ?? charge.Outcome?.SellerMessage ?? "");
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}