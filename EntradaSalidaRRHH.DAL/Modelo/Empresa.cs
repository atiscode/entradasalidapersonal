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
    
    public partial class Empresa
    {
        public int IdEmpresa { get; set; }
        public string CedulaRucPasaporte { get; set; }
        public string RazonSocial { get; set; }
        public string NombreComercial { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string PaginaWeb { get; set; }
        public string Logo { get; set; }
        public string RepresentanteLegal { get; set; }
        public Nullable<bool> ObligatorioLlevarContabilidad { get; set; }
    }
}
