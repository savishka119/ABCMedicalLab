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

namespace APP.Areas.Public.Controllers
{
    [Area("Public")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HomeController> _logger;
        public readonly IWebHostEnvironment _hostEnvironment;
        private readonly ISession _session;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment,IHttpContextAccessor httpContext)
        {
            _hostEnvironment = hostEnvironment;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _session = httpContext.HttpContext.Session;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<Test> productList = await _unitOfWork.Tests.GetAllAsync(a => a.CurStatus == SD.Active);

            if (productList != null)
            {
                return View(productList);
            }

            return View();

        }
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToOrder(OrderVM order)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (order.Test!= null && order.Orders!=null)
            {
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        var cartDetails = new Orders()
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
                        await _unitOfWork.Order.AddAsync(cartDetails);
                        _unitOfWork.SaveAsync();
                        _unitOfWork.CommitAsync(transaction);

                       
                        return RedirectToAction("ViewCart");
                    }
                    catch (Exception)
                    {
                        _unitOfWork.RollbackAsync(transaction);
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }
            }
            else
            {
                return RedirectToAction("AddToCart", new { Id = order.Test.Id });
            }


        }

    }
}
