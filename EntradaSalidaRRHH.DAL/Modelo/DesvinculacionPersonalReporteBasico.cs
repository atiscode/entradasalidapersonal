using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    public class DesvinculacionPersonalReporteBasico
    {
        public DateTime FechaSalida { get; set; }
        public string TextoMotivo { get; set; }
        public string UsuarioDesvinculado { get; set; }
        public string UsuarioResponsableDesvinculacion { get; set; }
        public string TextoEstadoSalidaSeguroMedico { get; set; }
        public string TextoEstadoEncuestaSalida { get; set; }
    }
}