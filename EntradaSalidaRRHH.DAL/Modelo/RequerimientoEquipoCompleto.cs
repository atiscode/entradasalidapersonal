using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Modelo
{
    public class RequerimientoEquipoCompleto
    {
        public RequerimientoEquipoCompleto()
        {
            RequerimientoEquipo = new RequerimientoEquipoInfo();

            EquipoUsuario = new List<RequerimientoUsuarioInfo>();
            HerramientasAdicionales = new List<RequerimientoUsuarioHerramientasadicionalesInfo>();
        }

        public RequerimientoEquipoCompleto(RequerimientoEquipoInfo _RequerimientoEquipo, List<RequerimientoUsuarioInfo> _EquipoUsuario
   , List<RequerimientoUsuarioHerramientasadicionalesInfo> _HerramientasAdicionales)
        {
            RequerimientoEquipo = _RequerimientoEquipo;

            EquipoUsuario = _EquipoUsuario;
            HerramientasAdicionales = _HerramientasAdicionales;
        }

        public RequerimientoEquipoInfo RequerimientoEquipo { get; set; }
        public List<RequerimientoUsuarioInfo> EquipoUsuario { get; set; }
        public List<RequerimientoUsuarioHerramientasadicionalesInfo> HerramientasAdicionales { get; set; }
    }
}