//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EntradaSalidaRRHH.DAL.Modelo
{
    using System;
    using System.Collections.Generic;
    
    public partial class RequerimientoEquipoHerramientasAdicionales
    {
        public int IDRequerimientoEquipoHerramientasAdicionales { get; set; }
        public int HerramientaAdicional { get; set; }
        public int RequerimientoEquipoID { get; set; }
        public Nullable<int> Estado { get; set; }
        public Nullable<System.DateTime> FechaModificacion { get; set; }
        public string Observaciones { get; set; }
    }
}
