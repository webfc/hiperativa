using Microsoft.Extensions.DependencyInjection;
using UtilityFramework.Services.Core;
using UtilityFramework.Services.Core.Interface;
using UtilityFramework.Services.Iugu.Core;
using UtilityFramework.Services.Iugu.Core.Interface;

namespace WebFc.Hiperativa.WebApi.Middleware
{

    public static class IocContainer
    {
        /// <summary>
        /// INJEÇÃO DE DEPENDENCIAS DE REPOSITORIO DE ACESSO A DADOS DO BANCO
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddRepositoryInjection(this IServiceCollection services)
        {
            /*REPOSITORIES*/
            services.AddSingleton(typeof(ICreditCardRepository), typeof(CreditCardRepository));
            services.AddSingleton(typeof(IUserAdministratorRepository), typeof(UserAdministratorRepository));
            services.AddSingleton(typeof(ICityRepository), typeof(CityRepository));
            services.AddSingleton(typeof(IBankRepository), typeof(BankRepository));
            services.AddSingleton(typeof(IStateRepository), typeof(StateRepository));
            services.AddSingleton(typeof(IProfileRepository), typeof(ProfileRepository));

            return services;
        }

        /// <summary>
        /// INJEÇÃO DE DEPENDENCIAS DE SERVIÇOS DE API
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddServicesInjection(this IServiceCollection services)
        {

            /*IUGU*/
            services.AddSingleton(typeof(IIuguChargeServices), typeof(IuguService));
            services.AddSingleton(typeof(IIuguMarketPlaceServices), typeof(IuguService));
            services.AddSingleton(typeof(IIuguPaymentMethodService), typeof(IuguService));
            services.AddSingleton(typeof(IIuguCustomerServices), typeof(IuguService));
            services.AddSingleton(typeof(IIuguService), typeof(IuguService));

            /* NOTIFICAÇÕES & EMAIL*/
            services.AddSingleton(typeof(ISenderMailService), typeof(SendService));
            services.AddSingleton(typeof(ISenderNotificationService), typeof(SendService));

            /*FIREBASE*/
            services.AddSingleton(typeof(IFirebaseServices), typeof(FirebaseServices));

            /*UTILIDADES */
            services.AddSingleton(typeof(IUtilService), typeof(UtilService));

            return services;
        }
    }
}