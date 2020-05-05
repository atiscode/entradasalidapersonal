using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    public class CargaMasiva
    {
        public CargaMasiva() {
            Detalles = new List<DetallesCargaMasiva>();
        }
        public bool OK;
        public List<DetallesCargaMasiva> Detalles { get; set; }
        public bool GetEstado() {
            if (!Detalles.Any())
                return true;
            else
                return false;
        }
    }

    public class DetallesCargaMasiva {
        public long Fila { get; set; }
        public long Columna { get; set; }
        public string Valor { get; set; }
        public string Error { get; set; }
    }
}