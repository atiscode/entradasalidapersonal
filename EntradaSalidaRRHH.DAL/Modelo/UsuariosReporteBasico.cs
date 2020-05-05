using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    public class UsuariosReporteBasico
    {
        public string NombresApellidos { get; set; }
        public string Identificacion { get; set; }
        public string NombreUsuario { get; set; }
        public string Departamento { get; set; }
        public string Area { get; set; }
        public string Cargo { get; set; }
        public string Pais { get; set; }
        public string Ciudad { get; set; }
        public string Direccion { get; set; }
        public string Mail { get; set; }
        public string MailCorporativo { get; set; }
        public string Telefono { get; set; }
        public string Celular { get; set; }
        public string Rol { get; set; }
        public string Estado { get; set; }
    }
}