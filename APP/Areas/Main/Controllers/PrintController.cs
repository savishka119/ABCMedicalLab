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
    public class PrintController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PrintController> _logger;
        public readonly IWebHostEnvironment _hostEnvironment;
        public PrintController(ILogger<PrintController> logger, IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult SalesInvoice()
        {
            return View();
        }
        public IActionResult Queries()
        {
            return View();
        }


    }
}
