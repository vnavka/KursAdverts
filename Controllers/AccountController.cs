using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using AdvertSite.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using AdvertSite.Filters;

namespace AdvertSite.Controllers
{
    [Authorize] 
    public class AccountController : Controller
    {
        public MongoDbContext context = new MongoDbContext();
        
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            
            ViewBag.ReturnUrl = returnUrl;

            
            return View();

        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            
            if (ModelState.IsValid && MongoDbContext.UserExists(model.UserName) && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                return Redirect(Request.UrlReferrer.OriginalString);
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Неверные логин или пароль");
            TempData["ValSummary"] = "Неверные логин или пароль";
            return Redirect(Url.Action("Index", "Home"));
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {  
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password);

                    UserContext user = new UserContext() { Id=ObjectId.GenerateNewId().ToString(), Email = model.Email, Login = model.UserName };
                    context.AddUser(user); 
                    WebSecurity.Login(model.UserName, model.Password);
                    return RedirectToAction("Index", "Home");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", "Ошибка создания пользователя");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }



        //
        // GET: /Account/ChangePassword

        public ActionResult ChangePassword(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Ваш пароль изменен"
                : "";

            return View();
        }

        //
        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(LocalPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                    return RedirectToAction("ChangePassword", new { Message = ManageMessageId.ChangePasswordSuccess });
                else
                    ModelState.AddModelError("", "Введеный текущий пароль неверен");
                
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Account/Manage

        public ActionResult Manage()
        {
            if(!User.IsInRole("Admin"))
            {
                var user = MongoDbContext.getUser(WebSecurity.CurrentUserName);
                int count = (int)MongoDbContext.db.GetCollection<Advert>("Advert").Count(c => c.UserId == user.Id);
                ViewBag.AdvertCount = count;
                ViewBag.Creationdate = WebSecurity.GetCreateDate(user.Login);
                return View(user);
            }
            else
                return new HttpStatusCodeResult(404);
                 
        }
        [HttpPost]
        public string Manage(string Name,string Surname,string City,string Phone)
        {
            try
            {
                var user = MongoDbContext.getUser(WebSecurity.CurrentUserName);
                user.Name = Name;
                user.Surname = Surname;
                user.City = City;
                user.Phone = Phone;
                MongoDbContext.db.GetCollection<UserContext>("UserContext").ReplaceOneAsync(c => c.Id == user.Id, user).Wait();
                return "Данные успешно сохранены";
            }
            catch(Exception e)
            {
                return "Ошибка сохранения данных";
            }
            
        }
        [AllowAnonymous]
        public ActionResult Display(string Id)
        {
            var user = MongoDbContext.db.GetCollection<UserContext>("UserContext").Find(c => c.Id == Id).First();
            int count = (int)MongoDbContext.db.GetCollection<Advert>("Advert").Count(c => c.UserId == user.Id);
            ViewBag.AdvertCount = count;
            ViewBag.Creationdate = WebSecurity.GetCreateDate(user.Login);
            return View(user);

        }
        [AllowAnonymous]
        public ActionResult UserAdverts(string Id)
        {
            var list = MongoDbContext.db.GetCollection<Advert>("Advert").Find(c => c.UserId == Id).ToList();
            return PartialView("_UserAdverts",list);
        }



        

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }



        
    }
}
        #endregion