using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using Utility;

namespace DataAccess.Initializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
        }


        public void Initialize()
        {
            #region Database Initialization
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception)
            {

            }
            #endregion

            #region Initial Admin User 

           

            if (_db.Roles.Any(r => r.Name == SD.RoleAdmin)) return;

            _roleManager.CreateAsync(new IdentityRole(SD.RoleAdmin)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.RoleCustomer)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.RoleTechnician)).GetAwaiter().GetResult();
          
            if (!_db.ApplicationUsers.Any())
            {
                var applicationUser = new ApplicationUser
                {
                    UserName = "admin@ABCLab.com",
                    Email = "admin@ABCLab.com",
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Admin",
                    CreatedBy = "System",
                    CreatedOnUTC = DateTime.UtcNow,
                };
                _userManager.CreateAsync(applicationUser, "Admin@#1234").GetAwaiter().GetResult();


                ApplicationUser user = _db.ApplicationUsers.Where(u => u.Email == "admin@ABCLab.com").FirstOrDefault();

                _userManager.AddToRoleAsync(user, SD.RoleAdmin).GetAwaiter().GetResult();

            };
            #endregion
        }
    }
}