using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace EntradaSalidaRRHH.DAL.Metodos
{
    public class IngresoDAL
    {
        private static readonly RRHHEntities db = new RRHHEntities();

        public static RespuestaTransaccion CrearIngreso(Ingreso objeto)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    db.Ingreso.Add(objeto);
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

        public static RespuestaTransaccion ActualizarIngreso(Ingreso objeto)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    bool ingresoExistente = db.Ingreso.Any(s => s.IDIngreso != objeto.IDIngreso && s.FichaIngresoID == objeto.FichaIngresoID && s.Estado);
                    bool emailAsignadoExistente = db.Ingreso.Any(s => s.IDIngreso != objeto.IDIngreso && s.CorreoAsignado == objeto.CorreoAsignado && s.Estado);

                    if (emailAsignadoExistente)
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeEmailExistenteAsociadoUsuario };

                    if (ingresoExistente)
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeFichaIngresoUsuarioExistente };

                    // assume Entity base class have an Id property for all items
                    var entity = db.Ingreso.Find(objeto.IDIngreso);
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

        public static RespuestaTransaccion EliminarIngreso(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var entidad = db.Ingreso.Find(id);

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

        public static List<IngresosInfo> ListadoIngreso(long? pagina = null, string textoBusqueda = null, string filtro = null, int? id = null)
        {
            List<IngresosInfo> listado = new List<IngresosInfo>();
            try
            {
                if (!id.HasValue)
                    listado = db.ListadoIngresos(pagina, textoBusqueda, filtro).ToList(); // Listado Completo
                else
                {
                    filtro = " WHERE IDIngreso = '{0}' ";
                    filtro = id.HasValue ? string.Format(filtro, id) : null;

                    listado = db.ListadoIngresos(null, null, filtro).ToList(); // Consulta por ID
                }
                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static Ingreso ConsultarIngreso(int id)
        {
            Ingreso objeto = new Ingreso();
            try
            {
                objeto = db.ConsultarIngreso(id).FirstOrDefault();
                return objeto;
            }
            catch (Exception)
            {
                return objeto;
            }
        }

        public static IngresosInfo ConsultarIngresoByUsuarioID(int usuarioID)
        {
            IngresosInfo objeto = new IngresosInfo();
            try
            {
                objeto = ListadoIngreso().Where(s => s.UsuarioID == usuarioID).FirstOrDefault();
                return objeto;
            }
            catch (Exception)
            {
                return objeto;
            }
        }

        public static bool IngresoExistente(int FichaIngresoID)
        {
            try
            {
                bool existe = db.Ingreso.Any(s => s.FichaIngresoID == FichaIngresoID && s.Estado);
                return existe;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool EmailAsignadoExistente(string emailAsignado)
        {
            try
            {
                bool existe = db.Ingreso.Any(s => s.CorreoAsignado == emailAsignado && s.Estado);
                return existe;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static List<IngresoReporteBasico> ListadoReporteBasico()
        {
            List<IngresoReporteBasico> listado = new List<IngresoReporteBasico>();
            try
            {
                listado = ListadoIngreso().Select(s => new IngresoReporteBasico
                {
                    TextoCatalogoTipoIngreso = s.TextoCatalogoTipoIngreso,
                    TextoCatalogoEmpresa = s.TextoCatalogoEmpresa,
                    TextoCatalogoArea = s.TextoCatalogoArea,
                    TextoCatalogoDepartamento = s.TextoCatalogoDepartamento,
                    PersonaReemplazante = s.PersonaReemplazante,
                }).ToList();

                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static int ObtenerTotalRegistrosListadoIngreso()
        {
            int total = 0;
            try
            {
                total = db.Database.SqlQuery<int>("SELECT [dbo].[ObtenerTotalRegistrosListadoIngresos]()").Single();
                return total;
            }
            catch (Exception ex)
            {
                return total;
            }
        }

    }
}