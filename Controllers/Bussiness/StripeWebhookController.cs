using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PototoTrade.Controllers.CustomerController;
using Stripe;
using Stripe.Checkout;
using PototoTrade.Service.Wallet;
using PototoTrade.Models.User;
using System.Security.Claims;
using Newtonsoft.Json;

namespace PototoTrade.Controllers
{
    [Route("api/stripe/public")]
    [ApiController]
    public class StripeWebHook : CustomerBaseController
    {
        //whsec_59e473ce86102764c7245c1afc108486d618a69b15b413b2b2ed2e31e6cd9aa6
        
        private const string WebhookSecret = "whsec_Nr8fpvSxbxrmmLRrfQoO7TfBHFBGTNZF"; //if doesnt work in the future, generate new ngrok url, renew ngrok url in stripe website and retrieve the new secret
        private readonly UserWalletService _userWalletService;

        public StripeWebHook(UserWalletService userWalletService){
            _userWalletService = userWalletService;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            
            // Read the request body from Stripe
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                Console.WriteLine("Received request: " + json);

            foreach (var header in Request.Headers)
                {
                    Console.WriteLine($"{header.Key}: {header.Value}");
                }
            Console.WriteLine(HttpContext.User.ToString());
            try
            {
                //Validate the Stripe event using the secret
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    WebhookSecret,
                    throwOnApiVersionMismatch: false
                );
                // var stripeEvent = JsonConvert.DeserializeObject<Event>(json);

                // Handle the event based on its type
                if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
                {
                    Console.WriteLine("Processing CheckoutSessionCompleted event...");

                    // Cast event data to a Stripe Session object
                    var session = stripeEvent.Data.Object as Session;
                    if (session?.Metadata == null || !session.Metadata.ContainsKey("userId"))
                    {
                        Console.WriteLine("Metadata is missing or userId is not present.");
                        return BadRequest("Invalid metadata.");
                    }

                    if (session?.PaymentStatus == "paid")
                    {
                        Console.WriteLine("Metadata: " + Newtonsoft.Json.JsonConvert.SerializeObject(session.Metadata));

                        var userId = session.Metadata["userId"];  // Metadata added during session creation
                        Console.WriteLine("User ID: " + userId.ToString());

                        //session.Metadata["userId"];
                        // Convert long? to decimal
                        var amount = session.AmountTotal.HasValue 
                            ? (decimal)session.AmountTotal.Value / 100 
                            : 0;

                        if (amount > 0) // Ensure the amount is valid
                        {
                            await _userWalletService.TopUpWalletAsync(userId, amount);
                            //new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>([new Claim(ClaimTypes.Name, userId)])))
                            Console.WriteLine("Wallet topped up successfully.");

                        }
                        else
                        {
                            Console.WriteLine("Invalid payment amount received.");
                        }
                    }
                }
                else
                {
                    // Log unexpected event types
                    Console.WriteLine($"Unhandled event type: {stripeEvent.Type}");
                }

                return Ok();
            }
            catch (StripeException e)
            {
                // Log the exception for debugging
                Console.WriteLine($"Stripe exception: {e.Message}");
                return BadRequest();
            }
            catch (Exception e)
            {
                // Log unexpected exceptions
                Console.WriteLine($"Unexpected exception: {e.Message}");
                return StatusCode(500);
            }
        }
        // [HttpGet("verify-session")]
        // public async Task<IActionResult> VerifySession([FromQuery] string session_id)
        // {
        //     if (string.IsNullOrEmpty(session_id))
        //     {
        //         return BadRequest(new { status = "error", message = "Session ID is required." });
        //     }

        //     try
        //     {
        //         var service = new SessionService();
        //         var session = await service.GetAsync(session_id);
        //         Console.WriteLine($"Received session_id: {session_id}");

        //         if (session.PaymentStatus == "paid")
        //         {
        //                 Console.WriteLine($"Received session_id post paid: {session_id}");

        //             if (session.Metadata == null || !session.Metadata.ContainsKey("userId"))
        //             {
        //                 Console.WriteLine("Metadata is missing or userId is not present.");
        //                 return BadRequest(new { status = "error", message = "Invalid session metadata." });
        //             }

        //             var response = new
        //             {
        //                 status = "success",
        //                 sessionId = session.Id,
        //                 userId = session.Metadata["userId"],
        //                 amount = session.AmountTotal.HasValue ? (decimal)session.AmountTotal.Value / 100 : 0,
        //             };
        //             return Ok(response);
        //         }

        //         return Ok(new { status = "failed", message = "Payment not completed." });
        //     }
        //     catch (StripeException e)
        //     {
        //         Console.WriteLine($"Stripe exception: {e.Message}");
        //         return BadRequest(new { status = "error", message = e.Message });
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine($"Unexpected exception: {e.Message}");
        //         return StatusCode(500, new { status = "error", message = "Internal Server Error" });
        //     }
        // }
    }
}

    

    

