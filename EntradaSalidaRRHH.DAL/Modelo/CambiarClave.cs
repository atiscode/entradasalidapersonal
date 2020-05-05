using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    public class CambiarClave
    {
        public long UsuaCodi { get; set; }


        //[Required(ErrorMessage = "Ingrese la Contraseña Actual")]
        //[DataType(DataType.Password)]
        public string ContraseniaActual { get; set; }

        public string idUsuario { get; set; }
        //[Required(ErrorMessage = "Ingrese la Nueva Contraseña")]
        //[DataType(DataType.Password)]
        public string ContraseniaNueva { get; set; }


        //[Required(ErrorMessage = "Confirme la Nueva Contraseña")]
        //[DataType(DataType.Password)]
        public string ConfirmarContrasenia { get; set; }
    }
}