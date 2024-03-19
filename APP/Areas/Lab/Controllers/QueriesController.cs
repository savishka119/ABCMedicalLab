using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utility;

namespace APP.Areas.Lab.Controllers
{
    [Area("Lab")]
    [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleTechnician)]
    public class QueriesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<QueriesController> _logger;
        public readonly IWebHostEnvironment _hostEnvironment;
        private readonly IEmailSender _emailSender;
        public QueriesController(ILogger<QueriesController> logger, IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment, IEmailSender emailSender)
        {
            _hostEnvironment = hostEnvironment;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task <IActionResult> ReplyToQuery(int id)
        {
            var obj = await _unitOfWork.Query.GetFirstOrDefaultAsync(a=>a.Id==id,includeProperties: "User");
            if (obj==null)
            {
                obj = new Query();
            }
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyToQuery(Query query)
        {
            var ord = await _unitOfWork.Query.GetFirstOrDefaultAsync(a => a.Id == query.Id, includeProperties: "User");
            if (ModelState.IsValid)
            {
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        ord.Reply = query.Reply;
                        _unitOfWork.SaveAsync();
                        _unitOfWork.CommitAsync(transaction);
                        await _emailSender.SendEmailAsync(ord.User.UserName, "Reply For Your Query", "<p>" + query.Reply + "</p>");
                        return RedirectToAction(nameof(Index));
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
        #region JS Calls
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {

            var objList = (await _unitOfWork.Query.GetAllAsync(a => a.CurStatus == SD.Active , includeProperties: "User"))
                .Select(a => new {
                    customerName = a.User.FirstName+" "+a.User.UserName,
                    queries = a.QueryDetails,
                    reply = a.Reply,
                  
                    id = a.Id
                });
            return Json(new { data = objList });
        }
        #endregion
    }
}
