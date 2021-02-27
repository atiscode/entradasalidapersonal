using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    public class IngresoReporteBasico
    {
        public string NombresApellidosUsuario { get; set; }
        public string TextoCatalogoTipoIngreso { get; set; }
        public string TextoCatalogoEmpresa { get; set; }
        public string TextoCatalogoArea { get; set; }
        public string TextoCatalogoDepartamento { get; set; }
        public string PersonaReemplazante { get; set; }
    }
}