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
    
    public partial class DetalleEstudios
    {
        public int IDDetalleEstudios { get; set; }
        public Nullable<int> TipoEstudio { get; set; }
        public Nullable<int> Institucion { get; set; }
        public Nullable<int> Titulo { get; set; }
        public Nullable<int> AnioFinalizacion { get; set; }
        public Nullable<int> Ciudad { get; set; }
        public Nullable<int> Pais { get; set; }
        public int FichaIngresoID { get; set; }
        public Nullable<bool> Finalizado { get; set; }
    }
}
