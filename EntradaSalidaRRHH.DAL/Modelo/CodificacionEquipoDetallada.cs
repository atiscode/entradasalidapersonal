using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    public class CodificacionEquipoDetallada
    {
        public CodificacionEquipoDetallada() {
            Codificacion = new CodificacionEquipo();
            Detalles = new List<DetalleCodificacionEquipo>();
        }

        public CodificacionEquipoDetallada(CodificacionEquipo codificacion, List<DetalleCodificacionEquipo> detalles)
        {
            Codificacion = codificacion;
            Detalles = detalles;
        }

        public CodificacionEquipo Codificacion { get; set; }
        public List<DetalleCodificacionEquipo> Detalles { get; set; }

    }
}