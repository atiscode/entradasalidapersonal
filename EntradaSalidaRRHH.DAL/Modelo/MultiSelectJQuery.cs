using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    public class MultiSelectJQuery
    {
        public MultiSelectJQuery(long ID, string texto, string descripcion)
        {
            value = ID;
            text = texto;
            desc = descripcion;
        }
        public long value { get; set; }
        public string text { get; set; }
        public string desc { get; set; }
    }
}