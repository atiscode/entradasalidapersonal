//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EntradaSalidaRRHH.DAL.Modelo
{
    using System;
    using System.Collections.Generic;
    
    public partial class DetalleCodificacionEquipo
    {
        public int IDDetalleCodificacionEquipo { get; set; }
        public int CodificacionEquipoID { get; set; }
        public string Codigo { get; set; }
        public string Factura { get; set; }
        public int EquipoID { get; set; }
        public string SerieEquipo { get; set; }
        public System.DateTime FechaCompra { get; set; }
        public string Observaciones { get; set; }
        public string Adjunto { get; set; }
    }
}
