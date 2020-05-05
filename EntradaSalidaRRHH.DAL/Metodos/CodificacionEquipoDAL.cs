using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Metodos
{
    public class CodificacionEquipoDAL
    {
        private static readonly RRHHEntities db = new RRHHEntities();

        public static RespuestaTransaccion CrearCodificacionEquipo(CodificacionEquipo objeto, List<DetalleCodificacionEquipo> detalles)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var equiposDuplicados = detalles.GroupBy(x => x.EquipoID).Any(g => g.Count() > 1);

                    var detallesFactura = detalles.Select(s => s.Factura).ToList();
                    var detallesSerieEquipo = detalles.Select(s => s.SerieEquipo).ToList();

                    bool facturaRegistrada = db.DetalleCodificacionEquipo.Any(s => detallesFactura.Contains(s.Factura));

                    bool serieEquipo = db.DetalleCodificacionEquipo.Any(s => detallesSerieEquipo.Contains(s.SerieEquipo));

                    if (equiposDuplicados)
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeEquiposRepetidos };

                    //Si la factura ya se encuentra en el sistema
                    if (facturaRegistrada)
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeFacturaRepetida };

                    if (serieEquipo)
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeSerieEquipoRepetida };

                    db.CodificacionEquipo.Add(objeto);
                    db.SaveChanges();

                    int CodificacionEquipoID = objeto.IDCodificacionEquipo;

                    foreach (var item in detalles)
                    {
                        item.CodificacionEquipoID = CodificacionEquipoID;
                        db.DetalleCodificacionEquipo.Add(item);
                        db.SaveChanges();
                    }

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

        public static RespuestaTransaccion ActualizarCodificacionEquipo(CodificacionEquipo objeto, List<DetalleCodificacionEquipo> detalles)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var equiposDuplicados = detalles.GroupBy(x => x.EquipoID).Any(g => g.Count() > 1);

                    var detallesIDs = detalles.Select(s => s.IDDetalleCodificacionEquipo).ToList();

                    var detallesFactura = detalles.Select(s => s.Factura).ToList();
                    var detallesSerieEquipo = detalles.Select(s => s.SerieEquipo).ToList();

                    bool facturaRegistrada = db.DetalleCodificacionEquipo.Any(s => detallesFactura.Contains(s.Factura) && !detallesIDs.Contains(s.IDDetalleCodificacionEquipo));

                    bool serieEquipo = db.DetalleCodificacionEquipo.Any(s => detallesSerieEquipo.Contains(s.SerieEquipo) && !detallesIDs.Contains(s.IDDetalleCodificacionEquipo));

                    if (equiposDuplicados)
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeEquiposRepetidos };

                    //Si la factura ya se encuentra en el sistema
                    if (facturaRegistrada)
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeFacturaRepetida };

                    if (serieEquipo)
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeSerieEquipoRepetida };

                    // assume Entity base class have an Id property for all items
                    var entity = db.CodificacionEquipo.Find(objeto.IDCodificacionEquipo);
                    if (entity == null)
                    {
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida };
                    }

                    db.Entry(entity).CurrentValues.SetValues(objeto);
                    db.SaveChanges();

                    int fechaIngresoID = objeto.IDCodificacionEquipo;

                    #region Actualizar Detalles
                    // Limpiar primero los detalles anteriores 
                    var detallesAnteriores = db.DetalleCodificacionEquipo.Where(s => s.CodificacionEquipoID == fechaIngresoID).ToList();
                    foreach (var item in detallesAnteriores)
                    {
                        db.DetalleCodificacionEquipo.Remove(item);
                        db.SaveChanges();
                    }
                    //Registrar nuevos detalles
                    foreach (var item in detalles)
                    {
                        item.CodificacionEquipoID = fechaIngresoID;
                        db.DetalleCodificacionEquipo.Add(item);
                        db.SaveChanges();
                    }
                    #endregion

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

        public static RespuestaTransaccion EliminarCodificacionEquipo(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var entidad = db.CodificacionEquipo.Find(id);

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

        public static List<CodificacionEquipoInfo> ListadoCodificacionEquipo(long? pagina = null, string textoBusqueda = null, string filtro = null, int? id = null)
        {
            List<CodificacionEquipoInfo> listado = new List<CodificacionEquipoInfo>();
            try
            {
                if (!id.HasValue)
                    listado = db.ListadoCodificacionEquipo(pagina, textoBusqueda, filtro).ToList(); // Listado Completo
                else
                {
                    filtro = " WHERE IDCodificacionEquipo = '{0}' ";
                    filtro = id.HasValue ? string.Format(filtro, id) : null;

                    listado = db.ListadoCodificacionEquipo(null, null, filtro).ToList(); // Consulta por ID
                }
                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }


        public static CodificacionEquipo ConsultarCodificacionEquipo(int id)
        {
            CodificacionEquipo objeto = new CodificacionEquipo();
            try
            {
                objeto = db.ConsultarCodificacionEquipo(id).FirstOrDefault();
                return objeto;
            }
            catch (Exception)
            {
                return objeto;
            }
        }

        public static InformacionEquipoCodificacionInfo ConsultarInformacionEquipoCodificado(int id, int usuarioID)
        {
            InformacionEquipoCodificacionInfo objeto = new InformacionEquipoCodificacionInfo();
            try
            {
                objeto = db.ConsultarInformacionEquipoCodificacion(id, usuarioID).FirstOrDefault();
                return objeto;
            }
            catch (Exception)
            {
                return objeto;
            }
        }

        public static CodificacionEquipoDetallada ConsultarCodificacionEquipoDetallada(int id)
        {
            try
            {
                var detalles = db.ConsultarDetalleCodificacionEquipo(id).ToList();

                CodificacionEquipoDetallada objeto = new CodificacionEquipoDetallada(ConsultarCodificacionEquipo(id), detalles);

                return objeto;
            }
            catch (Exception)
            {
                return new CodificacionEquipoDetallada();
            }
        }

        public static CodificacionEquipoCompleta ConsultarCodificacionEquipoCompleta(int? id)
        {
            try
            {
                var detalles = db.ListadoDetalleCodificacionEquipo(null, id).ToList();

                CodificacionEquipoCompleta objeto = new CodificacionEquipoCompleta(ListadoCodificacionEquipo(null, null, null, id).FirstOrDefault() ?? new CodificacionEquipoInfo(), detalles);

                return objeto;
            }
            catch (Exception)
            {
                return new CodificacionEquipoCompleta();
            }
        }

        public static List<CodificacionEquipoReporteBasico> ListadoReporteBasico()
        {
            List<CodificacionEquipoReporteBasico> listado = new List<CodificacionEquipoReporteBasico>();
            try
            {
                listado = ListadoCodificacionEquipo().Select(s => new CodificacionEquipoReporteBasico
                {
                    UsuarioCodificador = s.UsuarioCodificador,
                    FechaCodificacionEquipo = s.FechaCodificacionEquipo,
                    FechaSolicitud = s.FechaSolicitud,
                    NombresSolicitante = s.NombresSolicitante,
                    Equipos = s.Equipos,
                    HerramientasAdicionales = s.HerramientasAdicionales,
                    FechaAsignacion = s.FechaAsignacion,
                    CredencialAcceso = s.CredencialAcceso,
                }).ToList();

                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static int ObtenerTotalRegistrosListadoCodificacionEquipo()
        {
            int total = 0;
            try
            {
                total = db.Database.SqlQuery<int>("SELECT [dbo].[ObtenerTotalRegistrosListadoCodificacionEquipo]()").Single();
                return total;
            }
            catch (Exception ex)
            {
                return total;
            }
        }
    }
}