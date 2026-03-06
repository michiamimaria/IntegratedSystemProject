using BookStore.Domain.Models;
using BookStore.Domain.Models.Domain.DTO;
using BookStore.Repository.Interface;
using BookStore.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using System.Security.Claims;

namespace BookStore.Web.Controllers
{
    public class ShoppingCartsController : Controller
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly StripeSettings _stripeSettings;

        public ShoppingCartsController(IShoppingCartService _shoppingCartService, IOptions<StripeSettings> stripeSettings)
        {
            this._shoppingCartService = _shoppingCartService;
            _stripeSettings = stripeSettings.Value;
        }

        // GET: ShoppingCarts
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var dto = _shoppingCartService.getShoppingCartInfo(userId);

            return View(dto);
            //return (IActionResult)dto;
        }


        public IActionResult DeleteFromShoppingCart(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = _shoppingCartService.deleteBookFromShoppingCart(userId, id);
            // if(result == 0) -> throw 
            return RedirectToAction("Index", "ShoppingCarts");

        }

        public IActionResult SuccessPayment()
        {
            return View();
        }
        public IActionResult NotSuccessPayment()
        {
            return View();
        }

        public IActionResult Order()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var res = _shoppingCartService.order(userId);
            //if (res == 0) then throw;
            return RedirectToAction("Index", "ShoppingCarts");

        }

        public IActionResult PayOrder(string stripeEmail, string stripeToken)
        {
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
            var customerService = new CustomerService();
            var chargeService = new ChargeService();
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = this._shoppingCartService.getShoppingCartInfo(userId);

            var customer = customerService.Create(new CustomerCreateOptions
            {
                Email = stripeEmail,
                Source = stripeToken
            });

            var charge = chargeService.Create(new ChargeCreateOptions
            {
                Amount = (Convert.ToInt32(order.TotalPrice) * 100),
                Description = "EShop Application Payment",
                Currency = "usd",
                Customer = customer.Id
            });

            if (charge.Status == "succeeded")
            {
                this.Order();
                return RedirectToAction("SuccessPayment");

            }
            else
            {
                return RedirectToAction("NotSuccessPayment");
            }
        }
    }
}
