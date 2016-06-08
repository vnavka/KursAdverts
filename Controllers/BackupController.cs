using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdvertSite.Controllers
{   [Authorize]
    public class BackupController : Controller
    {
        //
        // GET: /Backup/

        public ActionResult Index()
        {
            return View();
        }
        public string Create()
        {
            try
            {
                Process.Start("mongodump.exe", "-d AdvertDb -o C:/dump");
                return "Резервная копия создана";
            }
            catch(Exception e)
            {
                return "Ошибка создания резервной копии";
            }
        }
        public string Restore()
        {
            try
            {
                Process.Start("mongorestore.exe", "-d AdvertDb C:/dump/AdvertDb");
                return "База данных восстановлена";
            }
            catch (Exception e)
            {
                return "Ошибка восстановления базы данных";
            }
        }

    }
}
