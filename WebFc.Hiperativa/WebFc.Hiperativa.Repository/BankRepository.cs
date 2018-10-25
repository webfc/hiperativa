using Microsoft.AspNetCore.Hosting;
using WebFc.Hiperativa.Repository.Interface;
using UtilityFramework.Infra.Core.MongoDb.Business;
using WebFc.Hiperativa.Data.Entities;

namespace WebFc.Hiperativa.Repository
{
    public class BankRepository : BusinessBaseAsync<Bank>, IBankRepository
    {
        public BankRepository(IHostingEnvironment env) : base(env)
        {
        }
    }
}