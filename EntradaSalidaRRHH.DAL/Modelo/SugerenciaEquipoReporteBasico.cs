using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    //TOMAR EN CUENTA EL ORDEN DE LAS PROPIEDADES PARA GENERAR EL REPORTE
    public class SugerenciaEquipoReporteBasico
    {
        public string NombreEquipo { get; set; }
        public string TextoCatalogoTipo { get; set; }
        public string Caracteristicas { get; set; }
        public string TextoCatalogoCargoSugerenciaEquipo { get; set; }
    }
}