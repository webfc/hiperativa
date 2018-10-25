using System.ComponentModel.DataAnnotations;
using UtilityFramework.Application.Core.ViewModels;

namespace WebFc.Hiperativa.Domain.ViewModels
{
    public class CreditCardViewModel : BaseViewModel
    {
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Name { get; set; }
        [MinLength(14, ErrorMessage = DefaultMessages.Minlength)]
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Number { get; set; }
        [Range(1, 12, ErrorMessage = DefaultMessages.Range)]
        public int ExpMonth { get; set; }
        [MinLength(3, ErrorMessage = DefaultMessages.Minlength)]
        [MaxLength(4, ErrorMessage = DefaultMessages.Maxlength)]
        public string Cvv { get; set; }
        public int ExpYear { get; set; }
        public string Flag { get; set; }
        public string ProfileId { get; set; }
        public string TokenCard { get; set; }
    }
}