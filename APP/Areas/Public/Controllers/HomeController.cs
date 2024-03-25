using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using APP.Session;
using Utility;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity.UI.Services;
using PayPal.Api;

namespace APP.Areas.Public.Controllers
{
    [Area("Public")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HomeController> _logger;
        public readonly IWebHostEnvironment _hostEnvironment;
        private readonly ISession _session;
        private readonly IEmailSender _emailSender;

        //constructor
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment, IHttpContextAccessor httpContext, IEmailSender emailSender)
        {
            _hostEnvironment = hostEnvironment;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _session = httpContext.HttpContext.Session;
            _emailSender = emailSender;
        }
       //view homepage
        public async Task<IActionResult> Index()
        {
            IEnumerable<Test> productList = await _unitOfWork.Tests.GetAllAsync(a => a.CurStatus == SD.Active);

            if (productList != null)
            {
                return View(productList);
            }

            return View();

        }

//view checkup function (lab tests)
        public async Task<IActionResult> CheckUp(string keyword="")
        {
            IEnumerable<Test> productList = await _unitOfWork.Tests.GetAllAsync(a => a.CurStatus == SD.Active);
            if (!string.IsNullOrEmpty(keyword))
            {
                productList = productList.Where(a => a.Name.Contains(keyword));
            }
            if (productList != null)
            {
                return View(productList);
            }

            return View();
        }
       //view of creating appointment
        [Authorize(Roles = SD.RoleCustomer)]
        public async Task<IActionResult> AddToOrder(string Id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var test = await _unitOfWork.Tests.GetFirstOrDefaultAsync(a => a.Id == Convert.ToInt32(Id));
         
            var crtVM = new OrderVM()
            {
                Test = test,
                
                

            };
           
            return View(crtVM);
        }
     //post method for creating new appointment 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToOrder(OrderVM order)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(a => a.Id == claim.Value);
            if (order.Test!= null && order.Orders!=null)
            {
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        //appointment object declaration
                        var orderDetails = new Orders()
                        {
                           AppoinmentDateTime=order.Orders.AppoinmentDateTime,
                           PatientAge=order.Orders.PatientAge,
                           TotAmount=order.Test.Price,
                           CurStatus=SD.Active,
                           DoctorName=order.Orders.DoctorName,
                           OrderDate=DateTime.Now,
                           OrderStatus=SD.Pending,
                           TestId=order.Test.Id,
                           TotPaid=0.00,
                           UserId=claim.Value
                        };
                        //save new appointment
                        await _unitOfWork.Order.AddAsync(orderDetails);
                        _unitOfWork.SaveAsync();
                        _unitOfWork.CommitAsync(transaction);

                        //sending email 
                        await _emailSender.SendEmailAsync(user.UserName, "Appoinment Confirmation",
                            "<p>Hi " + user.FirstName + " " + user.LastName + " " + user.UserName 
                            + ",</p><br><br><p>Your Appoinment for ABC Laboratories is submitted.</p><br>" +
                            " <p>Name : "+ user.FirstName + " " + user.LastName + " " + user.UserName + " <br>" +
                            " Patient No : "+orderDetails.Id+"<br>" +
                            " Doctor : " + orderDetails.DoctorName + "  <br>" +
                            " Date And Time : " + orderDetails.AppoinmentDateTime.ToString("yyyy-MMM-dd hh:mm:ss tt") + " </p> <br> <p>Thank You</P>");
                        //return to view "ViewNonPaidOrder" page
                        return RedirectToAction("ViewNonPaidOrder");
                    }
                    catch (Exception)
                    {
                        //error handling
                        _unitOfWork.RollbackAsync(transaction);
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }
            }
            else
            {
                return RedirectToAction("AddToOrder", new { Id = order.Test.Id });
            }


        }


        public IActionResult ViewNonPaidOrder()
        {
            return View();
        }
        [Authorize(Roles = SD.RoleCustomer)]
        public IActionResult ViewPaidOrder()
        {
            return View();
        }
        [Authorize(Roles = SD.RoleCustomer)]
        public async Task<IActionResult> ViewReport(string id)
        {

            var obj = await _unitOfWork.TestResult.GetFirstOrDefaultAsync(a => a.OrderId == Convert.ToInt32(id), includeProperties: "Orders,Orders.Test");
            if (obj == null)
            {
                obj = new TestResults();

            }

            return View(obj);
        }
        [Authorize(Roles = SD.RoleCustomer)]
        public IActionResult Queries()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task< IActionResult> Queries(Query query)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(a => a.Id == claim.Value);
            if (ModelState.IsValid)
            {
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {


                        query.UserId = claim.Value;
                        query.CurStatus = SD.Active;
                        query.QueryStatus = SD.Pending;
                        await _unitOfWork.Query.AddAsync(query);
                        _unitOfWork.SaveAsync();
                        _unitOfWork.CommitAsync(transaction);
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception)
                    {
                        _unitOfWork.RollbackAsync(transaction);
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }

                }
            }

            return View(query);
        }
        #region Paypal Interfration
        public async Task<IActionResult> PaymentWithPaypal(string guid = "", string Cancel = null, string token = "", string blogId = "", string PayerID = "",string amt="",string orderId="")
        {
            //getting the apiContext  
            var ClientID = "AR4fopBaJWwxFzlB1wwPKD2eVsXahcxQCF2yNoc2g70yoKQ335eOAMgmINt07IWiNv5ZHPaEeAFaVcSb";
            var ClientSecret = "EIZDayUeX7Ww9_QQ7UaR3ixcxuUZnZJhlARFNh1sxSfPtf4onxa3q6t0-xR7pznHhXNtRp2kBSLuMDwb";
            var mode = "sandbox";
            APIContext apiContext = PaypalConfiguration.GetAPIContext(ClientID, ClientSecret, mode);
            // apiContext.AccessToken="Bearer access_token$production$j27yms5fthzx9vzm$c123e8e154c510d70ad20e396dd28287";
            try
            {
                //A resource representing a Payer that funds a payment Payment Method as paypal  
                //Payer Id will be returned when payment proceeds or click to pay  
                string payerId = PayerID;
                if (string.IsNullOrEmpty(payerId) && string.IsNullOrEmpty(token))
                {
                    //this section will be executed first because PayerID doesn't exist  
                    //it is returned by the create function call of the payment class  
                    // Creating a payment  
                    // baseURL is the url on which paypal sendsback the data.  
                    string baseURI = this.Request.Scheme + "://" + this.Request.Host + "/Public/Home/PaymentWithPayPal?";
                    //here we are generating guid for storing the paymentID received in session  
                    //which will be used in the payment execution  
                    var guidd = Convert.ToString((new Random()).Next(100000));
                    guid = guidd;
                    //CreatePayment function gives us the payment approval url  
                    //on which payer is redirected for paypal account payment  
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid, blogId, amt);
                    //get links returned from paypal in response to Create function call  
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment  
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    // saving the paymentID in the key guid  
                    _session.SetString("payment", createdPayment.id);
                    _session.SetString("Ordid", orderId);
                    _session.SetString("amount", amt);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment  

                    var paymentId = _session.GetString("payment");
                    var executedPayment = ExecutePayment(apiContext, payerId, paymentId as string);
                    //If executed payment failed then we will show payment failure message to user  
                    if (executedPayment.state.ToLower() != "approved")
                    {

                        return View("PaymentFailed");
                    }
                    var blogIds = executedPayment.transactions[0].item_list.items[0].sku;


                    var obj = await _unitOfWork.Order.GetFirstOrDefaultAsync(a => a.Id ==Convert.ToInt32( _session.GetString("Ordid")));

                    obj.TotPaid = Convert.ToDouble(_session.GetString("amount"));
                    _unitOfWork.SaveAsync();
                    //on successful payment, show success page to user
                    return RedirectToAction(nameof(PaymentSuccess));
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(PaymentFailed));
            }
            //on successful payment, show success page to user.  
            return RedirectToAction(nameof(Index));
        }
        private PayPal.Api.Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }

        private Payment CreatePayment(APIContext apiContext, string redirectUrl, string blogId, string amt="")
        {
            //create itemlist and add item objects to it  

            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            //Adding Item Details like name, currency, price etc  
            itemList.items.Add(new Item()
            {
                name = "Item Detail",
                currency = "USD",
                price = string.IsNullOrEmpty(amt)?"0":Math.Ceiling((Convert.ToDouble(amt)/320)).ToString(),
                quantity = "1",
                sku = "asd"
            });
            var payer = new Payer()
            {
                payment_method = "paypal"
            };
            // Configure Redirect Urls here with RedirectUrls object  
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };
            // Adding Tax, shipping and Subtotal details  
            //var details = new Details()
            //{
            //    tax = "1",
            //    shipping = "1",
            //    subtotal = "1"
            //};
            //Final amount with details  
            var amount = new Amount()
            {
                currency = "USD",
                total = string.IsNullOrEmpty(amt) ? "0" : Math.Ceiling((Convert.ToDouble(amt) / 320)).ToString(), // Total must be equal to sum of tax, shipping and subtotal.  
                //details = details
            };
            var transactionList = new List<Transaction>();
            // Adding description about the transaction  
            transactionList.Add(new Transaction()
            {
                description = "Transaction description",
                invoice_number = Guid.NewGuid().ToString(), //Generate an Invoice No  
                amount = amount,
                item_list = itemList
            });
            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            // Create a payment using a APIContext  
            return this.payment.Create(apiContext);
        }
        public IActionResult PaymentFailed()
        {
            return View();
        }
        public IActionResult PaymentSuccess()
        {
            return View();
        }
        #endregion
        #region JS Calls
        [HttpGet]
        public async Task<IActionResult> GetAllNonPaidInvoice()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var objList = (await _unitOfWork.Order.GetAllAsync(a => a.CurStatus == SD.Active && a.TotAmount>a.TotPaid && a.UserId==claim.Value,includeProperties: "Test"))
                .Select(a=> new {
                    name=a.Test.Name,
                    modelName=a.Test.ModelName,
                    price=a.TotAmount,
                    dueAmt=a.TotAmount-a.TotPaid,
                    doctorName=a.DoctorName,
                    appoinmentDate=a.AppoinmentDateTime.ToString("yyyy-MMM-dd hh:mm:ss tt"),
                    id=a.Id
                });
            return Json(new { data = objList });
        } 
        [HttpGet]
        public async Task<IActionResult> GetAllPaidInvoice()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var objList = (await _unitOfWork.Order.GetAllAsync(a => a.CurStatus == SD.Active && a.TotAmount<=a.TotPaid && a.UserId==claim.Value,includeProperties: "Test"))
                .Select(a=> new {
                    name=a.Test.Name,
                    modelName=a.Test.ModelName,
                    price=a.TotAmount,
                    dueAmt=a.TotAmount-a.TotPaid,
                    doctorName=a.DoctorName,
                    appoinmentDate=a.AppoinmentDateTime.ToString("yyyy-MMM-dd hh:mm:ss tt"),
                    id=a.Id
                });
            return Json(new { data = objList });
        }

        #endregion
    }
}
