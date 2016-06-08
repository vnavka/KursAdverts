using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using System.Threading.Tasks;
using System.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;



namespace AdvertSite.Models
{

    public interface IEntity
    {
        string Id { get; set; }
    }
    public class UserContext:IEntity
    {  
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Login { get; set; }
        

        
        public string Email { get; set; }

        public string imageId { get; set; }

        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Display(Name = "Фамилия")]
        public string Surname { get; set; }

        [Display(Name = "Населенный пункт")]
        public string City { get; set; }

        [Display(Name = "Номер телефона")]
        [RegularExpression(@"[0-9]{10}", ErrorMessage = "Неверный формат")]
        public string Phone { get; set; }
    }
    public class Advert : IEntity
    {

        [BsonId]
        [HiddenInput(DisplayValue = true)]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Required(ErrorMessage="Введите название объявления")]
        [StringLength(50,MinimumLength=3,ErrorMessage="Длина от 5 до 50 символов")]
        [Display(Name = "Название объявления")]
        public string Name { get; set; }

        //public bool VIP { get; set; }

        [Required(ErrorMessage = "Выберите категорию")]
        [Display(Name = "Категория")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CategoryId { get; set; }

        [Required(ErrorMessage = "Введите описание")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Длина от 5 до 500 символов")]
        [Display(Name = "Описание")]
        public string Description { get; set; }
        
        public string Photo { get; set; }

        [Display(Name = "Цена")]
        [Range(1, 999999, ErrorMessage = "Недопустимая цена")]
        public int? Price { get; set; }
        
        public int? ViewNumber { get; set; }
        public DateTime PublicationDate { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId {get;set; }

        [BsonIgnore]
        public Category Category
        {
            get { return MongoDbContext.db.GetCollection<Category>("Category").Find(c => c.Id == CategoryId).FirstAsync().GetAwaiter().GetResult();}
        }
        [BsonIgnore]
        public UserContext User
        {
            get { return MongoDbContext.db.GetCollection<UserContext>("UserContext").Find(c => c.Id == UserId).FirstAsync().GetAwaiter().GetResult(); }
        }

         

    }
    public class Dialog : IEntity
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonId]
        public string Id { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string AdvertId { get; set; }
        
        [BsonIgnore]
        public Advert Advert
        {
            get { return MongoDbContext.db.GetCollection<Advert>("Advert").Find(c => c.Id == AdvertId).FirstAsync().GetAwaiter().GetResult(); }
        }

        public List<Message> Messages { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string userId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ownerId { get; set; }

        [BsonIgnore]
        public UserContext User
        {
            get { return MongoDbContext.db.GetCollection<UserContext>("UserContext").Find(c => c.Id == userId).FirstAsync().GetAwaiter().GetResult(); }
        }

        [BsonIgnore]
        public UserContext Owner
        {
            get { return MongoDbContext.db.GetCollection<UserContext>("UserContext").Find(c => c.Id == ownerId).FirstAsync().GetAwaiter().GetResult(); }
        }



    }
    public class Message : IEntity
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        public DateTime Datetime { get; set; }
        public string Text { get; set; }

        [BsonIgnore]
        public UserContext User
        {
            get { return MongoDbContext.db.GetCollection<UserContext>("UserContext").Find(c => c.Id == UserId).FirstAsync().GetAwaiter().GetResult(); }
        }

    }
    public class Category : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }

    }
    public class UserStatDisplay
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int AdverCount { get; set; }

    }

}