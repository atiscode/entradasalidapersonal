using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Metodos
{
    public class TipoNotificacionDAL
    {
        private static readonly AdministracionEntities db = new AdministracionEntities();

        public static RespuestaTransaccion CrearTipoNotificacion(TipoNotificacion tipoNotificacion)
        {
            try
            {
                tipoNotificacion.NombreNotificacion = tipoNotificacion.NombreNotificacion.ToUpper();
                tipoNotificacion.EstadoNotificacion = true;
                db.TipoNotificacion.Add(tipoNotificacion);
                db.SaveChanges();

                return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }

        public static RespuestaTransaccion ActualizarTipoNotificacion(TipoNotificacion tipoNotificacion)
        {
            try
            {
                // Por si queda el Attach de la entidad y no deja actualizar
                var local = db.TipoNotificacion.FirstOrDefault(f => f.IdNotificacion == tipoNotificacion.IdNotificacion);
                if (local != null)
                {
                    db.Entry(local).State = EntityState.Detached;
                }

                tipoNotificacion.NombreNotificacion = tipoNotificacion.NombreNotificacion.ToUpper();
                db.Entry(tipoNotificacion).State = EntityState.Modified;
                db.SaveChanges();

                return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }

        //Eliminación Lógica
        public static RespuestaTransaccion EliminarTipoNotificacion(int id)
        {
            try
            {
                var TipoNotificacion = db.TipoNotificacion.Find(id);

                if (TipoNotificacion.EstadoNotificacion == true)
                {
                    TipoNotificacion.EstadoNotificacion = false;
                }
                else
                {
                    TipoNotificacion.EstadoNotificacion = true;
                }


                db.Entry(TipoNotificacion).State = EntityState.Modified;
                db.SaveChanges();

                return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }

        public static List<TipoNotificacionesInfo> ListarTipoNotificaciones()
        {
            try
            {
                return db.ListadoTipoNotificaciones().ToList();
            }
            catch (Exception ex)
            {
                return new List<TipoNotificacionesInfo>();
            }
        }

        public static TipoNotificacion ConsultarNotificacion(int id)
        {
            try
            {
                TipoNotificacion tipoNotificacion = db.ConsultarTipoNotificacion(id).FirstOrDefault() ?? new TipoNotificacion();
                return tipoNotificacion;
            }
            catch (Exception ex)
            {
                return new TipoNotificacion();
            }
        }
    }
}