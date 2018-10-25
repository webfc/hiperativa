using Microsoft.AspNetCore.Hosting;
using WebFc.Hiperativa.Repository.Interface;
using UtilityFramework.Infra.Core.MongoDb.Business;

namespace WebFc.Hiperativa.Repository
{
    public class StateRepository : BusinessBaseAsync<Data.Entities.State>, IStateRepository
    {
        public StateRepository(IHostingEnvironment env) : base(env)
        {
        }
    }
}