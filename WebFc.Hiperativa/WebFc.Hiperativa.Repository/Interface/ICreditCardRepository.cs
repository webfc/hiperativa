using System.Collections.Generic;
using System.Threading.Tasks;
using UtilityFramework.Infra.Core.MongoDb.Business;
using WebFc.Hiperativa.Data.Entities;

namespace WebFc.Hiperativa.Repository.Interface
{
    public interface ICreditCardRepository : IBusinessBaseAsync<CreditCard>
    {
        Task<IEnumerable<CreditCard>> ListCreditCard(string profileId);
    }
}