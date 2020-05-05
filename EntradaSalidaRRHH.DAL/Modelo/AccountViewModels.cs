using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace EntradaSalidaRRHH.DAL.Modelo
{
   
        public class ExternalLoginConfirmationViewModel
        {
            [Required]
            [Display(Name = "Usuario")]
            public string Login { get; set; }
        }

        public class ExternalLoginListViewModel
        {
            public string ReturnUrl { get; set; }
        }

        public class SendCodeViewModel
        {
            public string SelectedProvider { get; set; }
            public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
            public string ReturnUrl { get; set; }
            public bool RememberMe { get; set; }
        }


        public class ForgotViewModel
        {
            [Required]
            [Display(Name = "Usuario")]
            public string Login { get; set; }
        }

        public class LoginViewModel
        {
            [Required]
            [Display(Name = "Usuario")]
            public string Login { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

        }

        public class RegisterViewModel
        {
            [Required]
            [Display(Name = "Nombre")]
            public string Nombre { get; set; }

            [Required]
            [Display(Name = "Apellido")]
            public string Apellidos { get; set; }

            [Required]
            [Display(Name = "Identificación")]
            public string Identificacion { get; set; }

            [Required]
            [Display(Name = "User Name")]
            public string UserName { get; set; }

            [Required]
            [Display(Name = "Email")]
            public string Email { get; set; }

            public string telefono_usuario { get; set; }
            public string celular_usuario { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "El número de caracteres de {0} debe ser al menos {2}.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar contraseña")]
            [Compare("Password", ErrorMessage = "La contraseña y la contraseña de confirmación no coinciden.")]
            public string ConfirmPassword { get; set; }
        }

        public class ResetPasswordViewModel
        {
            [Required]
            [Display(Name = "Usuario")]
            public string Login { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "El número de caracteres de {0} debe ser al menos {2}.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar contraseña")]
            [Compare("Password", ErrorMessage = "La contraseña y la contraseña de confirmación no coinciden.")]
            public string ConfirmPassword { get; set; }

            public string Code { get; set; }
        }

        public class ForgotPasswordViewModel
        {
            [Required]
            [Display(Name = "Usuario")]
            public string Login { get; set; }
        }
    }
