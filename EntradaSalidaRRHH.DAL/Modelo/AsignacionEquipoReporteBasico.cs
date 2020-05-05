using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    public class AsignacionEquipoReporteBasico
    {
        public DateTime? FechaSolicitud { get; set; }
        public string NombresApellidos { get; set; }
        public string Equipos { get; set; }
        public string HerramientasAdicionales { get; set; }
        public string NombreEmpresa { get; set; }
        public string TextoCatalogoCargo { get; set; }
        public string TextoCatalogoDepartamento { get; set; }
    }
}