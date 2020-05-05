using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace EntradaSalidaRRHH.DAL.Metodos
{
    public class AprobacionRequerimientoEquipoDAL
    {
        private static readonly RRHHEntities db = new RRHHEntities();

        public static List<AprobacionRequerimientoEquipoInfo> ListadoAprobacionRequerimientoEquipo(long? pagina = null, string textoBusqueda = null, string filtro = null, int? id = null)
        {
            List<AprobacionRequerimientoEquipoInfo> listado = new List<AprobacionRequerimientoEquipoInfo>();
            try
            {
                if (!id.HasValue)
                    listado = db.ListadoAprobacionRequerimientoEquipo(pagina, textoBusqueda, filtro).ToList(); // Listado Completo
                else
                {
                    filtro = " WHERE IDRequerimientoEquipo = '{0}' ";
                    filtro = id.HasValue ? string.Format(filtro, id) : null;

                    listado = db.ListadoAprobacionRequerimientoEquipo(null, null, filtro).ToList(); // Consulta por ID
                }
                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static int ObtenerTotalRegistrosListadoAprobacionRequerimientoEquipo()
        {
            int total = 0;
            try
            {
                total = db.Database.SqlQuery<int>("SELECT [dbo].[ObtenerTotalRegistrosListadoAprobacionRequerimientoEquipo]()").Single();
                return total;
            }
            catch (Exception ex)
            {
                return total;
            }
        }

    }
}