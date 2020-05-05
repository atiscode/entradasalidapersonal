using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    public class FichaIngresoDetallada
    {
        public FichaIngresoDetallada()
        {
            FichaIngreso = new FichaIngreso();

            CargasFamiliares = new List<DetalleCargasFamiliares>();
            Experiencias = new List<DetalleExperiencias>();
            Estudios = new List<DetalleEstudios>();
        }

        public FichaIngresoDetallada(FichaIngreso _FichaIngreso, List<DetalleCargasFamiliares> _DetalleCargasFamiliares
            , List<DetalleEstudios> _DetalleEstudios, List<DetalleExperiencias> _DetalleExperiencias)
        {
            FichaIngreso = _FichaIngreso;

            CargasFamiliares = _DetalleCargasFamiliares;
            Experiencias = _DetalleExperiencias;
            Estudios = _DetalleEstudios;
        }

        public FichaIngreso FichaIngreso { get; set; }
        public List<DetalleCargasFamiliares> CargasFamiliares { get; set; }
        public List<DetalleExperiencias> Experiencias { get; set; }
        public List<DetalleEstudios> Estudios { get; set; }
    }
}