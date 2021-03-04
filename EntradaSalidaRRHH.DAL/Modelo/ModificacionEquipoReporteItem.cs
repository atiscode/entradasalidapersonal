using System;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    public class ModificacionEquipoReporteItem
    {       
        public string NombresApellidos { get; set; }
        public string Equipo { get; set; }
        public string TipoEquipo { get; set; }          
        public string FechaModificacion { get; set; }
        public string Estado { get; set; }
        public string Observaciones { get; set; }
    }
}