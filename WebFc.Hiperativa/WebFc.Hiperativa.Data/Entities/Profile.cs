using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using UtilityFramework.Infra.Core.MongoDb.Data.Modelos;

namespace WebFc.Hiperativa.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class Profile : ModelBase
    {
        public Profile()
        {
            CreditCards = new List<string>();
            DeviceId = new List<string>();
        }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string Cpf { get; set; }
        public string Password { get; set; }
        public string LastPassword { get; set; }
        public string FacebookId { get; set; }
        public string Reason { get; set; }
        public string Photo { get; set; }
        public string Phone { get; set; }

        /*USO DA IUGU*/
        public string AccountKey { get; set; }
        public string AccountKeyDev { get; set; }
        public List<string> CreditCards { get; set; }
        public List<string> DeviceId { get; set; }

        public override string CollectionName => nameof(Profile);
    }
}