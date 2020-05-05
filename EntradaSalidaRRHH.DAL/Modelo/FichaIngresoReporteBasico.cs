using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    public class FichaIngresoReporteBasico
    {
        public string NombresApellidosUsuario { get; set; }
        public string identificacion { get; set; }
        public string NombreEmpresa { get; set; }
        public DateTime? FechaIngreso { get; set; }
    }
}