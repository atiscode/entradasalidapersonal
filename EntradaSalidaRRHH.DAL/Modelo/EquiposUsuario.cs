using System.Collections.Generic;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    public class EquiposUsuario
    {
        public EquiposUsuario()
        {
            Equipos = new List<ListadoEquiposAsignadosUsuario_Result>();
        }

        public List<ListadoEquiposAsignadosUsuario_Result> Equipos { get; set; }
    }
}