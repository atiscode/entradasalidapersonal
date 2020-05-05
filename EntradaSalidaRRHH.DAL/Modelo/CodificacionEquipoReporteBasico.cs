using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    public class CodificacionEquipoReporteBasico
    {
        public DateTime FechaCodificacionEquipo { get; set; }
        public string UsuarioCodificador { get; set; }
        public DateTime? FechaSolicitud { get; set; }
        public string NombresSolicitante { get; set; }
        public string Equipos { get; set; }
        public string HerramientasAdicionales { get; set; }
        public DateTime? FechaAsignacion { get; set; }
        public string CredencialAcceso { get; set; }
    }
}