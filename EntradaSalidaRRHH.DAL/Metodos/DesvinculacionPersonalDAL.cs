using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.DAL.Metodos
{
    public class DesvinculacionPersonalDAL
    {
        private static readonly RRHHEntities db = new RRHHEntities();

        public static RespuestaTransaccion CrearDesvinculacionPersonal(DesvinculacionPersonal objeto, List<DetalleEquiposEntregados> detalles)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    db.DesvinculacionPersonal.Add(objeto);
                    db.SaveChanges();

                    int DesvinculacionPersonalID = objeto.IDDesvinculacionPersonal;

                    foreach (var item in detalles)
                    {
                        item.DesvinculacionPersonalID = DesvinculacionPersonalID;
                        db.DetalleEquiposEntregados.Add(item);
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

        public static RespuestaTransaccion ActualizarDesvinculacionPersonal(DesvinculacionPersonal objeto, List<DetalleEquiposEntregados> detalles)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // assume Entity base class have an Id property for all items
                    var entity = db.DesvinculacionPersonal.Find(objeto.IDDesvinculacionPersonal);
                    if (entity == null)
                    {
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida };
                    }

                    db.Entry(entity).CurrentValues.SetValues(objeto);
                    db.SaveChanges();

                    int DesvinculacionPersonalID = objeto.IDDesvinculacionPersonal;

                    #region Actualizar Detalles
                    // Limpiar primero los detalles anteriores 
                    var detallesAnteriores = db.DetalleEquiposEntregados.Where(s => s.DesvinculacionPersonalID == DesvinculacionPersonalID).ToList();
                    foreach (var item in detallesAnteriores)
                    {
                        db.DetalleEquiposEntregados.Remove(item);
                        db.SaveChanges();
                    }
                    //Registrar nuevos detalles
                    foreach (var item in detalles)
                    {
                        item.DesvinculacionPersonalID = DesvinculacionPersonalID;
                        db.DetalleEquiposEntregados.Add(item);
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

        public static RespuestaTransaccion EliminarDesvinculacionPersonal(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var entidad = db.DesvinculacionPersonal.Find(id);

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

        public static List<DesvinculacionPersonalInfo> ListadoDesvinculacionPersonal(long? pagina = null, string textoBusqueda = null, string filtro = null, int? id = null)
        {
            List<DesvinculacionPersonalInfo> listado = new List<DesvinculacionPersonalInfo>();
            try
            {
                if (!id.HasValue)
                    listado = db.ListadoDesvinculacionPersonal(pagina, textoBusqueda, filtro).ToList(); // Listado Completo
                else
                {
                    filtro = " WHERE IDDesvinculacionPersonal = '{0}' ";
                    filtro = id.HasValue ? string.Format(filtro, id) : null;

                    listado = db.ListadoDesvinculacionPersonal(null, null, filtro).ToList(); // Consulta por ID
                }
                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }


        public static DesvinculacionPersonal ConsultarDesvinculacionPersonal(int id)
        {
            DesvinculacionPersonal objeto = new DesvinculacionPersonal();
            try
            {
                objeto = db.ConsultarDesvinculacionPersonal(id).FirstOrDefault();
                return objeto;
            }
            catch (Exception)
            {
                return objeto;
            }
        }

        public static DesvinculacionPersonalDetallada ConsultarDesvinculacionPersonalDetallada(int id)
        {
            try
            {
                var detalles = db.ConsultarDetalleEquiposEntregados(id).ToList();

                DesvinculacionPersonalDetallada objeto = new DesvinculacionPersonalDetallada(ConsultarDesvinculacionPersonal(id), detalles);

                return objeto;
            }
            catch (Exception)
            {
                return new DesvinculacionPersonalDetallada();
            }
        }

        public static DesvinculacionPersonalCompleta ConsultarDesvinculacionPersonalCompleta(int? id)
        {
            try
            {
                var detalles = db.ListadoDetalleEquiposEntregados(null, id).ToList();

                DesvinculacionPersonalCompleta objeto = new DesvinculacionPersonalCompleta(ListadoDesvinculacionPersonal(null, null, null, id).FirstOrDefault() ?? new DesvinculacionPersonalInfo(), detalles);

                return objeto;
            }
            catch (Exception)
            {
                return new DesvinculacionPersonalCompleta();
            }
        }

        public static List<DesvinculacionPersonalReporteBasico> ListadoReporteBasico()
        {
            List<DesvinculacionPersonalReporteBasico> listado = new List<DesvinculacionPersonalReporteBasico>();
            try
            {
                listado = ListadoDesvinculacionPersonal().Select(s => new DesvinculacionPersonalReporteBasico
                {
                    FechaSalida = s.FechaSalida,
                    TextoMotivo = s.TextoMotivo,
                    UsuarioDesvinculado = s.UsuarioDesvinculado,
                    UsuarioResponsableDesvinculacion = s.UsuarioResponsableDesvinculacion,
                    TextoEstadoSalidaSeguroMedico = s.TextoEstadoSalidaSeguroMedico,
                    TextoEstadoEncuestaSalida = s.TextoEstadoEncuestaSalida,
                }).ToList();

                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static int ObtenerTotalRegistrosListadoDesvinculacionPersonal()
        {
            int total = 0;
            try
            {
                total = db.Database.SqlQuery<int>("SELECT [dbo].[ObtenerTotalRegistrosListadoDesvinculacionPersonal]()").Single();
                return total;
            }
            catch (Exception ex)
            {
                return total;
            }
        }

        public static IEnumerable<SelectListItem> ObtenerListadoUsuariosValidadosDesvinculacion(string seleccionado = null)
        {
            List<SelectListItem> ListadoCatalogo = new List<SelectListItem> { new SelectListItem { Text = Etiquetas.TituloComboVacio, Value = string.Empty } };
            try
            {
                //Solo listar los usuarios que ya tengan fichas de ingreso
                ListadoCatalogo.AddRange(db.ListadoIngresoUsuarioDesvinculacion().Where(s => s.TieneFichaIngreso == 1 && s.TieneIngreso == 1 && s.PersonaDesvinculada == 0).Select(c => new SelectListItem
                {
                    Text = c.NombresApellidos.ToString() + " - " + c.Identificacion,
                    Value = c.IdUsuario.ToString()
                }).ToList());

                if (!string.IsNullOrEmpty(seleccionado))
                {
                    if (ListadoCatalogo.FirstOrDefault(s => s.Value == seleccionado.ToString()) != null)
                        ListadoCatalogo.FirstOrDefault(s => s.Value == seleccionado.ToString()).Selected = true;
                }

                return ListadoCatalogo;
            }
            catch (Exception ex)
            {
                return ListadoCatalogo;
            }
        }
    }
}