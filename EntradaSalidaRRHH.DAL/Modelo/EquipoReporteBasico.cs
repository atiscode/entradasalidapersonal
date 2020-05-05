using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    public class EquipoReporteBasico
    {
        public string NombreEquipo { get; set; }
        public string DescripcionEquipo { get; set; }
        public string TextoCatalogoTipo { get; set; }
        public string Caracteristicas { get; set; }
        public string Observaciones { get; set; }
        public decimal? Costo { get; set; }
        public string TextoCatalogoProveedor { get; set; }
    }
}