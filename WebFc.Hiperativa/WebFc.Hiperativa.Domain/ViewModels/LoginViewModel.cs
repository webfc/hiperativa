using System.ComponentModel.DataAnnotations;

namespace WebFc.Hiperativa.Domain.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Login { get; set; }
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [EmailAddress(ErrorMessage = DefaultMessages.EmailInvalid)]
        public string Email { get; set; }
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Password { get; set; }
        public string FacebookId { get; set; }
        public string GoogleId { get; set; }
        public string RefreshToken { get; set; }
    }
}