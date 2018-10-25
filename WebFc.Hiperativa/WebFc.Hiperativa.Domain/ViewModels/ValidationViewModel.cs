using System.ComponentModel.DataAnnotations;

namespace WebFc.Hiperativa.Domain.ViewModels
{
    public class ValidationViewModel
    {

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [EmailAddress(ErrorMessage = DefaultMessages.EmailInvalid)]
        public string Email { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Cpf { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]

        public string Cnpj { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Login { get; set; }

    }
}