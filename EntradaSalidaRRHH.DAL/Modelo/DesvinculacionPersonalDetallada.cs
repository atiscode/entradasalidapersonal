using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    public class DesvinculacionPersonalDetallada
    {
        public DesvinculacionPersonalDetallada()
        {
            Desvinculacion = new DesvinculacionPersonal();
            Detalles = new List<DetalleEquiposEntregados>();
        }

        public DesvinculacionPersonalDetallada(DesvinculacionPersonal codificacion, List<DetalleEquiposEntregados> detalles)
        {
            Desvinculacion = codificacion;
            Detalles = detalles;
        }

        public DesvinculacionPersonal Desvinculacion { get; set; }
        public List<DetalleEquiposEntregados> Detalles { get; set; }
    }
}