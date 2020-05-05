using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.DAL.Metodos
{
    public class FichaIngresoDAL
    {
        private static readonly RRHHEntities db = new RRHHEntities();

        public static RespuestaTransaccion CrearFichaIngreso(FichaIngreso objeto, List<DetalleCargasFamiliares> cargasFamiliares, List<DetalleEstudios> estudios, List<DetalleExperiencias> experiencias)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    //Validación de fecha de nacimiento
                    if(objeto.FechaNacimiento > DateTime.Now )
                        return new RespuestaTransaccion { Estado = false, Respuesta = string.Format(Mensajes.MensajeValidacionRangoFecha, "nacimiento","actual") };
                    
                    //Validación de fecha de nacimiento de cargas familiares
                    foreach (var item in cargasFamiliares)
                    {
                        if (item.FechaNacimiento > DateTime.Now)
                            return new RespuestaTransaccion { Estado = false, Respuesta = string.Format(Mensajes.MensajeValidacionRangoFecha, "nacimiento de " + item.Nombres + " ,", "actual") };
                    }

                    //Validación de rango de fechas en experiencias
                    foreach (var item in experiencias)
                    {
                        if (item.FechaInicio > item.FechaFin)
                            return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeValidacionRangoFechasInicioFin };
                    }

                    objeto.FechaIngresoFicha = DateTime.Now;

                    db.FichaIngreso.Add(objeto);
                    db.SaveChanges();

                    int fechaIngresoID = objeto.IDFichaIngreso;

                    foreach (var item in cargasFamiliares)
                    {
                        item.FichaIngresoID = fechaIngresoID;
                        db.DetalleCargasFamiliares.Add(item);
                        db.SaveChanges();
                    }

                    foreach (var item in experiencias)
                    {
                        item.FichaIngresoID = fechaIngresoID;
                        db.DetalleExperiencias.Add(item);
                        db.SaveChanges();
                    }

                    foreach (var item in estudios)
                    {
                        item.FichaIngresoID = fechaIngresoID;
                        db.DetalleEstudios.Add(item);
                        db.SaveChanges();
                    }

                    //Solucion error atach
                    db.Entry(objeto).State = EntityState.Detached;

                    transaction.Commit();

                    return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
                }
                catch (Exception ex)
                {
                    //Solucion error atach
                    db.Entry(objeto).State = EntityState.Detached;

                    transaction.Rollback();
                    return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
                }
            }
        }

        public static RespuestaTransaccion ActualizarFichaIngreso(FichaIngreso objeto, List<DetalleCargasFamiliares> cargasFamiliares, List<DetalleEstudios> estudios, List<DetalleExperiencias> experiencias)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // assume Entity base class have an Id property for all items
                    var entity = db.FichaIngreso.Find(objeto.IDFichaIngreso);
                    if (entity == null)
                    {
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida };
                    }

                    if (objeto.Foto == null)
                        objeto.Foto = entity.Foto;

                    db.Entry(entity).CurrentValues.SetValues(objeto);
                    db.SaveChanges();

                    int fechaIngresoID = objeto.IDFichaIngreso;

                    #region Actualizar Detalles
                    // Limpiar primero los detalles anteriores 
                    var detallesAnterioresCargasFamiliares = db.DetalleCargasFamiliares.Where(s => s.FichaIngresoID == fechaIngresoID).ToList();
                    foreach (var item in detallesAnterioresCargasFamiliares)
                    {
                        db.DetalleCargasFamiliares.Remove(item);
                        db.SaveChanges();
                    }
                    //Registrar nuevos detalles
                    foreach (var item in cargasFamiliares)
                    {
                        item.FichaIngresoID = fechaIngresoID;
                        db.DetalleCargasFamiliares.Add(item);
                        db.SaveChanges();
                    }

                    // Limpiar primero los detalles anteriores 
                    var detallesAnterioresExperiencias = db.DetalleExperiencias.Where(s => s.FichaIngresoID == fechaIngresoID).ToList();
                    foreach (var item in detallesAnterioresExperiencias)
                    {
                        db.DetalleExperiencias.Remove(item);
                        db.SaveChanges();
                    }
                    foreach (var item in experiencias)
                    {
                        item.FichaIngresoID = fechaIngresoID;
                        db.DetalleExperiencias.Add(item);
                        db.SaveChanges();
                    }

                    // Limpiar primero los detalles anteriores 
                    var detallesAnterioresEstudios = db.DetalleEstudios.Where(s => s.FichaIngresoID == fechaIngresoID).ToList();
                    foreach (var item in detallesAnterioresEstudios)
                    {
                        db.DetalleEstudios.Remove(item);
                        db.SaveChanges();
                    }
                    foreach (var item in estudios)
                    {
                        item.FichaIngresoID = fechaIngresoID;
                        db.DetalleEstudios.Add(item);
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

        public static RespuestaTransaccion EliminarFichaIngreso(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var entidad = db.FichaIngreso.Find(id);

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

        public static IEnumerable<SelectListItem> ObtenerListadoFichasIngreso(string seleccionado = null)
        {
            List<SelectListItem> ListadoCatalogo = new List<SelectListItem> { new SelectListItem { Text = Etiquetas.TituloComboVacio, Value = string.Empty } };
            try
            {
                //Solo listar los usuarios que ya tengan fichas de ingreso
                ListadoCatalogo.AddRange(db.ListadoIngresoUsuarioDesvinculacion().Where(s => s.TieneFichaIngreso == 1).Select(c => new SelectListItem
                {
                    Text = c.NombresApellidos.ToString() + " - " + c.Identificacion,
                    Value = c.IDFichaIngreso.ToString()
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

        public static List<FichasIngresosInfo> ListadoFichaIngreso(long? pagina = null, string textoBusqueda = null, string filtro = null, int? id = null)
        {
            List<FichasIngresosInfo> listado = new List<FichasIngresosInfo>();
            try
            {
                if (!id.HasValue)
                    listado = db.ListadoFichasIngresos(pagina, textoBusqueda, filtro).ToList(); // Listado Completo
                else
                {
                    filtro = " WHERE IDFichaIngreso = '{0}' ";
                    filtro = id.HasValue ? string.Format(filtro, id) : null;

                    listado = db.ListadoFichasIngresos(null, null, filtro).ToList(); // Consulta por ID
                }
                return listado;

            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static FichaIngreso ConsultarFichaIngreso(int id)
        {
            FichaIngreso objeto = new FichaIngreso();
            try
            {
                objeto = db.ConsultarFichaIngreso(id).FirstOrDefault();
                return objeto;
            }
            catch (Exception)
            {
                return objeto;
            }
        }

        //Verificar si el usuario ya cuenta con una ficha de ingreso creada previamente.
        public static bool FichaIngresoExistente(int usuarioID)
        {
            bool objeto = false;
            try
            {
                objeto = db.ConsultarFichaIngreso(null).Any(s => s.UsuarioID == usuarioID && s.Estado);
                return objeto;
            }
            catch (Exception ex)
            {
                return objeto;
            }
        }

        public static IEnumerable<SelectListItem> ObtenerListadoFichasIngresoUsuarios(string seleccionado = null)
        {
            List<SelectListItem> ListadoCatalogo = new List<SelectListItem> { new SelectListItem { Text = Etiquetas.TituloComboVacio, Value = string.Empty } };
            try
            {
                ListadoCatalogo.AddRange(db.ListadoFichasIngresos(null, null,null).Where(s=> s.Activo ?? false).Select(c => new SelectListItem
                {
                    Text = c.NombresApellidosUsuario.ToString() + " - " + c.identificacion,
                    Value = c.UsuarioID.ToString()
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

        //Un usuario solo puede tener una ficha de ingreso
        public static bool ValidarFichaIngresoUsuarioExistente(int usuarioID)
        {
            bool existe = false;
            try
            {
                existe = db.ConsultarFichaIngreso(null).Any(s => s.UsuarioID == usuarioID);
                return existe;
            }
            catch (Exception)
            {
                return existe;
            }
        }

        public static FichaIngresoDetallada ConsultarFichaIngresoDetallada(int id)
        {
            try
            {
                var cargasFamiliares = db.ConsultarDetalleCargasFamiliares(null, id).ToList();
                var estudios = db.ConsultarDetalleEstudios(null, id).ToList();
                var experiencia = db.ConsultarDetalleExperiencias(null, id).ToList();

                FichaIngresoDetallada objeto = new FichaIngresoDetallada(ConsultarFichaIngreso(id), cargasFamiliares, estudios, experiencia);

                return objeto;
            }
            catch (Exception)
            {
                return new FichaIngresoDetallada();
            }
        }

        public static FichaIngresoCompleta ConsultarFichaIngresoCompleta(int id)
        {
            try
            {
                var cargasFamiliares = db.ListadoDetalleCargasFamiliares(null, id).ToList();
                var estudios = db.ListadoDetalleEstudios(null, id).ToList();
                var experiencia = db.ListadoDetalleExperiencias(null, id).ToList();
                var ingreso = db.ConsultarIngresoPorFicha(id).FirstOrDefault();

                FichaIngresoCompleta objeto = new FichaIngresoCompleta(ListadoFichaIngreso(null, null, null, id).FirstOrDefault(), cargasFamiliares, estudios, experiencia, ingreso);

                return objeto;
            }
            catch (Exception)
            {
                return new FichaIngresoCompleta();
            }
        }

        public static List<FichaIngresoReporteBasico> ListadoReporteBasico()
        {
            List<FichaIngresoReporteBasico> listado = new List<FichaIngresoReporteBasico>();
            try
            {
                listado = ListadoFichaIngreso().Select(s => new FichaIngresoReporteBasico
                {
                    NombresApellidosUsuario = s.NombresApellidosUsuario,
                    identificacion = s.identificacion,
                    NombreEmpresa = s.NombreEmpresa,
                    FechaIngreso = s.FechaIngreso
                }).ToList();

                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static int ObtenerTotalRegistrosListadoFichaIngreso()
        {
            int total = 0;
            try
            {
                total = db.Database.SqlQuery<int>("SELECT [dbo].[ObtenerTotalRegistrosListadoFichaIngreso]()").Single();
                return total;
            }
            catch (Exception ex)
            {
                return total;
            }
        }

        public static IEnumerable<SelectListItem> ObtenerListadoUsuariosFichasIngreso()
        {
            List<SelectListItem> ListadoCatalogo = new List<SelectListItem> { new SelectListItem { Text = Etiquetas.TituloComboVacio, Value = string.Empty } };
            try
            {
                //Solo listar los usuarios que no tengan fichas de ingreso
                ListadoCatalogo.AddRange(db.ListadoIngresoUsuarioDesvinculacion().Where(s=> s.TieneFichaIngreso != 1).Select(c => new SelectListItem
                {
                    Text = c.NombresApellidos.ToString() + " - " + c.Identificacion,
                    Value = c.IdUsuario.ToString()
                }).ToList());

                return ListadoCatalogo;
            }
            catch (Exception ex)
            {
                return ListadoCatalogo;
            }
        }

    }
}