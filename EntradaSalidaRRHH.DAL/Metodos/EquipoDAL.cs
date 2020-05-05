using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.DAL.Metodos
{
    public class EquipoDAL
    {
        private static readonly RRHHEntities db = new RRHHEntities();

        public static RespuestaTransaccion CrearEquipo(Equipo objeto, List<int> caracteristicas)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    db.Equipo.Add(objeto);
                    db.SaveChanges();

                    int IDEquipo = objeto.IDEquipo;

                    foreach (var item in caracteristicas)
                    {
                        db.EquipoCaracteristica.Add(new EquipoCaracteristica
                        {
                            Caracteristica = item,
                            EquipoID = IDEquipo
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

        public static RespuestaTransaccion ActualizarEquipo(Equipo objeto, List<int> caracteristicas)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // assume Entity base class have an Id property for all items
                    var entity = db.Equipo.Find(objeto.IDEquipo);
                    if (entity == null)
                    {
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida };
                    }

                    db.Entry(entity).CurrentValues.SetValues(objeto);
                    db.SaveChanges();

                    int IDEquipo = objeto.IDEquipo;

                    // Limpiar primero los detalles anteriores 
                    var detallesAnterioresCaracteristicas = db.EquipoCaracteristica.Where(s => s.EquipoID == IDEquipo).ToList();
                    foreach (var item in detallesAnterioresCaracteristicas)
                    {
                        db.EquipoCaracteristica.Remove(item);
                        db.SaveChanges();
                    }

                    foreach (var item in caracteristicas)
                    {
                        db.EquipoCaracteristica.Add(new EquipoCaracteristica
                        {
                            Caracteristica = item,
                            EquipoID = IDEquipo
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

        public static RespuestaTransaccion EliminarEquipo(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var entidad = db.Equipo.Find(id);

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

        public static List<EquiposInfo> ListadoEquipo(long? pagina = null, string textoBusqueda = null, string filtro = null, int? id = null)
        {
            List<EquiposInfo> listado = new List<EquiposInfo>();
            try
            {
                if (!id.HasValue)
                    listado = db.ListadoEquipos(pagina, textoBusqueda, filtro).ToList(); // Listado Completo
                else
                {
                    filtro = " WHERE IDEquipo = '{0}' ";

                    filtro = id.HasValue ? string.Format(filtro, id) : null;
                    listado = db.ListadoEquipos(null, null, filtro).ToList(); // Consulta por ID
                }

                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static Equipo ConsultarEquipo(int id)
        {
            Equipo objeto = new Equipo();
            try
            {
                objeto = db.ConsultarEquipo(id).FirstOrDefault();
                return objeto;
            }
            catch (Exception)
            {
                return objeto;
            }
        }

        public static List<EquipoReporteBasico> ListadoReporteBasico()
        {
            List<EquipoReporteBasico> listado = new List<EquipoReporteBasico>();
            try
            {
                listado = ListadoEquipo().Select(s => new EquipoReporteBasico
                {
                    NombreEquipo = s.NombreEquipo,
                    DescripcionEquipo = s.DescripcionEquipo,
                    TextoCatalogoTipo = s.TextoCatalogoTipo,
                    Caracteristicas = s.Caracteristicas,
                    Observaciones = s.Observaciones,
                    Costo = s.Costo,
                    TextoCatalogoProveedor = s.TextoCatalogoProveedor,
                }).ToList();

                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static int ObtenerTotalRegistrosListadoEquipo()
        {
            int total = 0;
            try
            {
                total = db.Database.SqlQuery<int>("SELECT [dbo].[ObtenerTotalRegistrosListadoEquipo]()").Single();
                return total;
            }
            catch (Exception ex)
            {
                return total;
            }
        }

        public static IEnumerable<SelectListItem> ObtenerListadoEquipos(string seleccionado = null, string filtro = " WHERE CodigoCatalogoTipo in ('CELULAR-01', 'COMPUTADOR-01', 'ENSERES-01') ")
        {
            List<SelectListItem> ListadoCatalogo = new List<SelectListItem> { new SelectListItem { Text = Etiquetas.TituloComboVacio, Value = string.Empty } };
            try
            {
                ListadoCatalogo.AddRange(db.ListadoEquipos(null, null, filtro).Select(c => new SelectListItem
                {
                    Text = filtro.Contains("COMPUTADOR-01") ? c.NombreEquipo + "( " + c.TextoCatalogoTipo + " )" : c.NombreEquipo,
                    Value = c.IDEquipo.ToString()
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

        public static IEnumerable<SelectListItem> ObtenerListadoEquiposCompletos(string seleccionado = null)
        {
            List<SelectListItem> ListadoCatalogo = new List<SelectListItem> { new SelectListItem { Text = Etiquetas.TituloComboVacio, Value = string.Empty } };
            try
            {
                ListadoCatalogo.AddRange(db.ListadoEquipos(null, null, null).Select(c => new SelectListItem
                {
                    Text =  c.NombreEquipo,
                    Value = c.IDEquipo.ToString()
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

        public static IEnumerable<SelectListItem> ObtenerListadoEquiposAccesoriosHerramientasAdicionales(string seleccionado = null)
        {
            List<SelectListItem> ListadoCatalogo = new List<SelectListItem> { new SelectListItem { Text = Etiquetas.TituloComboVacio, Value = string.Empty } };
            try
            {
                ListadoCatalogo.AddRange(db.ListadoEquipos(null, null, " WHERE CodigoCatalogoTipo = 'ACCESORIOS-01' ").Select(c => new SelectListItem
                {
                    Text = c.NombreEquipo.ToString() + "( " + c.TextoCatalogoTipo + " )",
                    Value = c.IDEquipo.ToString()
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