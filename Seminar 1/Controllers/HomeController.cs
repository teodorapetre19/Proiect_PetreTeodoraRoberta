using Microsoft.AspNetCore.Mvc;
using Seminar_1.Models.VMs;
using Seminar_1.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PayPal.Api;
using Microsoft.AspNetCore.Mvc;


namespace Seminar_1.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ProductVM _productVM;
        private readonly IHttpContextAccessor httpContextAccessor;
        private IConfiguration configuration;
        private Payment payment;


        public HomeController(ProductVM productVM, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _productVM = productVM;
            this.httpContextAccessor = httpContextAccessor;
            this.configuration = configuration;
        }

        public IActionResult Index()
        {
            var products = _productVM.GetAll();
            var productVMs = products.Select(p => _productVM.ProdToProdVM(p)).ToList();
            return View(productVMs);
        }

        [HttpGet]
        [Route("Details/{id}")]
        public IActionResult Details(int id)
        {
            var product = _productVM.GetAll().FirstOrDefault(p => p.Id == id);

            if (product != null)
            {
                var productVM = _productVM.ProdToProdVM(product);
                return View(productVM);
            }

            return NotFound();
        }

        [HttpPost]
        [Route("AddToCart/{id}")]
        public IActionResult AddToCart(int id, int quantity)
        {
            var shopList = HttpContext.Session.Get<List<CartItem>>(SessionHelper.ShoppingCart);

            if (shopList == null)
                shopList = new List<CartItem>();

            var existingCartItem = shopList.FirstOrDefault(item => item.ProductId == id);

            if (existingCartItem != null)
            {
                // If the item exists, update the quantity
                existingCartItem.Quantity += quantity;
            }
            else
            {
                // If the item doesn't exist, add a new cart item
                shopList.Add(new CartItem { ProductId = id, Quantity = quantity });
            }

            HttpContext.Session.Set(SessionHelper.ShoppingCart, shopList);

            return RedirectToAction("Index", "Home", _productVM.GetAll());
        }


        [HttpPost]
        [Route("Remove/{id}")]
        public IActionResult Remove(int id, int quantity)
        {
            var shopList = HttpContext.Session.Get<List<CartItem>>(SessionHelper.ShoppingCart);

            if (shopList == null)
                return RedirectToAction("Index", "Home", _productVM.GetAll());
            var itemToRemove = shopList.FirstOrDefault(item => item.ProductId == id);

            if (itemToRemove != null)
            {
                // Remove the specified quantity or remove the item entirely if the quantity is greater than or equal to the current quantity
                if (quantity >= itemToRemove.Quantity)
                {
                    shopList.Remove(itemToRemove);
                }
                else
                {
                    itemToRemove.Quantity -= quantity;
                }
            }

            HttpContext.Session.Set(SessionHelper.ShoppingCart, shopList);

            // Get the updated shopping count
            var shoppingCount = shopList.Sum(s => s.Quantity);

            // Redirect to Index action with the updated shopping count
            return RedirectToAction("Index", "Home", new { shoppingCount });
        }

        public ActionResult PaymentWithPayPal(string cancel = null, string blogId = "", string payerId = "", string guid = "")
        {
            var clientId = configuration.GetValue<string>("PayPal:Key");
            var clientSecret = configuration.GetValue<string>("PayPal:Secret");
            var mode = configuration.GetValue<string>("PayPal:mode");
            var apiContext = PayPalConfiguration.GetAPIContext(clientId, clientSecret, mode);

            try
            {
                if (string.IsNullOrWhiteSpace(payerId))
                {
                    var baseURI = this.Request.Scheme + "://" + this.Request.Host + "/Home/PaymentWithPayPal?";
                    var guidd = Convert.ToString((new Random()).Next(100000));
                    guid = guidd;

                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid, blogId);
                    var links = createdPayment.links.GetEnumerator();
                    string? paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        var lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectUrl = lnk.href;
                        }
                    }

                    httpContextAccessor.HttpContext.Session.SetString("payment", createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    var paymentId = httpContextAccessor.HttpContext.Session.GetString("payment");
                    var executedPayment = ExecutePayment(apiContext, payerId, paymentId);
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("PaymentFailed");
                    }

                    var blogs = executedPayment.transactions[0].item_list.items[0].sku;

                    return View("PaymentSuccess");
                }
            }
            catch (Exception e)
            {
                return View("PaymentFailed");
            }
        }

        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId,
            };

            this.payment = new Payment()
            {
                id = paymentId,
            };

            return this.payment.Execute(apiContext, paymentExecution);
        }

        private Payment CreatePayment(APIContext apiContext, string redirectUrl, string blogId)
        {
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };

            itemList.items.Add(new Item()
            {
                name = "Item Detail",
                currency = "USD",
                price = "1000.00",
                quantity = "1",
                sku = "asd"
            });

            var payer = new Payer()
            {
                payment_method = "paypal"
            };

            var redirectUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };

            var amount = new Amount()
            {
                currency = "USD",
                total = "1000.00"
            };

            var tramsactionList = new List<Transaction>();

            tramsactionList.Add(new Transaction()
            {
                description = "Transaction description",
                invoice_number = Guid.NewGuid().ToString(),
                amount = amount,
                item_list = itemList
            });

            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = tramsactionList,
                redirect_urls = redirectUrls
            };

            return this.payment.Create(apiContext);
        }

        
    }
}
