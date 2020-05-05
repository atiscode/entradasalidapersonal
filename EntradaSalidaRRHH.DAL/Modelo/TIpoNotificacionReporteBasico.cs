using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    public class TIpoNotificacionReporteBasico
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int? TiempoEspera { get; set; }
        public string Estado { get; set; }
    }
}