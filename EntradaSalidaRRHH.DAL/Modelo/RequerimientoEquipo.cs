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
    
    public partial class RequerimientoEquipo
    {
        public int IDRequerimientoEquipo { get; set; }
        public System.DateTime FechaSolicitud { get; set; }
        public int UsuarioID { get; set; }
        public bool Estado { get; set; }
        public int UsuarioSolicitanteID { get; set; }
        public string CredencialAcceso { get; set; }
        public Nullable<bool> Aprobado { get; set; }
        public Nullable<System.DateTime> FechaAprobacion { get; set; }
        public bool Asignado { get; set; }
        public Nullable<System.DateTime> FechaAsignacion { get; set; }
        public Nullable<System.DateTime> FechaEstimadaEntregaEquipos { get; set; }
    }
}
