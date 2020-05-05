using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    public class DesvinculacionPersonalCompleta
    {
        public DesvinculacionPersonalCompleta()
        {
            Desvinculacion = new DesvinculacionPersonalInfo();
            Detalles = new List<DetalleEquiposEntregadosInfo>();
        }

        public DesvinculacionPersonalCompleta(DesvinculacionPersonalInfo codificacion, List<DetalleEquiposEntregadosInfo> detalles)
        {
            Desvinculacion = codificacion;
            Detalles = detalles;
        }

        public DesvinculacionPersonalInfo Desvinculacion { get; set; }
        public List<DetalleEquiposEntregadosInfo> Detalles { get; set; }
    }
}