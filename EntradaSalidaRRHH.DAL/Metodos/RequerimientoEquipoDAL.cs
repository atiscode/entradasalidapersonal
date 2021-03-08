using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.DAL.Metodos
{
    public class RequerimientoEquipoDAL
    {
        private static readonly RRHHEntities db = new RRHHEntities();

        public static RespuestaTransaccion CrearRequerimientoEquipo(RequerimientoEquipo objeto, List<int> equipos, List<int> herramientasAdicionales)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {                   

                    objeto.FechaSolicitud = DateTime.Now;

                    db.RequerimientoEquipo.Add(objeto);
                    db.SaveChanges();

                    int IDRequerimientoEquipo = objeto.IDRequerimientoEquipo;

                    foreach (var item in equipos)
                    {
                        db.RequerimientoEquipoUsuario.Add(new RequerimientoEquipoUsuario
                        {
                            EquipoID = item,
                            RequerimientoEquipoID = IDRequerimientoEquipo
                        });
                        db.SaveChanges();
                    }

                    foreach (var item in herramientasAdicionales)
                    {
                        db.RequerimientoEquipoHerramientasAdicionales.Add(new RequerimientoEquipoHerramientasAdicionales
                        {
                            HerramientaAdicional = item,
                            RequerimientoEquipoID = IDRequerimientoEquipo
                        });
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

        public static RespuestaTransaccion ActualizarRequerimientoEquipo(RequerimientoEquipo objeto, List<int> equipos, List<int> herramientasAdicionales)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (!equipos.Any())
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeEquipoRequerido };

                    // assume Entity base class have an Id property for all items
                    var entity = db.RequerimientoEquipo.Find(objeto.IDRequerimientoEquipo);
                    if (entity == null)
                    {
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida };
                    }

                    db.Entry(entity).CurrentValues.SetValues(objeto);
                    db.SaveChanges();

                    int IDRequerimientoEquipo = objeto.IDRequerimientoEquipo;

                    // Limpiar primero los detalles anteriores 
                    var detallesAnterioresEquipos = db.RequerimientoEquipoUsuario.Where(s => s.RequerimientoEquipoID == IDRequerimientoEquipo).ToList();
                    foreach (var item in detallesAnterioresEquipos)
                    {
                        db.RequerimientoEquipoUsuario.Remove(item);
                        db.SaveChanges();
                    }

                    foreach (var item in equipos)
                    {
                        db.RequerimientoEquipoUsuario.Add(new RequerimientoEquipoUsuario
                        {
                            EquipoID = item,
                            RequerimientoEquipoID = IDRequerimientoEquipo
                        });
                        db.SaveChanges();
                    }
                    // Limpiar primero los detalles anteriores 
                    var detallesAnterioresHerramientasAdicionales = db.RequerimientoEquipoHerramientasAdicionales.Where(s => s.RequerimientoEquipoID == IDRequerimientoEquipo).ToList();
                    foreach (var item in detallesAnterioresHerramientasAdicionales)
                    {
                        db.RequerimientoEquipoHerramientasAdicionales.Remove(item);
                        db.SaveChanges();
                    }
                    foreach (var item in herramientasAdicionales)
                    {
                        db.RequerimientoEquipoHerramientasAdicionales.Add(new RequerimientoEquipoHerramientasAdicionales
                        {
                            HerramientaAdicional = item,
                            RequerimientoEquipoID = IDRequerimientoEquipo
                        });
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

        public static RespuestaTransaccion ActualizarRequerimientoEquipoSimple(RequerimientoEquipo objeto)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {

                    // assume Entity base class have an Id property for all items
                    var entity = db.RequerimientoEquipo.Find(objeto.IDRequerimientoEquipo);
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

        public static RespuestaTransaccion AsignarEquiposHerramientasAdicionalesPorRequerimiento(RequerimientoEquipo requerimientoEquipo)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {

                    var entity = db.RequerimientoEquipo.Find(requerimientoEquipo.IDRequerimientoEquipo);
                    if (entity == null)
                    {
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida };
                    }

                    db.AsignarEquiposPorRequerimiento(requerimientoEquipo.IDRequerimientoEquipo);
                    
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

        public static RespuestaTransaccion EliminarRequerimientoEquipo(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var entidad = db.RequerimientoEquipo.Find(id);

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

        public static RespuestaTransaccion AprobarRequerimientoEquipo(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var entidad = db.RequerimientoEquipo.Find(id);

                    entidad.Aprobado = true;
                    entidad.FechaAprobacion = DateTime.Now;

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

        public static List<RequerimientoEquipoInfo> ListadoRequerimientoEquipo(long? pagina = null, string textoBusqueda = null, string filtro = null, int? id = null)
        {
            List<RequerimientoEquipoInfo> listado = new List<RequerimientoEquipoInfo>();
            try
            {
                if (!id.HasValue)
                    listado = db.ListadoRequerimientoEquipo(pagina, textoBusqueda, filtro).ToList(); // Listado Completo
                else
                {
                    filtro = " WHERE IDRequerimientoEquipo = '{0}' ";
                    filtro = id.HasValue ? string.Format(filtro, id) : null;

                    listado = db.ListadoRequerimientoEquipo(null, null, filtro).ToList(); // Consulta por ID
                }
                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static List<EquiposAsignadosUsuariosRequerimientosInfo> ListadoEquiposAsignadosUsuariosRequerimientos(int id)
        {
            List<EquiposAsignadosUsuariosRequerimientosInfo> listado = new List<EquiposAsignadosUsuariosRequerimientosInfo>();
            try
            {
                listado = db.ListadoEquiposAsignadosUsuariosRequerimientos(id).ToList();
                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static List<ListadoEquiposAsignadosUsuario_Result> ListadoEquiposAsignadosPorUsuario(int? idUsuario = null)
        {
            List<ListadoEquiposAsignadosUsuario_Result> listado = new List<ListadoEquiposAsignadosUsuario_Result>();
            try
            {
                listado = db.ListadoEquiposAsignadosUsuario(idUsuario).ToList();
                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static RequerimientoEquipo ConsultarRequerimientoEquipo(int id)
        {
            RequerimientoEquipo objeto = new RequerimientoEquipo();
            try
            {
                objeto = db.ConsultarRequerimientoEquipo(id).FirstOrDefault();
                return objeto;
            }
            catch (Exception)
            {
                return objeto;
            }
        }

        public static RequerimientoEquipoCompleto ConsultarRequerimientoEquipoCompleto(int id)
        {
            try
            {
                var equipousuario = db.ListadoRequerimientoUsuario(id).ToList();
                var herramientasadicionales = db.ListadoRequerimientoUsuarioHerramientasadicionales(id).ToList();

                RequerimientoEquipoCompleto objeto = new RequerimientoEquipoCompleto(ListadoRequerimientoEquipo(null, null, null, id).FirstOrDefault(), equipousuario, herramientasadicionales);

                return objeto;
            }
            catch (Exception)
            {
                return new RequerimientoEquipoCompleto();
            }
        }

        public static List<RequerimientoEquipoReporteBasico> ListadoReporteBasico()
        {
            List<RequerimientoEquipoReporteBasico> listado = new List<RequerimientoEquipoReporteBasico>();
            try
            {
                listado = ListadoRequerimientoEquipo().Select(s => new RequerimientoEquipoReporteBasico
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

        public static int ObtenerTotalRegistrosListadoRequerimientoEquipo()
        {
            int total = 0;
            try
            {
                total = db.Database.SqlQuery<int>("SELECT [dbo].[ObtenerTotalRegistrosListadoRequerimientoEquipo]()").Single();
                return total;
            }
            catch (Exception ex)
            {
                return total;
            }
        }

        public static IEnumerable<SelectListItem> ObtenerEquipoSugeridosByCargo(int cargo)
        {
            List<SelectListItem> listado = new List<SelectListItem>();
            try
            {
                string queryString = "SELECT [dbo].[ObtenerEquipoSugeridosIDsByCargo]({0})";

                queryString = string.Format(queryString, cargo);

                string resultado = db.Database.SqlQuery<string>(queryString).Single();

                var equipos = resultado != null ? resultado.Split(',').ToList() : new List<string>();

                listado = EquipoDAL.ObtenerListadoEquipos().Where(s => equipos.Contains(s.Value) && !string.IsNullOrEmpty(s.Value)).Select(m => new SelectListItem
                {
                    Text = m.Text,
                    Value = m.Value,
                }).ToList();

                return listado;
            }
            catch (Exception ex)
            {
                return listado;
            }
        }

        public static IEnumerable<int> ObtenerIdEquipoSugeridosByCargo(int cargo)
        {
            List<int> listado = new List<int>();
            try
            {
                string queryString = "SELECT [dbo].[ObtenerEquipoSugeridosIDsByCargo]({0})";

                queryString = string.Format(queryString, cargo);

                string resultado = db.Database.SqlQuery<string>(queryString).Single();

                var equipos = resultado != null ? resultado.Split(',').ToList() : new List<string>();

                listado = EquipoDAL.ObtenerListadoEquipos().Where(s => equipos.Contains(s.Value)).Select(v => (int)int.Parse(v.Value)).ToList();
                return listado;
            }
            catch (Exception ex)
            {
                return listado;
            }
        }

        public static IEnumerable<SelectListItem> ObtenerListadoUsuariosValidadosRequerimiento(string seleccionado = null)
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

        public static RespuestaTransaccion ActualizarRequerimientoEquipoUsuario(int idRequerimientoEquipoUsuario, int estado, string observaciones)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    RequerimientoEquipoUsuario entity = db.RequerimientoEquipoUsuario.Find(idRequerimientoEquipoUsuario);
                    entity.FechaModificacion = DateTime.UtcNow;
                    entity.Estado = estado;
                    entity.Observaciones = observaciones;

                    db.Entry(entity).CurrentValues.SetValues(entity);
                    db.Entry(entity).State = EntityState.Modified;
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

        public static RespuestaTransaccion ActualizarRequerimientoEquipoHerramientaAdicional(int idRequerimientoEquipoHerramientasAdicionales, int estado, string observaciones)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var entity = db.RequerimientoEquipoHerramientasAdicionales.Find(idRequerimientoEquipoHerramientasAdicionales);
                    entity.FechaModificacion = DateTime.UtcNow;
                    entity.Estado = estado;
                    entity.Observaciones = observaciones;
                    db.Entry(entity).CurrentValues.SetValues(entity);
                    db.Entry(entity).State = EntityState.Modified;
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
    }
}