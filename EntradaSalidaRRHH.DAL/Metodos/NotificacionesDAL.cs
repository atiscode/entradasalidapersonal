using EntradaSalidaRRHH.DAL.Helpers;
using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Metodos
{
    public class NotificacionesDAL
    {
        private static readonly NotificadorAtiscodeEntities db = new NotificadorAtiscodeEntities();

        public static List<NotificacionesInfo> ListarNotificaciones()
        {
            List<NotificacionesInfo> notificaciones = new List<NotificacionesInfo>();
            try
            {
                notificaciones = db.ListadoNotificacionesRRHH().ToList();
                return notificaciones;
            }
            catch (Exception ex)
            {
                return notificaciones;
            }
        }

        public static RespuestaTransaccion CrearNotificacion(Notificaciones notificacion)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (!Configurations.EnvioDeCorreosProduccionActivado())
                    {
                        notificacion.CorreosDestinarios = string.Join(";",CatalogoDAL.ListadoCatalogosPorCodigoPadre("CORREOS-PRUEBAS", "RRHH").Select(t => t.DescripcionCatalogo));
                    }
                    //Verificar si se envía la clave encriptada
                    db.InsertarNotificacionAtiscode(notificacion.NombreTarea, notificacion.DescripcionTarea, notificacion.NombreEmisor, notificacion.CorreoEmisor, notificacion.ClaveCorreo, notificacion.CorreosDestinarios, notificacion.AsuntoCorreo, notificacion.NombreArchivoPlantillaCorreo, notificacion.CuerpoCorreo, notificacion.AdjuntosCorreo, notificacion.FechaEnvioCorreo, notificacion.DetalleEstadoEjecucionNotificacion, notificacion.Empresa, notificacion.Canal, notificacion.Tipo);

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

        public static RespuestaTransaccion CancelarNotificacion(int id)
        {
            try
            {
                using (var context = new NotificadorAtiscodeEntities())
                {
                    var notificacion = context.Notificaciones.Find(id);

                    // Se puede cancelar,  Siempre y cuando la notificacion no haya sido enviada
                    if (!notificacion.EstadoEnviadoNotificacion.Value)
                    {
                        notificacion.EstadoNotificacion = false;
                        notificacion.EstadoEjecucionNotificacion = false;
                        notificacion.EstadoEnviadoNotificacion = false;

                        notificacion.DetalleEstadoEjecucionNotificacion = "Envío cancelado. " + System.DateTime.Now;

                        context.Entry(notificacion).State = EntityState.Modified;
                        context.SaveChanges();

                        return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
                    }
                    else
                    {
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeValidacionNotificacionEnviada + " " + Mensajes.MensajeTransaccionFallida };
                    }
                }
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }

        public static RespuestaTransaccion SaveInBatchNotifications(List<Notificaciones> listado)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var notificacion in listado)
                    {
                        var respuestaOperacionDB = db.InsertarNotificacionAtiscodeCatchingResults(notificacion.NombreTarea, notificacion.DescripcionTarea, notificacion.NombreEmisor, notificacion.CorreoEmisor, notificacion.ClaveCorreo, notificacion.CorreosDestinarios, notificacion.AsuntoCorreo, notificacion.NombreArchivoPlantillaCorreo, notificacion.CuerpoCorreo, notificacion.AdjuntosCorreo, notificacion.FechaEnvioCorreo, notificacion.DetalleEstadoEjecucionNotificacion, notificacion.Empresa, notificacion.Canal, notificacion.Tipo).FirstOrDefault();
                        // Si la transacción NO se ejecutó correctamente
                        if (!respuestaOperacionDB.Estado.Value)
                        {
                            transaction.Rollback();
                            return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + respuestaOperacionDB.Respuesta };
                        }
                    }

                    db.SaveChanges(); // Only Once
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

    }
}