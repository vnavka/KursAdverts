using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdvertSite.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using PagedList.Mvc;
using PagedList;
using WebMatrix.WebData;
using AdvertSite.Filters;

namespace AdvertSite.Controllers
{
    [InitializeSimpleMembershipAttribute]
    public class HomeController : Controller
    {
        public ActionResult Index(int? page)
        {
            ViewBag.ValSummary = TempData["ValSummary"];
   
            var list = MongoDbContext.db.GetCollection<Advert>("Advert").Find(n => true).SortByDescending(n => n.PublicationDate).ToList();
            var cats = MongoDbContext.db.GetCollection<Category>("Category").Find(n=>true).ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.SortList = new SelectList(SearchHelper.getSortList(), "Val", "Data");
            ViewBag.Category = new SelectList(cats, "Id", "Name"); 
            return View(list.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Search(string SortList,int? page,string Category,string search=null)
        {
            var lst = SearchHelper.getSearchRez(search, Category);

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            List <Advert> list = null;
            if (SortList == "date")
                list = lst.SortByDescending(c => c.PublicationDate).ToList();
            else
                list = lst.SortByDescending(c => c.ViewNumber).ToList();

            ViewBag.SortList = new SelectList(SearchHelper.getSortList(), "Val", "Data", SortList);
            var cats = MongoDbContext.db.GetCollection<Category>("Category").Find(n => true).ToList();
            ViewBag.Category = new SelectList(cats, "Id", "Name");

            return View("Index", list.ToPagedList(pageNumber, pageSize));

            
        }

    }
}
public class SearchHelper
{
    public static List<SortDropDown> getSortList()
    {
        List<SortDropDown> lst = new List<SortDropDown>{
            new SortDropDown(){Val="date",Data = "Сортировать по дате"},
            new SortDropDown(){Val="view",Data ="Сортировать по количеству просмотров"}
        };
        return lst;
    }
    public static IFindFluent<Advert, Advert> getSearchRez(string search, string cat)
    {
        IFindFluent<Advert, Advert> lst = null;
        if (search == null && String.IsNullOrEmpty(cat))
            return MongoDbContext.db.GetCollection<Advert>("Advert").Find(n => true);

        else if (search != null && String.IsNullOrEmpty(cat))
        {
            search = search.ToLower();

            return MongoDbContext.db.GetCollection<Advert>("Advert").Find
            (n => n.Description.ToLower().Contains(search) || n.Name.ToLower().Contains(search));
        }
        else if (search == null && !String.IsNullOrEmpty(cat))
            return MongoDbContext.db.GetCollection<Advert>("Advert").Find(c => c.CategoryId == cat);
        else
        {
            search = search.ToLower();

            return MongoDbContext.db.GetCollection<Advert>("Advert").Find
            (n => n.CategoryId == cat && (n.Description.ToLower().Contains(search) || n.Name.ToLower().Contains(search)));
        }
             

    }

}
public class SortDropDown
{
    public string Val { get; set; }
    public string Data { get; set; }
}
