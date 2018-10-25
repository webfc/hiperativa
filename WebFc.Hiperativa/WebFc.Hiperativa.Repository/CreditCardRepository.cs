using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using MongoDB.Driver;
using WebFc.Hiperativa.Repository.Interface;
using UtilityFramework.Infra.Core.MongoDb.Business;
using WebFc.Hiperativa.Data.Entities;

namespace WebFc.Hiperativa.Repository
{
    public class CreditCardRepository : BusinessBaseAsync<CreditCard>, ICreditCardRepository
    {
        public async Task<IEnumerable<CreditCard>> ListCreditCard(string profileId) => await FindByAsync(x => x.ProfileId == profileId, Builders<CreditCard>.Sort.Descending(x => x.Created));

        public CreditCardRepository(IHostingEnvironment env) : base(env)
        {
        }
    }
}