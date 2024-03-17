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
using System.Threading.Tasks;
using Utility;

namespace APP.Areas.Main.Controllers
{
    [Area("Main")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class TestController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TestController> _logger;
        public readonly IWebHostEnvironment _hostEnvironment;
        public TestController(ILogger<TestController> logger, IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Test test)
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
                            var uploads = Path.Combine(webRootPath, @"img\checkUp");
                            var extenstion = Path.GetExtension(files[0].FileName);



                            using (var filesStreams = new FileStream(Path.Combine(uploads, fileName + extenstion), FileMode.Create))
                            {
                                files[0].CopyTo(filesStreams);
                            }
                            test.ImgUrl = @"\img\checkUp\" + fileName + extenstion;
                        }
                        var tst = new Test()
                        {
                            ImgUrl = test.ImgUrl,

                            Name = test.Name,
                            Price = test.Price,
                            Description = test.Description,
                            ModelName = test.ModelName,
                            CurStatus=SD.Active


                        };
                        await _unitOfWork.Tests.AddAsync(tst);
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

        public async Task<IActionResult> EditTest(string id)
        {
            var test = await _unitOfWork.Tests.GetFirstOrDefaultAsync(a => a.Id == Convert.ToInt32(id));
            if (test == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(test);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTest(Test test)
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
                            var uploads = Path.Combine(webRootPath, @"img\checkUp");
                            var extenstion = Path.GetExtension(files[0].FileName);

                            if (test.ImgUrl != null)
                            {

                                var imagePath = Path.Combine(webRootPath, test.ImgUrl.TrimStart('\\'));
                                if (System.IO.File.Exists(imagePath))
                                {
                                    System.IO.File.Delete(imagePath);
                                }
                            }
                            using (var filesStreams = new FileStream(Path.Combine(uploads, fileName + extenstion), FileMode.Create))
                            {
                                files[0].CopyTo(filesStreams);
                            }
                            test.ImgUrl = @"\img\checkUp\" + fileName + extenstion;
                        }
                        var book = await _unitOfWork.Tests.GetFirstOrDefaultAsync(a => a.Id == test.Id);

                        book.ImgUrl = test.ImgUrl;
                        book.Name = test.Name;
                        book.Price = test.Price;
                        book.ModelName = test.ModelName;
                        book.Description = test.Description;
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
        #region JS Calls
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var objList = await _unitOfWork.Tests.GetAllAsync(a=>a.CurStatus==SD.Active);
            return Json(new { data = objList });
        }

        #endregion
    }
}
