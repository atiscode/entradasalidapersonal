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
    
    public partial class Ingreso
    {
        public int IDIngreso { get; set; }
        public Nullable<int> TipoIngreso { get; set; }
        public Nullable<int> Empresa { get; set; }
        public Nullable<int> Area { get; set; }
        public Nullable<int> Departamento { get; set; }
        public string PersonaReemplazante { get; set; }
        public string CorreoAsignado { get; set; }
        public Nullable<decimal> Sueldo { get; set; }
        public int FichaIngresoID { get; set; }
        public bool Estado { get; set; }
        public Nullable<int> JefeDirecto { get; set; }
        public Nullable<int> Cargo { get; set; }
        public Nullable<int> TipoContrato { get; set; }
        public Nullable<int> GrupoCorreo { get; set; }
        public string CredencialAcceso { get; set; }
    }
}
