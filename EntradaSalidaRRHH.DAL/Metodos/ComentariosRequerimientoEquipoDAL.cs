using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace EntradaSalidaRRHH.DAL.Metodos
{
    public class ComentariosRequerimientoEquipoDAL
    {
        private static readonly RRHHEntities db = new RRHHEntities();

        public static RespuestaTransaccion CrearComentariosRequerimientoEquipo(ComentariosRequerimientoEquipo objeto)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    
                    db.ComentariosRequerimientoEquipo.Add(objeto);
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

        public static RespuestaTransaccion ActualizarComentariosRequerimientoEquipo(ComentariosRequerimientoEquipo objeto)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // assume Entity base class have an Id property for all items
                    var entity = db.ComentariosRequerimientoEquipo.Find(objeto.IDComentariosRequerimientoEquipo);
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

        public static RespuestaTransaccion EliminarComentariosRequerimientoEquipo(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var entidad = db.ComentariosRequerimientoEquipo.Find(id);

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

        public static List<ComentariosRequerimientoEquipoInfo> ListadoComentariosRequerimientoEquipo(long? pagina = null, string textoBusqueda = null, string filtro = null, int? id = null)
        {
            List<ComentariosRequerimientoEquipoInfo> listado = new List<ComentariosRequerimientoEquipoInfo>();
            try
            {
                if (!id.HasValue)
                    listado = db.ListadoComentariosRequerimientoEquipo(pagina, textoBusqueda, filtro).ToList(); // Listado Completo
                else
                {
                    filtro = " WHERE IDComentariosRequerimientoEquipo = '{0}' ";
                    filtro = id.HasValue ? string.Format(filtro, id) : null;

                    listado = db.ListadoComentariosRequerimientoEquipo(null, null, filtro).ToList(); // Consulta por ID
                }
                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static ComentariosRequerimientoEquipo ConsultarComentariosRequerimientoEquipo(int id)
        {
            ComentariosRequerimientoEquipo objeto = new ComentariosRequerimientoEquipo();
            try
            {
                objeto = db.ConsultaComentariosRequerimientoEquipo(id).FirstOrDefault();
                return objeto;
            }
            catch (Exception)
            {
                return objeto;
            }
        }

        public static List<ComentariosRequerimientoEquipoInfo> ConsultarComentariosRequerimientoEquipoByRequerimientoID(int requerimientoEquipoID)
        {
            List<ComentariosRequerimientoEquipoInfo> listado = new List<ComentariosRequerimientoEquipoInfo>();
            try
            {
                string query = string.Format(" WHERE RequerimientoEquipoID = '{0}' ", requerimientoEquipoID);

                listado = db.ListadoComentariosRequerimientoEquipo(null, null, query).OrderByDescending(s=> s.Fecha).ToList(); // Listado Completo

                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

    }
}