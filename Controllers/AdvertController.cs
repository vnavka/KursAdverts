using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdvertSite.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Drawing;
using System.IO;
using PagedList.Mvc;
using PagedList;

namespace AdvertSite.Controllers
{
    [Authorize]
    public class AdvertController : Controller
    {
        public MongoDbContext context = new MongoDbContext();
        //
        // GET: /Advert/
        
        public ActionResult Index(int? page,string Message=null,bool IsPositive=false)
        {
            List<Advert> list = null;

                var user = MongoDbContext.db.GetCollection<UserContext>("UserContext").Find(c => c.Login == User.Identity.Name).FirstAsync().GetAwaiter().GetResult();
                list = MongoDbContext.db.GetCollection<Advert>("Advert").Find(a => a.UserId == user.Id).ToListAsync().GetAwaiter().GetResult();
            
            ViewBag.Message = Message;
            ViewBag.IsPositive = IsPositive;

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(list.ToPagedList(pageNumber, pageSize));
            
        }

        //
        // GET: /Advert/Create
        public ActionResult AdvertCreate(ManageMessageId? message)
        {
            //HttpPostedFileBase file 
            var cats = MongoDbContext.db.GetCollection<Category>("Category").Find(n => true).ToListAsync().GetAwaiter().GetResult();
            ViewBag.CategoryId = new SelectList(cats, "Id", "Name");

            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Объвление добавлено"
                : "";
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdvertCreate(Advert model,HttpPostedFileBase image)
        {
            var cats = MongoDbContext.db.GetCollection<Category>("Category").Find(n => true).ToListAsync().GetAwaiter().GetResult();
            
            ViewBag.CategoryId = new SelectList(cats, "Id", "Name");
            
                 //attempt to add the advert
            if(ModelState.IsValid)
            {
                bool addSucceded;

                try
                {
                    if (image != null && image.IsImage())
                    {
                        image.SaveAs(Server.MapPath("~/Content/Images/") + image.FileName);
                        model.Photo = image.FileName;
                    }
                    else
                        model.Photo = null;
   
                    model.Id = ObjectId.GenerateNewId().ToString();
                    model.PublicationDate = DateTime.Now;
                    model.ViewNumber = 0;
                    //model.VIP = false;
                    model.UserId = MongoDbContext.getUser(User.Identity.Name).Id.ToString();
                    context.AddAdvert(model);
                    addSucceded = true;
                }
                catch (Exception e)
                { 
                    addSucceded = false;
                }
                if (addSucceded)
                    return RedirectToAction("AdvertCreate", new { Message = ManageMessageId.ChangePasswordSuccess });
                else
                    ModelState.AddModelError("", "Ошибка добавления объявления");
            }

                

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        
        [AllowAnonymous]
        public ActionResult Display(string Id)
        {
            Advert adv = MongoDbContext.db.GetCollection<Advert>("Advert").Find(m => m.Id == Id).FirstAsync().GetAwaiter().GetResult();

            var uptd = Builders<Advert>.Update.Inc(c => c.ViewNumber, 1);

            MongoDbContext.db.GetCollection<Advert>("Advert").UpdateOne(a => a.Id == Id, uptd);

            return View("Display", adv);
        }
        public ActionResult Delete(string Id)
        {
            try
            {
                MongoDbContext.DeleteAdvert(Id);

                return RedirectToAction("Index", new { IsPositive = true, Message = "Объявление удалено" });
                
            }
            catch(Exception e)
            {
                return RedirectToAction("Index", new { IsPositive = false, Message = "Во время удаления возникли ошибки" });
            }
        }
        public ActionResult Edit(string Id,ManageMessageId? message)
        {
            Advert adv = MongoDbContext.db.GetCollection<Advert>("Advert").Find(m => m.Id == Id).FirstAsync().GetAwaiter().GetResult();
            var cats = MongoDbContext.db.GetCollection<Category>("Category").Find(n => true).ToListAsync().GetAwaiter().GetResult();

            ViewBag.CategoryId = new SelectList(cats, "Id", "Name",adv.CategoryId);

            ViewBag.StatusMessage =
            message == ManageMessageId.Success ? "Объявление изменено"
            : "";
            return View(adv);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Advert model, HttpPostedFileBase image)
        {
            var cats = MongoDbContext.db.GetCollection<Category>("Category").Find(n => true).ToListAsync().GetAwaiter().GetResult();
            var old = MongoDbContext.db.GetCollection<Advert>("Advert").Find(m => m.Id == model.Id).FirstAsync().GetAwaiter().GetResult();

            ViewBag.CategoryId = new SelectList(cats, "Id", "Name", model.CategoryId);

            //attempt to edit the advert
            if (ModelState.IsValid)
            {
                bool editSucceded;
                try
                {
                    if (image != null && image.IsImage())
                    {
                        image.SaveAs(Server.MapPath("~/Content/Images/") + image.FileName);
                        model.Photo = image.FileName;
                    }
                    else
                        model.Photo = old.Photo;
                    model.PublicationDate = old.PublicationDate;
                    model.ViewNumber = old.ViewNumber;
                    model.UserId = old.UserId;
                    //model.VIP = old.VIP;

                    MongoDbContext.db.GetCollection<Advert>("Advert").ReplaceOneAsync(m => m.Id == model.Id, model).GetAwaiter().GetResult();
                    editSucceded = true;
                }
                catch (Exception e)
                {
                    editSucceded = false;
                }
                if (editSucceded)
                    return RedirectToAction("Edit", new { Message = ManageMessageId.Success });
                else
                    ModelState.AddModelError("", "Ошибка добавления объявления");
            }



            // If we got this far, something failed, redisplay form
            return View(model);
        }

    }

    



    public enum ManageMessageId
    {
        Success,
        ChangePasswordSuccess,
        SetPasswordSuccess,
        RemoveLoginSuccess,
    }


}
public static class HttpPostedFileBaseExtensions
{
    public const int ImageMinimumBytes = 1;

    public static bool IsImage(this HttpPostedFileBase postedFile)
    {
        //-------------------------------------------
        //  Check the image mime types
        //-------------------------------------------
        if (postedFile.ContentType.ToLower() != "image/jpg" &&
                    postedFile.ContentType.ToLower() != "image/jpeg" &&
                    postedFile.ContentType.ToLower() != "image/pjpeg" &&
                    postedFile.ContentType.ToLower() != "image/gif" &&
                    postedFile.ContentType.ToLower() != "image/x-png" &&
                    postedFile.ContentType.ToLower() != "image/png")
        {
            return false;
        }

        //-------------------------------------------
        //  Check the image extension
        //-------------------------------------------
        if (Path.GetExtension(postedFile.FileName).ToLower() != ".jpg"
            && Path.GetExtension(postedFile.FileName).ToLower() != ".png"
            && Path.GetExtension(postedFile.FileName).ToLower() != ".gif"
            && Path.GetExtension(postedFile.FileName).ToLower() != ".jpeg")
        {
            return false;
        }

        //-------------------------------------------
        //  Attempt to read the file and check the first bytes
        //-------------------------------------------
        try
        {
            if (!postedFile.InputStream.CanRead)
            {
                return false;
            }

            if (postedFile.ContentLength < ImageMinimumBytes)
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }

        //-------------------------------------------
        //  Try to instantiate new Bitmap, if .NET will throw exception
        //  we can assume that it's not a valid image
        //-------------------------------------------

        try
        {
            using (var bitmap = new System.Drawing.Bitmap(postedFile.InputStream))
            {
            }
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}
