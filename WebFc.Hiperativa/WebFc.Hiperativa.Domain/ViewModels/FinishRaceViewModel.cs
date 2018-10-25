using UtilityFramework.Application.Core.ViewModels;

namespace WebFc.Hiperativa.Domain.ViewModels
{
    public class FinishRaceViewModel : BaseViewModel
    {
        public double FinalDistance { get; set; }
        public string RoutePath { get; set; }
    }
}