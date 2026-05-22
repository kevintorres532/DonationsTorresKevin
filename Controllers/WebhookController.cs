using Microsoft.AspNetCore.Mvc;
using DonationsTorresKevin.Data;
using Stripe;


namespace DonationsTorresKevin.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WebhookController(
    IConfiguration config,
    IServiceProvider serviceProvider) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Index()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var endpointSecret = config["Stripe:WebhookSecret"];

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                endpointSecret
            );

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                var donationIdStr = session?.ClientReferenceId;

                if (int.TryParse(donationIdStr, out int donationId))
                {
                    using var scope = serviceProvider.CreateScope();

                    var dbContext = scope.ServiceProvider
                        .GetRequiredService<ApplicationDbContext>();

                    var donation = await dbContext.Donations.FindAsync(donationId);

                    if (donation != null && donation.Status != "Completed")
                    {
                        donation.Status = "Completed";
                        donation.StripeSessionId = session?.Id;

                        await dbContext.SaveChangesAsync();
                    }
                }
            }

            return Ok();
        }
        catch (StripeException)
        {
            return BadRequest();
        }
    }
}