using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace EntradaSalidaRRHH.DAL.Metodos
{
    public class SugerenciaEquipoDAL
    {
        private static readonly RRHHEntities db = new RRHHEntities();

        public static RespuestaTransaccion CrearSugerenciaEquiposCargo(SugerenciaEquiposCargo objeto, List<int> programas)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    bool sugerenciaEquipoExistente = db.SugerenciaEquiposCargo.Any(s => s.Cargo == objeto.Cargo && s.EquipoID == objeto.EquipoID && s.Estado);

                    if (sugerenciaEquipoExistente)
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeEquipoSugeridoCargoExistente };

                    db.SugerenciaEquiposCargo.Add(objeto);
                    db.SaveChanges();

                    int IDSugerenciaEquipo = objeto.IDSugerenciaEquiposCargo;

                    foreach (var item in programas)
                    {
                        db.SugerenciaEquiposCargoProgramas.Add(new SugerenciaEquiposCargoProgramas
                        {
                            Programa = item,
                            SugerenciaEquiposCargoID = IDSugerenciaEquipo
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

        public static RespuestaTransaccion ActualizarSugerenciaEquiposCargo(SugerenciaEquiposCargo objeto, List<int> programas)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    bool sugerenciaEquipoExistente = db.SugerenciaEquiposCargo.Any(s => s.IDSugerenciaEquiposCargo != objeto.IDSugerenciaEquiposCargo && s.Cargo == objeto.Cargo && s.EquipoID == objeto.EquipoID && s.Estado);

                    if (sugerenciaEquipoExistente)
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeEquipoSugeridoCargoExistente };

                    // assume Entity base class have an Id property for all items
                    var entity = db.SugerenciaEquiposCargo.Find(objeto.IDSugerenciaEquiposCargo);
                    if (entity == null)
                    {
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida };
                    }

                    db.Entry(entity).CurrentValues.SetValues(objeto);
                    db.SaveChanges();


                    int IDSugerenciaEquipo = objeto.IDSugerenciaEquiposCargo;

                    // Limpiar primero los detalles anteriores 
                    var detallesAnterioresProgramas = db.SugerenciaEquiposCargoProgramas.Where(s => s.SugerenciaEquiposCargoID == IDSugerenciaEquipo).ToList();
                    foreach (var item in detallesAnterioresProgramas)
                    {
                        db.SugerenciaEquiposCargoProgramas.Remove(item);
                        db.SaveChanges();
                    }

                    foreach (var item in programas)
                    {
                        db.SugerenciaEquiposCargoProgramas.Add(new SugerenciaEquiposCargoProgramas
                        {
                            Programa = item,
                            SugerenciaEquiposCargoID = IDSugerenciaEquipo
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

        public static RespuestaTransaccion EliminarSugerenciaEquiposCargo(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var entidad = db.SugerenciaEquiposCargo.Find(id);

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

        public static List<SugerenciaEquiposCargoInfo> ListadoSugerenciaEquiposCargo(long? pagina = null, string textoBusqueda = null, string filtro = null, int? id = null)
        {
            List<SugerenciaEquiposCargoInfo> listado = new List<SugerenciaEquiposCargoInfo>();
            try
            {
                if (!id.HasValue)
                    listado = db.ListadoSugerenciaEquiposCargo(pagina, textoBusqueda, filtro).ToList(); // Listado Completo
                else
                {
                    filtro = " WHERE IDSugerenciaEquiposCargo = '{0}' ";
                    filtro = id.HasValue ? string.Format(filtro, id) : null;
                    listado = db.ListadoSugerenciaEquiposCargo(null, null, filtro).ToList(); // Consulta por ID
                }
                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static SugerenciaEquiposCargo ConsultarSugerenciaEquiposCargo(int id)
        {
            SugerenciaEquiposCargo objeto = new SugerenciaEquiposCargo();
            try
            {
                objeto = db.ConsultarSugerenciaEquiposCargo(id).FirstOrDefault();
                return objeto;
            }
            catch (Exception)
            {
                return objeto;
            }
        }

        public static List<SugerenciaEquiposCargoInfo> ListadosEquiposSugeridosByCargo(int id)
        {
            List<SugerenciaEquiposCargoInfo> listado = new List<SugerenciaEquiposCargoInfo>();
            try
            {
                listado = ListadoSugerenciaEquiposCargo().Where(s=> s.CargoSugerenciaEquipos == id).ToList();
                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static List<SugerenciaEquipoReporteBasico> ListadoReporteBasico()
        {
            List<SugerenciaEquipoReporteBasico> listado = new List<SugerenciaEquipoReporteBasico>();
            try
            {
                listado = ListadoSugerenciaEquiposCargo().Select(s => new SugerenciaEquipoReporteBasico
                {
                    Caracteristicas = s.Caracteristicas,
                    NombreEquipo = s.NombreEquipo,
                    TextoCatalogoCargoSugerenciaEquipo = s.TextoCatalogoCargoSugerenciaEquipo,
                    TextoCatalogoTipo = s.TextoCatalogoTipo
                }).ToList();

                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static int ObtenerTotalRegistrosListadoSugerenciaEquipo()
        {
            int total = 0;
            try
            {
                total = db.Database.SqlQuery<int>("SELECT [dbo].[ObtenerTotalRegistrosListadoSugerenciaEquiposCargo]()").Single();
                return total;
            }
            catch (Exception ex)
            {
                return total;
            }
        }
    }
}