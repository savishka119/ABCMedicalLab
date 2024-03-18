using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Utility;

namespace APP.Areas.Lab.Controllers
{
    [Area("Lab")]
    [Authorize(Roles = SD.RoleAdmin+","+SD.RoleTechnician)]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HomeController> _logger;
        public readonly IWebHostEnvironment _hostEnvironment;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ViewPaidOrder()
        {
            return View();
        }
        public async Task<IActionResult> AddReport(string id)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(a => a.Id == Convert.ToInt32(id),includeProperties: "Test");
           
            if (order == null)
            {
                return RedirectToAction(nameof(Index));
            }
            var testResult = new TestResults()
            {
                Orders = order,
               
                OrderId = order.Id,

            };
            return View(testResult);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReport(TestResults test)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        string webRootPath = _hostEnvironment.WebRootPath;
                        var files = HttpContext.Request.Form.Files;
                        if (files.Count > 0)
                        {
                            string fileName = Guid.NewGuid().ToString();
                            var uploads = Path.Combine(webRootPath, @"img\testResult");
                            var extenstion = Path.GetExtension(files[0].FileName);

                            if (test.imgUrl != null)
                            {

                                var imagePath = Path.Combine(webRootPath, test.imgUrl.TrimStart('\\'));
                                if (System.IO.File.Exists(imagePath))
                                {
                                    System.IO.File.Delete(imagePath);
                                }
                            }
                            using (var filesStreams = new FileStream(Path.Combine(uploads, fileName + extenstion), FileMode.Create))
                            {
                                files[0].CopyTo(filesStreams);
                            }
                            test.imgUrl = @"\img\testResult\" + fileName + extenstion;
                        }
                        await _unitOfWork.TestResult.AddAsync(test);
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

            return View(test);
        }

        public async Task<IActionResult> ViewReport(string id)
        {
           
            var obj = await _unitOfWork.TestResult.GetFirstOrDefaultAsync(a => a.OrderId == Convert.ToInt32(id), includeProperties: "Orders,Orders.Test");
            if (obj==null)
            {
                obj = new TestResults();
              
            }
                
            return View(obj);
        }
        public IActionResult ViewNonPaidOrder()
        {
            return View();
        }
        public async Task<IActionResult> PayOrderRcpt(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(a => a.Id == Convert.ToInt32(id), includeProperties: "User");

                return View(order);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayOrderRcpt(Orders orders)
        {
            var ord = await _unitOfWork.Order.GetFirstOrDefaultAsync(a => a.Id == orders.Id, includeProperties: "User");
            if (ModelState.IsValid)
            {
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        ord.TotPaid = ord.TotAmount;
                        _unitOfWork.SaveAsync();
                        _unitOfWork.CommitAsync(transaction);
                        return RedirectToAction(nameof(ViewNonPaidOrder));
                    }
                    catch (Exception)
                    {
                        _unitOfWork.RollbackAsync(transaction);
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }

                }
            }

            return View(ord);
        }
        #region JS Call
        [HttpGet]
        public async Task<IActionResult> GetAllPaidInvoice()
        {
          
            var objList = (await _unitOfWork.Order.GetAllAsync(a => a.CurStatus == SD.Active && a.TotAmount <= a.TotPaid , includeProperties: "Test"))
                .Select(a => new {
                    name = a.Test.Name,
                    modelName = a.Test.ModelName,
                    price = a.TotAmount,
                    dueAmt = a.TotAmount - a.TotPaid,
                    doctorName = a.DoctorName,
                    appoinmentDate = a.AppoinmentDateTime.ToString("yyyy-MMM-dd hh:mm:ss tt"),
                    id = a.Id
                });
            return Json(new { data = objList });
        }
        [HttpGet]
        public async Task<IActionResult> GetAllNonPaidInvoice()
        {

            var objList = (await _unitOfWork.Order.GetAllAsync(a => a.CurStatus == SD.Active && a.TotAmount > a.TotPaid, includeProperties: "Test"))
                .Select(a => new {
                    name = a.Test.Name,
                    modelName = a.Test.ModelName,
                    price = a.TotAmount,
                    dueAmt = a.TotAmount - a.TotPaid,
                    doctorName = a.DoctorName,
                    appoinmentDate = a.AppoinmentDateTime.ToString("yyyy-MMM-dd hh:mm:ss tt"),
                    id = a.Id
                });
            return Json(new { data = objList });
        }
        #endregion
    }
}
