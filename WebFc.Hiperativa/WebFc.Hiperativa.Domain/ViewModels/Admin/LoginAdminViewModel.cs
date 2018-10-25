using System.ComponentModel.DataAnnotations;

namespace WebFc.Hiperativa.Domain.ViewModels.Admin
{
    public class LoginAdminViewModel
    {
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Login { get; set; }
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]

        public string Password { get; set; }
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [EmailAddress(ErrorMessage = DefaultMessages.EmailInvalid)]

        public string Email { get; set; }
    }
}