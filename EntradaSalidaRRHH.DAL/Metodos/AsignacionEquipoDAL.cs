using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace EntradaSalidaRRHH.DAL.Metodos
{
    public class AsignacionEquipoDAL
    {
        private static readonly RRHHEntities db = new RRHHEntities();

        public static RespuestaTransaccion CrearAsignacionEquipo(AsignacionEquipo objeto)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    db.AsignacionEquipo.Add(objeto);
                    db.SaveChanges();

                    transaction.Commit();

                    return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
                }
            }
        }

        public static RespuestaTransaccion ActualizarAsignacionEquipo(AsignacionEquipo objeto)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // assume Entity base class have an Id property for all items
                    var entity = db.AsignacionEquipo.Find(objeto.IDAsignacionEquipo);
                    if (entity == null)
                    {
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida };
                    }

                    db.Entry(entity).CurrentValues.SetValues(objeto);
                    db.SaveChanges();

                    transaction.Commit();

                    return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
                }
            }
        }

        public static RespuestaTransaccion EliminarAsignacionEquipo(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var entidad = db.AsignacionEquipo.Find(id);

                    entidad.Estado = false;

                    db.Entry(entidad).State = EntityState.Modified;
                    db.SaveChanges();

                    transaction.Commit();
                    return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
                }
            }
        }

        public static RespuestaTransaccion AsignarEquipo(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var entidad = db.RequerimientoEquipo.Find(id);

                    entidad.Asignado = true;
                    entidad.FechaAsignacion = DateTime.Now;

                    db.Entry(entidad).State = EntityState.Modified;
                    db.SaveChanges();

                    transaction.Commit();
                    return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
                }
            }
        }

        public static List<AsignacionEquipoInfo> ListadoAsignacionEquipo(long? pagina = null, string textoBusqueda = null, string filtro = null, int? id = null)
        {
            List<AsignacionEquipoInfo> listado = new List<AsignacionEquipoInfo>();
            try
            {
                if (!id.HasValue)
                    listado = db.ListadoAsignacionEquipo(pagina, textoBusqueda, filtro).ToList(); // Listado Completo
                else
                {
                    filtro = " WHERE IDRequerimientoEquipo = '{0}' ";
                    filtro = id.HasValue ? string.Format(filtro, id) : null;

                    listado = db.ListadoAsignacionEquipo(null, null, filtro).ToList(); // Consulta por ID
                }
                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static AsignacionEquipo ConsultarAsignacionEquipo(int id)
        {
            AsignacionEquipo objeto = new AsignacionEquipo();
            try
            {
                objeto = db.ConsultarAsignacionesEquipo(id).FirstOrDefault();
                return objeto;
            }
            catch (Exception)
            {
                return objeto;
            }
        }

        public static int ObtenerTotalRegistrosListadoAsignacionEquipo()
        {
            int total = 0;
            try
            {
                total = db.Database.SqlQuery<int>("SELECT [dbo].[ObtenerTotalRegistrosListadoAsignacionEquipo]()").Single();
                return total;
            }
            catch (Exception ex)
            {
                return total;
            }
        }

        public static List<AsignacionEquipoReporteBasico> ListadoReporteBasico()
        {
            List<AsignacionEquipoReporteBasico> listado = new List<AsignacionEquipoReporteBasico>();
            try
            {
                listado = ListadoAsignacionEquipo().Select(s => new AsignacionEquipoReporteBasico
                {
                    FechaSolicitud = s.FechaSolicitud,
                    NombresApellidos = s.NombresApellidos,
                    Equipos = s.Equipos,
                    HerramientasAdicionales = s.HerramientasAdicionales,
                    NombreEmpresa = s.NombreEmpresa,
                    TextoCatalogoCargo = s.TextoCatalogoCargo,
                    TextoCatalogoDepartamento = s.TextoCatalogoDepartamento,
                }).ToList();

                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }
    }
}