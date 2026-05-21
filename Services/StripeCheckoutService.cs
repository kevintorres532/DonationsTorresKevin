using Stripe.Checkout;

namespace DonationsTorresKevin.Services;

public class StripeCheckoutService
{
    public async Task<string> CreateCheckoutSessionAsync(
        decimal amount,
        string currency,
        string donationId,
        string successUrl,
        string cancelUrl)
    {
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            LineItems =
            [
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        // Stripe espera el monto en la denominación más baja
                        // Por lo tanto, multiplicamos por 100. (10.00 USD = 1000 centavos)
                        UnitAmountDecimal = amount * 100,
                        Currency = currency.ToLower(),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Donation",
                            Description = "Thank you for supporting our cause!"
                        }
                    },
                    Quantity = 1,
                }
            ],
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,

            // Se usa ClientReferenceId para enviar el Donation.Id a Stripe,
            // se recibe luego en el webhook
            ClientReferenceId = donationId
        };

        var service = new SessionService();
        Session session = await service.CreateAsync(options);

        return session.Url;
    }
}