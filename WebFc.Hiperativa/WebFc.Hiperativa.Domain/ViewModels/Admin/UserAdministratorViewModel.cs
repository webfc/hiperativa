using System.ComponentModel.DataAnnotations;
using UtilityFramework.Application.Core.ViewModels;

namespace WebFc.Hiperativa.Domain.ViewModels.Admin
{
    public class UserAdministratorViewModel : BaseViewModel
    {
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Login { get; set; }
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Password { get; set; }
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Name { get; set; }
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Email { get; set; }
        public int Level { get; set; }
        public bool Blocked { get; set; }
    }
}