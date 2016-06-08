using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdvertSite.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AdvertSite.Controllers
{
    [Authorize]
    public class StatisticsController : Controller
    {
        //
        // GET: /Statistics/

        public ActionResult UserAdvertsStat()
        {
            var user = MongoDbContext.getUser(User.Identity.Name);
            var list = MongoDbContext.db.GetCollection<Advert>("Advert").Find(a => a.UserId == user.Id).SortByDescending(c => c.ViewNumber).ToList();

            return View(list);
        }

        [Authorize(Roles="Admin")]
        public ActionResult UserStat()
        {
            var list = new List<UserStatDisplay>();
            var users = MongoDbContext.db.GetCollection<UserContext>("UserContext").Find(c => true).ToList();
            
            

            foreach (var item in users)
            {
                list.Add(new UserStatDisplay()
                {
                    Id = item.Id,
                    Name = item.Login,
                    AdverCount = (int)MongoDbContext.db.GetCollection<Advert>("Advert").Count(c => c.UserId == item.Id)
                });
            }
            

            return View(list);
        }

    }
}
