using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.DAL.Metodos
{
    public class CatalogoDAL
    {
        private static readonly AdministracionEntities db = new AdministracionEntities();

        public static RespuestaTransaccion CrearCatalogo(Catalogo catalogo)
        {
            try
            {
                //Validar que no exista otro catalogo con ese nombre
                var ValidarCatalogosDuplicados = db.ConsultarCatalogo(null).Where(c => c.NombreCatalogo == catalogo.NombreCatalogo).ToList();
                if (ValidarCatalogosDuplicados.Count() > 0)
                {
                    return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeResgistroExistente };
                }
                else
                {
                    //validar datos obligatorios
                    if (catalogo.NombreCatalogo is null || catalogo.DescripcionCatalogo is null || catalogo.CodigoCatalogo is null)
                    {
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeDatosObligatorios };
                    }
                    else
                    {
                        catalogo.NombreCatalogo = catalogo.NombreCatalogo.ToUpper();
                        catalogo.DescripcionCatalogo = catalogo.DescripcionCatalogo.ToUpper();
                        catalogo.Eliminado = false;
                        catalogo.EstadoCatalogo = true;
                        catalogo.IdEmpresa = 1;
                        db.Catalogo.Add(catalogo);
                        db.SaveChanges();

                        return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
                    }
                }
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }


        public static RespuestaTransaccion ActualizarCatalogo(Catalogo catalogo)
        {

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // assume Entity base class have an Id property for all items
                    var entity = db.Catalogo.Find(catalogo.IdCatalogo);
                    if (entity == null)
                    {
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida };
                    }
                    if (catalogo.NombreCatalogo is null || catalogo.DescripcionCatalogo is null || catalogo.CodigoCatalogo is null)
                    {
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeDatosObligatorios };
                    }
                    else
                    {
                        //valiar que no se repita el nombre
                        var ValidarCatalogosDuplicados = db.ConsultarCatalogo(null).Where(c => c.NombreCatalogo == catalogo.NombreCatalogo && c.IdCatalogo != catalogo.IdCatalogo).ToList();
                        if (ValidarCatalogosDuplicados.Count() > 0)
                        {
                            return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeDatosObligatorios };
                        }
                        else
                        {
                            catalogo.NombreCatalogo = catalogo.NombreCatalogo.ToUpper();
                            catalogo.DescripcionCatalogo = catalogo.NombreCatalogo.ToUpper();
                            db.Entry(entity).CurrentValues.SetValues(catalogo);
                            db.SaveChanges();

                            transaction.Commit();
                            return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
                        }

                    }


                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
                }
            }

        }

        public static RespuestaTransaccion EliminarCatalogo(int id)
        {
            try
            {
                var catalogo = db.Catalogo.Find(id);

                if (catalogo.EstadoCatalogo == true)
                {
                    catalogo.EstadoCatalogo = false;
                }
                else
                {
                    catalogo.EstadoCatalogo = true;
                }

                db.Entry(catalogo).State = EntityState.Modified;
                db.SaveChanges();

                return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }

        public static RespuestaTransaccion CrearSubcatalogo(Catalogo catalogo)
        {
            try
            {
                //validar datos obligatorios
                if (catalogo.NombreCatalogo is null)
                {
                    return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeDatosObligatorios };
                }
                else
                {
                    //Nuevo objeto de tipo catalogo
                    Catalogo subcatalogo = new Catalogo();

                    //Es un subcatalogo del subcatalogo
                    if (catalogo.DescripcionCatalogo != null && catalogo.IdCatalogo == 0)
                    {
                        //valiar que no se repita el nombre
                        var ValidarCatalogosDuplicados = db.ConsultarCatalogo(null).Where(c => c.NombreCatalogo == catalogo.NombreCatalogo && c.CodigoCatalogo == catalogo.DescripcionCatalogo && c.IdCatalogo == catalogo.IdCatalogoPadre).ToList();
                        if (ValidarCatalogosDuplicados.Count() > 0)
                        {
                            return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeResgistroExistente };
                        }
                        else
                        {

                            subcatalogo.NombreCatalogo = catalogo.NombreCatalogo.ToUpper();
                            subcatalogo.DescripcionCatalogo = catalogo.NombreCatalogo.ToUpper();
                            subcatalogo.CodigoCatalogo = catalogo.DescripcionCatalogo;
                            subcatalogo.IdCatalogoPadre = catalogo.IdCatalogoPadre;
                            subcatalogo.Eliminado = false;
                            subcatalogo.IdEmpresa = 1;
                            subcatalogo.EstadoCatalogo = true;

                            db.Catalogo.Add(subcatalogo);
                            db.SaveChanges();

                            return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
                        }
                    }
                    else
                    {
                        //valiar que no se repita el nombre
                        var ValidarCatalogosDuplicados = db.ConsultarCatalogo(null).Where(c => c.NombreCatalogo == catalogo.NombreCatalogo && c.IdCatalogoPadre == catalogo.IdCatalogo).ToList();
                        if (ValidarCatalogosDuplicados.Count() > 0)
                        {
                            return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeResgistroExistente };
                        }
                        else
                        {
                            var consultarCatalogo = ConsultarCatalogo(catalogo.IdCatalogo);
                            var verificarcatalogo = consultarCatalogo.CodigoCatalogo;
                            if (verificarcatalogo == "ACCIONES-SIST-01")
                            {
                                subcatalogo.NombreCatalogo = catalogo.NombreCatalogo.ToUpper();
                                subcatalogo.DescripcionCatalogo = catalogo.DescripcionCatalogo;
                                subcatalogo.CodigoCatalogo = catalogo.CodigoCatalogo.ToUpper();
                                subcatalogo.IdCatalogoPadre = catalogo.IdCatalogo;
                                subcatalogo.Eliminado = false;
                                subcatalogo.IdEmpresa = 1;
                                subcatalogo.EstadoCatalogo = true;

                                db.Catalogo.Add(subcatalogo);
                                db.SaveChanges();
                            }
                            else
                            {

                                subcatalogo.NombreCatalogo = catalogo.NombreCatalogo.ToUpper();
                                subcatalogo.DescripcionCatalogo = catalogo.NombreCatalogo.ToUpper();
                                subcatalogo.IdCatalogoPadre = catalogo.IdCatalogo;
                                subcatalogo.Eliminado = false;
                                subcatalogo.IdEmpresa = 1;
                                subcatalogo.EstadoCatalogo = true;

                                db.Catalogo.Add(subcatalogo);
                                db.SaveChanges();
                            }


                            return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }

        //public static RespuestaTransaccion ActualizarSubCatalogo(Catalogo catalogo)
        //{
        //    try
        //    {
        //        // Por si queda el Attach de la entidad y no deja actualizar
        //        var local = db.ConsultarCatalogo(null).FirstOrDefault(c => c.IdCatalogo == catalogo.IdCatalogo);
        //        if (local != null)
        //        {
        //            db.Entry(local).State = EntityState.Detached;
        //        }

        //        //validar que no este vacio
        //        if (catalogo.NombreCatalogo == null)
        //        {
        //            return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeDatosObligatorios };
        //        }
        //        else
        //        {
        //            //valiar que no se repita el nombre
        //            var ValidarCatalogosDuplicados = db.ConsultarCatalogo(null).Where(c => c.NombreCatalogo == catalogo.NombreCatalogo && c.IdCatalogo != catalogo.IdCatalogo && c.IdCatalogoPadre == catalogo.IdCatalogoPadre).ToList();
        //            if (ValidarCatalogosDuplicados.Count() > 0)
        //            {
        //                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeDatosObligatorios };
        //            }
        //            else
        //            {
        //                catalogo.NombreCatalogo = catalogo.NombreCatalogo.ToUpper();
        //                catalogo.DescripcionCatalogo = catalogo.NombreCatalogo.ToUpper();
        //                db.SaveChanges();
        //                db.Entry(catalogo).State = EntityState.Modified;

        //                return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
        //    }
        //}

        public static RespuestaTransaccion ActualizarSubCatalogo(Catalogo catalogo)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // assume Entity base class have an Id property for all items
                    var entity = db.Catalogo.Find(catalogo.IdCatalogo);
                    if (entity == null)
                    {
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida };
                    }
                    if (catalogo.NombreCatalogo == null)
                    {
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeDatosObligatorios };
                    }
                    else
                    {
                        //valiar que no se repita el nombre
                        var ValidarCatalogosDuplicados = db.ConsultarCatalogo(null).Where(c => c.NombreCatalogo == catalogo.NombreCatalogo && c.IdCatalogo != catalogo.IdCatalogo && c.IdCatalogoPadre == catalogo.IdCatalogoPadre).ToList();
                        if (ValidarCatalogosDuplicados.Count() > 0)
                        {
                            return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeDatosObligatorios };
                        }
                        else
                        {
                            catalogo.NombreCatalogo = catalogo.NombreCatalogo.ToUpper();
                            catalogo.DescripcionCatalogo = catalogo.NombreCatalogo.ToUpper();
                            db.Entry(entity).CurrentValues.SetValues(catalogo);
                            db.SaveChanges();

                            transaction.Commit();
                            return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
                        }

                    }


                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
                }
            }
        }

        public static RespuestaTransaccion EliminarSubcatalogo(int id)
        {
            try
            {
                var catalogo = db.Catalogo.First(c => c.IdCatalogo == id);

                db.Catalogo.Remove(catalogo);
                db.SaveChanges();

                return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }


        public static List<CatalogoInfo> ListarCatalogos()
        {
            try
            {
                return db.ListadoCatalogosPadres().ToList();
            }
            catch (Exception ex)
            {
                return new List<CatalogoInfo>();
            }
        }


        public static List<CatalogoInfo> ListarSubcatalogos()
        {
            try
            {
                return db.ListadoSubcatalogos().ToList();
            }
            catch (Exception)
            {
                return new List<CatalogoInfo>();
            }
        }

        public static List<CatalogoInfo> ListadoCatalogos()
        {
            try
            {
                return db.ListadoCatalogo().ToList();
            }
            catch (Exception ex)
            {
                return new List<CatalogoInfo>();
            }
        }

        public static Catalogo ConsultarCatalogo(int id)
        {
            try
            {
                Catalogo catalogo = db.ConsultarCatalogo(id).FirstOrDefault();
                return catalogo;
            }
            catch (Exception ex)
            {
                return new Catalogo();
            }
        }

        public static CatalogoInfo ConsultarCatalogo(string codigo)
        {
            try
            {
                CatalogoInfo catalogo = db.ListaCatalogos().Where(s=> s.CodigoCatalogo == codigo).FirstOrDefault() ?? new CatalogoInfo {  NombreCatalogo = "CATÁLOGO NO ENCONTRADO"};
                return catalogo;
            }
            catch (Exception ex)
            {
                return new CatalogoInfo();
            }
        }

        public static string ConsultarNombreCatalogo(int id)
        {
            string nombreCatalogo = string.Empty;
            try
            {
                nombreCatalogo = (ConsultarCatalogo(id) ?? new Catalogo()).NombreCatalogo;
                return nombreCatalogo;
            }
            catch (Exception ex)
            {
                return nombreCatalogo;
            }
        }

        public static int BuscarCatalogo(int id)
        {
            try
            {
                int IDCatalogo = (ConsultarCatalogo(id) ?? new Catalogo()).IdCatalogo;
                return IDCatalogo;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static bool VerificarExistenciaCatalogo(int id)
        {
            try
            {
                bool existe = ConsultarCatalogo(id) != null ? true : false;
                return existe;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static IEnumerable<SelectListItem> ListadoHijosoCatalogoPorIdPadre(int id_catalogo)
        {
            var ListadoCatalogo = db.ConsultarCatalogosPorPadre(id_catalogo).Select(c => new SelectListItem
            {
                Text = c.NombreCatalogo,
                Value = c.IdCatalogo.ToString()
            }).ToList();

            return ListadoCatalogo;
        }


        public static IEnumerable<SelectListItem> ListadoCatalogosPorCodigo(string codigo)
        {
            var ListadoCatalogo = db.ConsultarCatalogoPorCodigo(codigo).OrderBy(c => c.NombreCatalogo).Select(c => new SelectListItem
            {
                Text = c.NombreCatalogo.ToUpper(),
                Value = c.IdCatalogo.ToString()
            }).ToList();

            return ListadoCatalogo;
        }

        public static IEnumerable<SelectListItem> ListadoCatalogosPorId(int id)
        {
            var ListadoCatalogo = db.ConsultarCatalogo(id).OrderBy(c => c.NombreCatalogo).Select(c => new SelectListItem
            {
                Text = c.NombreCatalogo,
                Value = c.IdCatalogo.ToString()
            }).ToList();

            return ListadoCatalogo;
        }

        public static IEnumerable<SelectListItem> ListadoCatalogosPorIdSinOrdenar(int id)
        {
            List<SelectListItem> listado = new List<SelectListItem>();
            try
            {
                listado = db.ConsultarCatalogosPorId(id, "").Select(c => new SelectListItem
                {
                    Text = c.Nombre,
                    Value = c.IdCatalogo.ToString()
                }).ToList();

                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        //Para catálogos dependientes que tienen varios niveles
        public static IEnumerable<SelectListItem> ListadoCatalogosPorCodigoId(string codigo, int id, string seleccionado = null)
        {
            List<SelectListItem> listado = new List<SelectListItem> { new SelectListItem { Text = Etiquetas.TituloComboVacio, Value = string.Empty } };
            try
            {
                listado.AddRange(db.ConsultarCatalogosCodigoIdPadre(codigo, id).OrderBy(c => c.NombreCatalogo).Select(c => new SelectListItem
                {
                    Text = c.NombreCatalogo,
                    Value = c.IdCatalogo.ToString()
                }).ToList());

                if (!string.IsNullOrEmpty(seleccionado))
                {
                    if (listado.FirstOrDefault(s => s.Value == seleccionado.ToString()) != null)
                        listado.FirstOrDefault(s => s.Value == seleccionado.ToString()).Selected = true;
                }
                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static int ObtenerNumeroHijosCatalgo(int id)
        {
            try
            {
                var numero_hijos = db.ConsultarNumeroHijosCatalogo(id).First();
                int numero = Convert.ToInt32(numero_hijos.NumeroHIjos);

                return numero;


            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static int ObtenerIdPadre(int id)
        {
            try
            {
                var numero_hijos = (db.ConsultarCatalogo(id).FirstOrDefault(c => c.IdCatalogo == id) ?? new Catalogo()).IdCatalogoPadre;
                int idPadre = Convert.ToInt32(numero_hijos);

                return idPadre;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static IEnumerable<SelectListItem> ObtenerListadoCatalogosCodigoPadre(string codigopadre)
        {
            List<SelectListItem> listado = new List<SelectListItem>();
            try
            {
                var catalogo = db.Catalogo.Where(c => c.CodigoCatalogo == codigopadre).FirstOrDefault();

                listado = db.Catalogo.Where(c => c.IdCatalogoPadre == catalogo.IdCatalogo).OrderBy(c => c.NombreCatalogo).Select(c => new SelectListItem
                {
                    Text = c.NombreCatalogo,
                    Value = c.IdCatalogo.ToString()
                }).ToList();

                return listado;
            }
            catch (Exception)
            {
                return listado;
                throw;
            }

        }

        public static List<CatalogosPorIdInfo> ListarCatalogosPorId(int id, string filtro)
        {
            try
            {
                var listado = db.ConsultarCatalogosPorId(id, filtro).ToList();
                return listado;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static IEnumerable<SelectListItem> ObtenerListadoCatalogos(int id_padre)
        {
            List<SelectListItem> listado = new List<SelectListItem>();
            try
            {
                listado = db.ConsultarCatalogo(null).Where(c => c.IdCatalogoPadre == id_padre).OrderBy(c => c.NombreCatalogo).Select(c => new SelectListItem
                {
                    Text = c.NombreCatalogo,
                    Value = c.IdCatalogo.ToString()
                }).ToList();

                return listado;
            }
            catch (Exception ex)
            {
                return listado;
            }
        }

        public static IEnumerable<SelectListItem> ObtenerListadoCatalogos(string codigocatalogo)
        {
            List<SelectListItem> listado = new List<SelectListItem>();
            try
            {
                var catalogo = db.Catalogo.Where(c => c.CodigoCatalogo == codigocatalogo).FirstOrDefault();

                listado = db.Catalogo.Where(c => c.IdCatalogoPadre == catalogo.IdCatalogo).OrderBy(c => c.NombreCatalogo).Select(c => new SelectListItem
                {
                    Text = c.NombreCatalogo + "|" + c.DescripcionCatalogo,
                    Value = c.IdCatalogo.ToString()
                }).ToList();

                return listado;
            }
            catch (Exception)
            {
                return listado;
                throw;
            }

        }

        public static IEnumerable<SelectListItem> ObtenerListadoCatalogosByCodigoSeleccion(string codigo, string seleccionado = null)
        {
            List<SelectListItem> ListadoCatalogo = new List<SelectListItem>();
            try
            {
                var padreCatalogo = db.ConsultarCatalogo(null).FirstOrDefault(s => s.CodigoCatalogo == codigo);

                if (padreCatalogo == null)
                {
                    return new List<SelectListItem>();
                }

                ListadoCatalogo = db.ConsultarCatalogo(null).Where(c => c.IdCatalogoPadre == padreCatalogo.IdCatalogo).OrderBy(c => c.NombreCatalogo).Select(c => new SelectListItem
                {
                    Text = c.NombreCatalogo,
                    Value = c.IdCatalogo.ToString(),
                }).ToList();

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



        public static IEnumerable<SelectListItem> ObtenerListadoCatalogosByCodigo(string codigo, string seleccionado = null)
        {
            List<SelectListItem> ListadoCatalogo = new List<SelectListItem>();
            try
            {
                var padreCatalogo = db.Catalogo.FirstOrDefault(s => s.CodigoCatalogo == codigo);

                if (padreCatalogo == null)
                {
                    return new List<SelectListItem>();
                }

                ListadoCatalogo = db.Catalogo.Where(c => c.IdCatalogoPadre == padreCatalogo.IdCatalogo).OrderBy(c => c.NombreCatalogo).Select(c => new SelectListItem
                {
                    Text = c.NombreCatalogo,
                    Value = c.IdCatalogo.ToString(),
                }).ToList();

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

        public static IEnumerable<SelectListItem> ObtenerCatalogoAños (string seleccionado = null)
        {
            List<SelectListItem> ListadoCatalogo = new List<SelectListItem>();
            try
            {
                var añoActual = DateTime.Now.Year;
                for (var año = añoActual; año >= 1970; año--)
                {
                    ListadoCatalogo.Add(new SelectListItem { Text = año.ToString(), Value = año.ToString() }); ;
                }

                if(!string.IsNullOrEmpty(seleccionado))
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



        public static IEnumerable<SelectListItem> ConsultarCatalogoPorPadre(int id, string codigo)
        {
            List<SelectListItem> ListadoCatalogo = new List<SelectListItem>();
            try
            {

                ListadoCatalogo = db.ConsultarCatalogo(null).Where(s => s.IdCatalogoPadre == id && s.CodigoCatalogo == codigo).OrderBy(c => c.NombreCatalogo).Select(c => new SelectListItem
                {
                    Text = c.NombreCatalogo,
                    Value = c.IdCatalogo.ToString()
                }).ToList();

                return ListadoCatalogo;

            }
            catch (Exception)
            {
                return ListadoCatalogo;
            }
        }

        //Para dependientes
        public static IEnumerable<SelectListItem> ConsultarCatalogoPorPadreByCodigo(string codigo, int id, string seleccionado = null)
        {
            SelectListItem vacio = new SelectListItem { Selected = true, Text = Etiquetas.TituloComboVacio, Value = "" };
            List<SelectListItem> ListadoCatalogo = new List<SelectListItem> { vacio };
            try
            {

                var padreCatalogo = db.Catalogo.FirstOrDefault(s => s.CodigoCatalogo == codigo);

                if (padreCatalogo == null)
                {
                    return ListadoCatalogo;
                }

                if (id != 0)
                    ListadoCatalogo = db.ConsultarCatalogo(null).Where(s => s.IdCatalogoPadre == id && s.CodigoCatalogo == codigo).OrderBy(c => c.NombreCatalogo).Select(c => new SelectListItem
                    {
                        Text = c.NombreCatalogo,
                        Value = c.IdCatalogo.ToString()
                    }).ToList();
                else
                    return ListadoCatalogo;

                if (!string.IsNullOrEmpty(seleccionado))
                {
                    if (ListadoCatalogo.FirstOrDefault(s => s.Value == seleccionado.ToString()) != null)
                        ListadoCatalogo.FirstOrDefault(s => s.Value == seleccionado.ToString()).Selected = true;
                }

                return ListadoCatalogo;

            }
            catch (Exception)
            {
                return ListadoCatalogo;
            }
        }

        public static IEnumerable<SelectListItem> ObtenerListadoCatalogosByCodigoSinOrdenar(string codigo, string seleccionado = null)
        {
            List<SelectListItem> ListadoCatalogo = new List<SelectListItem>();
            try
            {
                var padreCatalogo = db.ConsultarCatalogo(null).FirstOrDefault(s => s.CodigoCatalogo == codigo);

                if (padreCatalogo == null)
                {
                    return new List<SelectListItem>();
                }

                ListadoCatalogo = db.ConsultarCatalogo(null).Where(c => c.IdCatalogoPadre == padreCatalogo.IdCatalogo).Select(c => new SelectListItem
                {
                    Text = c.NombreCatalogo,
                    Value = c.IdCatalogo.ToString(),
                }).ToList();

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

        public static bool ValidarExistenciaPrefijo(string numeroPrefijo)
        {
            bool flag = true;
            try
            {
                var padreCatalogo = db.ConsultarCatalogo(null).FirstOrDefault(s => s.CodigoCatalogo == "PREFIJO" && s.NombreCatalogo == numeroPrefijo);
                if (padreCatalogo == null)
                    flag = false;

                return flag;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static string ConsultarCodigocatalogo(int id)
        {
            try
            {
                Catalogo catalogo = (ConsultarCatalogo(id) ?? new Catalogo());

                return catalogo.CodigoCatalogo;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static List<CatalogosReporteBasico> ListadoReporteBasico()
        {
            List<CatalogosReporteBasico> listado = new List<CatalogosReporteBasico>();
            try
            {
                listado = db.ListaCatalogos().Select(p => new CatalogosReporteBasico
                {
                    Nombre = p.NombreCatalogo,
                    Descripcion = p.DescripcionCatalogo,
                    Codigo = p.CodigoCatalogo,
                    Estado = p.TextoEstadoCatalogo

                }).ToList();

                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

    }
}