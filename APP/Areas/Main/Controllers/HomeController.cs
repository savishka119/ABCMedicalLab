using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Models.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Utility;

namespace APP.Areas.Main.Controllers
{
    [Area("Main")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        public readonly IWebHostEnvironment _hostEnvironment;
        public HomeController(ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager, IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
            _logger = logger;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index()
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return Redirect(Url.Action("Login", "Account", new { area = "Identity" }));
            }
            TempData["OrdersCount"] = (await _unitOfWork.Order.GetAllAsync()).Count();
            TempData["PendingOrder"] = (await _unitOfWork.Order.GetAllAsync(a=>a.TotPaid==0)).Count();
            TempData["ProcessOrder"] = (await _unitOfWork.Order.GetAllAsync(a => a.TotPaid > 0)).Count();
            TempData["TotCustomer"] = (await _unitOfWork.ApplicationUser.GetAllAsync(a=>a.Role==SD.RoleCustomer && a.CurStatus==SD.Active)).Count();
            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
