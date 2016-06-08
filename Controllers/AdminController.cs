using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdvertSite.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using WebMatrix.WebData;
using System.Web.Security;


namespace AdvertSite.Controllers
{
    [Authorize(Roles="Admin")]
    public class AdminController : Controller
    {
        //
        // GET: /Admin/

        public ActionResult Users()
        {
            var users = MongoDbContext.db.GetCollection<UserContext>("UserContext").Find(n => true).ToList();
            return View(users);
        }
        public ActionResult Categories()
        {
            var cats = MongoDbContext.db.GetCollection<Category>("Category").Find(n => true).ToList();
            return View(cats);
        }
        public string DeleteCat(string Id)
        {
            //Delete adverts
            try
            {
                MongoDbContext.DeleteCat(Id);
                return "Категория удалена";
            }
            catch (Exception e)
            {
                return "Ошибка удаления категории";
            }

        }
        public ActionResult CatAdd(string name)
        {
            var cat = new Category() { Name = name };
            MongoDbContext.db.GetCollection<Category>("Category").InsertOne(cat);
            var cats = MongoDbContext.db.GetCollection<Category>("Category").Find(n => true).ToList();
            return PartialView("_CatsTable", cats);
        }
        public string DeleteUser(string Id)
        {
            try
            {
                MongoDbContext.DeleteUser(Id);

                return "Пользователь удален";
            }
            catch(Exception e)
            {
                return "Ошибка удаления пользователя";
            }

        }
        public string DeleteAdvert(string Id)
        {
            try
            {
                MongoDbContext.DeleteAdvert(Id);
                return "Обїявление удалено";
            }
            catch (Exception e)
            {
                return "Ошибка удаления объявления";
            }
        }

    }
}
