using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntradaSalidaRRHH.Repositorios
{
    public class RespuestaTransaccion
    {
        public bool Estado { get; set; }
        public string Respuesta { get; set; }
        public string Adicional { get; set; }
        // Parametro generico EntidadID
        public long? EntidadID { get; set; }
    }
}
