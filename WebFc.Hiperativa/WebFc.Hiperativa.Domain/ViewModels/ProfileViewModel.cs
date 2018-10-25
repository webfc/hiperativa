using System.ComponentModel.DataAnnotations;
using UtilityFramework.Application.Core.ViewModels;

namespace WebFc.Hiperativa.Domain.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string FullName { get; set; }
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Login { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [EmailAddress(ErrorMessage = DefaultMessages.EmailInvalid)]
        public string Email { get; set; }
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]

        public string Photo { get; set; }
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Cpf { get; set; }
        public string Phone { get; set; }
        public bool Blocked { get; set; }
    }
}