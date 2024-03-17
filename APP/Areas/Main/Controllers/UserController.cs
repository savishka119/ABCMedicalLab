using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Models;
using Models.ViewModels;
using Utility;

namespace APP.Areas.Main.Controllers
{
    [Authorize(Roles = SD.RoleAdmin)]
    [Area("Main")]
    public class UserController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserController> _logger;
        public readonly IWebHostEnvironment _hostEnvironment;

        public UserController(ILogger<UserController> logger, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment hostEnvironment)
        {

            _hostEnvironment = hostEnvironment;
            _roleManager = roleManager;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
   
        }
        public IActionResult Index()
        {

            return View();
        }
        public IActionResult ViewPermissions()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult CustomerDetails()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUserVM applicationUserVM)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    //!Do not always use transaction, use only if there are more than one SaveAsync() calls
                    try
                    {
                        var claimsIdentity = (ClaimsIdentity)User.Identity;
                        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                        var logedInUser = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(x => x.Id == claim.Value);

                        string userKey = Guid.NewGuid().ToString();
                        var applicationUser = new ApplicationUser
                        {
                            UserName = applicationUserVM.Email,
                            Email = applicationUserVM.Email,
                            EmailConfirmed = true,
                            FirstName = applicationUserVM.FirstName,
                            LastName = applicationUserVM.LastName,
                            CreatedBy = logedInUser.Email,
                            CreatedOnUTC = DateTime.UtcNow,
                            Role = applicationUserVM.Role
                        };
                        var result = await _userManager.CreateAsync(applicationUser, applicationUserVM.Password);

                        if (result.Succeeded)
                        {
                            var user = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(x => x.Id == applicationUser.Id);
                            await _userManager.AddToRoleAsync(user, applicationUserVM.Role);
                           

                            _unitOfWork.CommitAsync(transaction);
                            return RedirectToAction(nameof(Index));
                        }
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                    catch (System.Exception)
                    {
                        _unitOfWork.RollbackAsync(transaction);
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }
            }
            return View(applicationUserVM);
        }
        public async Task<IActionResult> ResetPassword(string id)
        {
            RestPasswordVM restPasswordVM = new RestPasswordVM()
            {
                ApplicationUser = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(a => a.Id == id)
            };
            var code = await _userManager.GeneratePasswordResetTokenAsync(restPasswordVM.ApplicationUser);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            restPasswordVM.Code = code;

            return View(restPasswordVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(RestPasswordVM restPasswordVM)
        {
            if (ModelState.IsValid)
            {
                var user = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(a => a.Id == restPasswordVM.ApplicationUser.Id);
                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(restPasswordVM.Code));
                var result = await _userManager.ResetPasswordAsync(user, code, restPasswordVM.Password);
            }
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> ViewCustomer(string id)
        {
            var user = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(a => a.Id == id);
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ViewCustomer(ApplicationUser user)
        {
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var logedInUser = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(x => x.Id == claim.Value);

                    var appUser = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(a => a.Id == user.Id);
                    appUser.UpdatedBy = logedInUser.Id;
                    appUser.UpdatedOnUTC = DateTime.UtcNow;
                    appUser.CurStatus = appUser.CurStatus == SD.Active ? SD.DeActive : SD.Active;
                    _unitOfWork.ApplicationUser.Update(appUser);
                    _unitOfWork.SaveAsync();
                    _unitOfWork.CommitAsync(transaction);

                }
                catch (System.Exception)
                {
                    _unitOfWork.RollbackAsync(transaction);
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            return RedirectToAction(nameof(CustomerDetails));
        }
        public async Task<IActionResult> EditProfile(string id)
        {
            var user = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(a => a.Id == id);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    //!Do not always use transaction, use only if there are more than one SaveAsync() calls
                    try
                    {
                        var claimsIdentity = (ClaimsIdentity)User.Identity;
                        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                        var logedInUser = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(x => x.Id == claim.Value);

                        var appUser = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(a => a.Id == user.Id);
                        appUser = user;
                        appUser.UpdatedBy = logedInUser.Id;
                        appUser.UpdatedOnUTC = DateTime.UtcNow;
                        _unitOfWork.ApplicationUser.Update(appUser);
                        _unitOfWork.SaveAsync();
                        _unitOfWork.CommitAsync(transaction);
                    }
                    catch (System.Exception)
                    {
                        _unitOfWork.RollbackAsync(transaction);
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }
            }
            return RedirectToAction(nameof(Index));
        }

        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = from a in await _unitOfWork.ApplicationUser.GetAllAsync()
                       where a.Role==SD.RoleAdmin || a.Role==SD.RoleTechnician
                       select new
                       {
                           a.Email,
                           name = $"{a.FirstName} {a.LastName}",
                           a.Id
                       };
            return Json(new { data });
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            var data = from a in await _unitOfWork.ApplicationUser.GetAllAsync()
                       where a.Role==SD.RoleCustomer
                       select new
                       {
                           a.Email,
                           name = $"{a.FirstName} {a.LastName}",
                           a.Id
                       };
            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPermissions()
        {
            var data = await _unitOfWork.ApplicationRole.GetAllAsync();
            return Json(new { data });
        }

        #endregion

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
