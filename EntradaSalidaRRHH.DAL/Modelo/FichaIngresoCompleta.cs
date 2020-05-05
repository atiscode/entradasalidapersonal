using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    //Para los tipos complejos
    public class FichaIngresoCompleta
    {
        public FichaIngresoCompleta()
        {
            FichaIngreso = new FichasIngresosInfo();

            CargasFamiliares = new List<DetalleCargasFamiliaresInfo>();
            Experiencias = new List<DetalleExperienciasInfo>();
            Estudios = new List<DetalleEstudiosInfo>();
            DatosIngreso = new IngresosInfo();
        }

        public FichaIngresoCompleta(FichasIngresosInfo _FichaIngreso, List<DetalleCargasFamiliaresInfo> _DetalleCargasFamiliares
    , List<DetalleEstudiosInfo> _DetalleEstudios, List<DetalleExperienciasInfo> _DetalleExperiencias, IngresosInfo _DatosIngreso)
        {
            FichaIngreso = _FichaIngreso;

            CargasFamiliares = _DetalleCargasFamiliares;
            Experiencias = _DetalleExperiencias;
            Estudios = _DetalleEstudios;
            DatosIngreso = _DatosIngreso;
        }

        public FichasIngresosInfo FichaIngreso { get; set; }
        public List<DetalleCargasFamiliaresInfo> CargasFamiliares { get; set; }
        public List<DetalleExperienciasInfo> Experiencias { get; set; }
        public List<DetalleEstudiosInfo> Estudios { get; set; }
        public IngresosInfo DatosIngreso { get; set; }
    }
}