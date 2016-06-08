using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using System.Threading.Tasks;
using System.Configuration;
using WebMatrix.WebData;
using System.Web.Security;
using System.IO;

namespace AdvertSite.Models
{
    public class MongoDbContext
    {
        public static IMongoDatabase db { get; private set; }

        //public static MongoClient client;
        static MongoDbContext()
        {
            string connect = ConfigurationManager.ConnectionStrings["MongoConnect"].ConnectionString;
            var client = new MongoClient(connect);
            db = client.GetDatabase("AdvertDb");
 


        }
        public void AddUser(UserContext user)
        {
            db.GetCollection<UserContext>("UserContext").InsertOneAsync(user).Wait();
        }
        public void AddAdvert(Advert advert)
        {
            db.GetCollection<Advert>("Advert").InsertOneAsync(advert).Wait();
        }
        public void AddDialog(Dialog dialog)
        {
            db.GetCollection<Dialog>("Dialog").InsertOneAsync(dialog).Wait();
        }
        public void AddMessage(Message message)
        {
            db.GetCollection<Message>("Message").InsertOneAsync(message).Wait();
        }
        public static UserContext getUser(string userName)
        {
            var t = db.GetCollection<UserContext>("UserContext").Find(u => u.Login == userName).FirstOrDefaultAsync();
            return t.Result;
        }
        public static void ManageDialogs()
        {
            db.GetCollection<Dialog>("Dialog").DeleteMany(c => c.Messages.Count == 0);
        }
        public static bool DeleteAdvert(string Id)
        {
            try
            {
                db.GetCollection<Dialog>("Dialog").DeleteMany(c => c.AdvertId == Id);
                db.GetCollection<Advert>("Advert").DeleteOne(a => a.Id == Id);
                return true;

            }
            catch(Exception e)
            {
                return false;
            }
        }
        public static bool DeleteUser(string Id)
        {
            try
            {
                var adverts = db.GetCollection<Advert>("Advert").Find(c => c.UserId == Id).ToList();
                foreach (var item in adverts)
                    DeleteAdvert(item.Id);
                var user = MongoDbContext.db.GetCollection<UserContext>("UserContext").Find(c => c.Id == Id).First();

                MongoDbContext.db.GetCollection<UserContext>("UserContext").DeleteOne(c => c.Id == Id);
                //((SimpleMembershipProvider)Membership.Provider).DeleteUser(user.Login,true);

                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static bool DeleteCat(string Id)
        {
            try
            {
                var adverts = db.GetCollection<Advert>("Advert").Find(c => c.CategoryId == Id).ToList();
                foreach (var item in adverts)
                    DeleteAdvert(item.Id);
                db.GetCollection<Category>("Category").DeleteOne(a => a.Id == Id);
                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static bool UserExists(string name)
        {
            if (name == "admin")
                return true;
            else
            {
                var u = db.GetCollection<UserContext>("UserContext").Find(c => c.Login == name).FirstOrDefault();
                return u != null;
            }

        }

    }

}