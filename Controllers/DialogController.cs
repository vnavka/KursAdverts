using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdvertSite.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;

namespace AdvertSite.Controllers
{
    [Authorize]
    public class DialogController : Controller
    {
        //
        // GET: /Dialog/

        public ActionResult AddDialog(string username, string advertid)
        {
            
            var user = MongoDbContext.db.GetCollection<UserContext>("UserContext").Find(c => c.Login == username).FirstAsync().GetAwaiter().GetResult();
            var dialog = MongoDbContext.db.GetCollection<Dialog>("Dialog").Find(c => c.AdvertId == advertid && c.userId == user.Id).FirstOrDefaultAsync().GetAwaiter().GetResult();
            if(dialog == null)
            {
                var advert = MongoDbContext.db.GetCollection<Advert>("Advert").Find(c => c.Id == advertid).FirstAsync().GetAwaiter().GetResult();
                dialog = new Dialog()
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    CreationDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    AdvertId = advertid,
                    Messages = new List<Message>(),
                    userId = user.Id,
                    ownerId = advert.User.Id
                };
                MongoDbContext.db.GetCollection<Dialog>("Dialog").InsertOneAsync(dialog).Wait();
            }


            return RedirectToAction("Display", new { Id = dialog.Id });
        }
        public ActionResult Display(string Id)
        {
            var dialog = MongoDbContext.db.GetCollection<Dialog>("Dialog").Find(c => c.Id == Id).FirstOrDefaultAsync().GetAwaiter().GetResult();
            return View(dialog);
        }

        [HttpPost]
        public ActionResult MessageSent(string dialog_id, string username,string message)
        {
            if (!String.IsNullOrWhiteSpace(message))
            {
                var user = MongoDbContext.db.GetCollection<UserContext>("UserContext").Find(c => c.Login == username).FirstAsync().GetAwaiter().GetResult();
                var dialog = MongoDbContext.db.GetCollection<Dialog>("Dialog").Find(c => c.Id == dialog_id).FirstOrDefaultAsync().GetAwaiter().GetResult();
                dialog.Messages.Add(new Message()
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Text=message,
                    Datetime=DateTime.Now,
                    UserId=user.Id
                    
                });
                dialog.UpdateDate = DateTime.Now;
                MongoDbContext.db.GetCollection<Dialog>("Dialog").ReplaceOneAsync(c => c.Id == dialog_id, dialog).Wait();
            }
            var dialog_updated = MongoDbContext.db.GetCollection<Dialog>("Dialog").Find(c => c.Id == dialog_id).FirstOrDefaultAsync().GetAwaiter().GetResult();

            return PartialView("_MessageList",dialog_updated);
        }

        public ActionResult Index()
        {
            MongoDbContext.ManageDialogs();
            var user = MongoDbContext.getUser(User.Identity.Name);

            var dialogs = MongoDbContext.db.GetCollection<Dialog>("Dialog").Find(c => c.ownerId == user.Id || c.userId == user.Id).SortByDescending(d => d.UpdateDate).ToList();
            return View(dialogs);
        }

    }
}
