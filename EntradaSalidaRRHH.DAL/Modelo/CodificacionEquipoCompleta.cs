using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    public class CodificacionEquipoCompleta
    {
        public CodificacionEquipoCompleta() {
            Codificacion = new CodificacionEquipoInfo();
            Detalles = new List<DetalleCodificacionEquipoInfo>();
        }

        public CodificacionEquipoCompleta(CodificacionEquipoInfo codificacion, List<DetalleCodificacionEquipoInfo> detalles)
        {
            Codificacion = codificacion;
            Detalles = detalles;
        }

        public CodificacionEquipoInfo Codificacion { get; set; }
        public List<DetalleCodificacionEquipoInfo> Detalles { get; set; }
    }
}