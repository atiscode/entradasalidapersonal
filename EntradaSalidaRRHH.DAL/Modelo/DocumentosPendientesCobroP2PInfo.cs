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
    
    public partial class DocumentosPendientesCobroP2PInfo
    {
        public string NumeroFactura { get; set; }
        public string ClaveCliente { get; set; }
        public string NombreCliente { get; set; }
        public string Ruc { get; set; }
        public Nullable<System.DateTime> FechaEmision { get; set; }
        public string FechaVence { get; set; }
        public Nullable<decimal> ValorFactura { get; set; }
        public Nullable<decimal> Abonos { get; set; }
        public Nullable<decimal> Saldo { get; set; }
        public Nullable<int> Dias { get; set; }
        public Nullable<decimal> PorVencer { get; set; }
        public Nullable<decimal> Vencido { get; set; }
        public string Detalle { get; set; }
    }
}
