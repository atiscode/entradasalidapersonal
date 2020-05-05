using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using EntradaSalidaRRHH.UI.Helper;
using NLog;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.UI.Controllers
{

    public class CatalogoController : BaseController
    {
        private List<string> columnasReportesBasicos = new List<string> { "NOMBRE", "DESCRIPCIÓN", "CÓDIGO CATALOGO", "ESTADO" };

        [Autenticado]
        // GET: Catalogo
        public ActionResult Index()
        {
            var respuesta = System.Web.HttpContext.Current.Session["Resultado"] as string;
            var estado = System.Web.HttpContext.Current.Session["Estado"] as string;

            ViewBag.Resultado = respuesta;
            ViewBag.Estado = estado;

            Session["Resultado"] = "";
            Session["Estado"] = "";
            return View();
        }

        [Autenticado]
        [HttpGet]
        public async Task<PartialViewResult> _IndexGrid(String search)
        {
            ViewBag.NombreListado = Etiquetas.TituloGridCatalogo;

            //Controlar permisos
            var usuario = UsuarioLogeadoSession.IdUsuario;
            string nombreControlador = ControllerContext.RouteData.Values["controller"].ToString();
            ViewBag.NombreControlador = nombreControlador;


            ViewBag.AccionesUsuario = ManejoPermisosDAL.ListadoAccionesCatalogoUsuario(usuario, nombreControlador);

            //Obtener Acciones del controlador
            ViewBag.AccionesControlador = GetMetodosControlador(nombreControlador);

            //Búsqueda

            var listado = CatalogoDAL.ListarCatalogos();

            search = !string.IsNullOrEmpty(search) ? search.Trim() : "";

            if (!string.IsNullOrEmpty(search))//filter
            {
                var type = listado.GetType().GetGenericArguments()[0];
                var properties = type.GetProperties();

                listado = listado.Where(x => properties
                            .Any(p =>
                            {
                                var value = p.GetValue(x);
                                return value != null && value.ToString().ToLower().Contains(search.ToLower());
                            })).ToList();
            }

            // Only grid query values will be available here.
            return PartialView(await Task.Run(() => listado));
        }

        [Autenticado]
        // GET: Catalogo/Create
        public ActionResult Create()
        {
            return View();
        }

        [Autenticado]
        // POST: Catalogo/Create
        [HttpPost]
        public ActionResult Create(Catalogo catalogo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    RespuestaTransaccion resultado = CatalogoDAL.CrearCatalogo(catalogo);

                    //Almacenar en una variable de sesion
                    Session["Resultado"] = resultado.Respuesta;
                    Session["Estado"] = resultado.Estado.ToString();

                    if (resultado.Estado.ToString() == "True")
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.Resultado = resultado.Respuesta;
                        ViewBag.Estado = resultado.Estado.ToString();
                        Session["Resultado"] = "";
                        Session["Estado"] = "";
                        return View(catalogo);
                    }
                }
                return View(catalogo);
            }
            catch
            {
                return View(catalogo);
            }
        }


        [HttpPost]
        public ActionResult CrearSubCatalogo(Catalogo formulario)
        {
            formulario.DescripcionCatalogo = formulario.NombreCatalogo;
            formulario.CodigoCatalogo = "CREADO-N-" + Guid.NewGuid().ToString().Substring(0, 15).ToUpper();

            Resultado = CatalogoDAL.CrearCatalogo(formulario);
            return Json(new { Resultado }, JsonRequestBehavior.AllowGet);
        }

        [Autenticado]
        // GET: Catalogo/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                var catalogo = CatalogoDAL.ConsultarCatalogo(id.Value);

                if (catalogo == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    return View(catalogo);
                }
            }
        }

        [Autenticado]
        // POST: Catalogo/Edit/5
        [HttpPost]
        public ActionResult Edit(Catalogo catalogo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    RespuestaTransaccion resultado = CatalogoDAL.ActualizarCatalogo(catalogo);

                    //Almacenar en una variable de sesion
                    Session["Resultado"] = resultado.Respuesta;
                    Session["Estado"] = resultado.Estado.ToString();

                    if (resultado.Estado.ToString() == "True")
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.Resultado = resultado.Respuesta;
                        ViewBag.Estado = resultado.Estado.ToString();
                        Session["Resultado"] = "";
                        Session["Estado"] = "";
                        return View(catalogo);
                    }
                }

                return View(catalogo);
            }
            catch
            {
                return View();
            }
        }

        [Autenticado]
        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            RespuestaTransaccion resultado = CatalogoDAL.EliminarCatalogo(id);

            //Almacenar en una variable de sesion
            Session["Resultado"] = resultado.Respuesta;
            Session["Estado"] = resultado.Estado.ToString();

            //return RedirectToAction("Index");
            return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _AgregarSubCatalogo(string codigoCatalogo)
        {
            string titulo = "Agregar nuevo Catálogo a {0}";
            try
            {
                var catalogo = CatalogoDAL.ConsultarCatalogo(codigoCatalogo);
                ViewBag.TituloModal = string.Format(titulo, catalogo.NombreCatalogo);
                return PartialView(catalogo);
            }
            catch (Exception ex)
            {
                ViewBag.TituloModal = string.Format(titulo, ex.Message);
                return PartialView(new CatalogoInfo());
            }
        }

        [Autenticado]
        // GET: Subcatalogo/
        public ActionResult IndexSubcatalogo(int id)
        {
            var respuesta = System.Web.HttpContext.Current.Session["Resultado"] as string;
            var estado = System.Web.HttpContext.Current.Session["Estado"] as string;

            ViewBag.Resultado = respuesta;
            ViewBag.Estado = estado;

            Session["Resultado"] = "";
            Session["Estado"] = "";

            System.Web.HttpContext.Current.Session["IdCatalogo"] = id.ToString();

            //Obtener Listado de Hijos del Catalogo 
            var numeroHijos = CatalogoDAL.ObtenerNumeroHijosCatalgo(id);
            ViewBag.numeroHijos = numeroHijos;

            //Obtener listado de hijos del id seleccionado
            var ListadoHijosPadre = CatalogoDAL.ListadoHijosoCatalogoPorIdPadre(id);
            ViewBag.ListadoHijosPadre = ListadoHijosPadre;

            //Obtener listado de hijos 
            var ListadoCatalogoPadre = CatalogoDAL.ListadoCatalogosPorIdSinOrdenar(id);
            ViewBag.ListadoCatalogoPadre = ListadoCatalogoPadre;

            //ViewBag.EtapaGeneral = new Catalogo();
            //ViewBag.EstatusDetallado = new Catalogo();
            //ViewBag.EstatusGeneral = new Catalogo();

            ViewBag.IdCatalogo = id;
            var codigoCatalogo = CatalogoDAL.ConsultarCodigocatalogo(id);
            ViewBag.CodigoCatalago = codigoCatalogo;

            return View();
        }

        [Autenticado]
        [HttpGet]
        public async Task<PartialViewResult> IndexGridSubcatalogo(String search, int? tipo, int? subcatalogo, string filtro)
        {

            //Controlar permisos
            var usuario = UsuarioLogeadoSession.IdUsuario;
            string nombreControlador = ControllerContext.RouteData.Values["controller"].ToString();
            ViewBag.NombreControlador = nombreControlador;
            ViewBag.AccionesUsuario = ManejoPermisosDAL.ListadoAccionesCatalogoUsuario(usuario, nombreControlador);
            ViewBag.AccionesControlador = GetMetodosControlador(nombreControlador);//Obtener Acciones del controlador

            //Búsqueda

            //Cuando no selecciona tipo
            if (tipo == null && subcatalogo == null)
            {
                //Obtener le id del catalogo
                var id_catalogo = ViewData["IdCatalogo"] = System.Web.HttpContext.Current.Session["IdCatalogo"] as String;

                var nombre = CatalogoDAL.ConsultarNombreCatalogo(Convert.ToInt32(id_catalogo));

                ViewBag.NombreListado = "Catálogo -" + nombre;
                var listado = CatalogoDAL.ListarCatalogosPorId(Convert.ToInt32(id_catalogo), filtro);

                search = !string.IsNullOrEmpty(search) ? search.Trim() : "";

                if (!string.IsNullOrEmpty(search))//filter
                {
                    var type = listado.GetType().GetGenericArguments()[0];
                    var properties = type.GetProperties();

                    listado = listado.Where(x => properties
                                .Any(p =>
                                {
                                    var value = p.GetValue(x);
                                    return value != null && value.ToString().ToLower().Contains(search.ToLower());
                                })).ToList();
                }

                // Only grid query values will be available here.
                return PartialView("_IndexGridSubcatalogo", await Task.Run(() => listado));
            }
            else
            {
                //Cuando no selecciona tipo
                if (tipo != null && subcatalogo != null)
                {
                    //Obtener le id del catalogo
                    var id_catalogo = ViewData["IdCatalogo"] = System.Web.HttpContext.Current.Session["IdCatalogo"] as String;

                    var nombre = CatalogoDAL.ConsultarNombreCatalogo(Convert.ToInt32(subcatalogo));

                    ViewBag.NombreListado = "Catálogo -" + nombre;
                    var listado = CatalogoDAL.ListarCatalogosPorId(subcatalogo.Value, filtro);

                    search = !string.IsNullOrEmpty(search) ? search.Trim() : "";

                    if (!string.IsNullOrEmpty(search))//filter
                    {
                        var type = listado.GetType().GetGenericArguments()[0];
                        var properties = type.GetProperties();

                        listado = listado.Where(x => properties
                                    .Any(p =>
                                    {
                                        var value = p.GetValue(x);
                                        return value != null && value.ToString().ToLower().Contains(search.ToLower());
                                    })).ToList();
                    }

                    // Only grid query values will be available here.
                    return PartialView("_IndexGridSubcatalogo", await Task.Run(() => listado));
                }
                else
                {
                    //Obtener le id del catalogo
                    var id_catalogo = ViewData["IdCatalogo"] = System.Web.HttpContext.Current.Session["IdCatalogo"] as String;

                    var nombre = CatalogoDAL.ConsultarNombreCatalogo(Convert.ToInt32(tipo));

                    ViewBag.NombreListado = "Catálogo -" + nombre;
                    var listado = CatalogoDAL.ListarCatalogosPorId(Convert.ToInt32(tipo), filtro);

                    search = !string.IsNullOrEmpty(search) ? search.Trim() : "";

                    if (!string.IsNullOrEmpty(search))//filter
                    {
                        var type = listado.GetType().GetGenericArguments()[0];
                        var properties = type.GetProperties();

                        listado = listado.Where(x => properties
                                    .Any(p =>
                                    {
                                        var value = p.GetValue(x);
                                        return value != null && value.ToString().ToLower().Contains(search.ToLower());
                                    })).ToList();
                    }

                    // Only grid query values will be available here.
                    return PartialView("_IndexGridSubcatalogo", await Task.Run(() => listado));
                }
            }
        }

        [Autenticado]
        //Accion para crear subcatalogo
        [HttpPost]
        public ActionResult IndexSubcatalogo(Catalogo catalogo, string general, string detallado, string statusGeneral)
        {
            //Variable para las respuestas
            RespuestaTransaccion resultado = new RespuestaTransaccion();
            int idCatalogo = 0;

            //Validar campos llenos
            if ((catalogo.NombreCatalogo == null))
            {
                resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeDatosObligatorios };
            }
            else
            {
                //Recuperado de la vista
                idCatalogo = catalogo.IdCatalogo;

                //validar si el tipo es diferente catalogo
                if (catalogo.IdCatalogo == 0)
                {
                    idCatalogo = CatalogoDAL.ObtenerIdPadre(catalogo.IdCatalogoPadre.Value);
                }


                //Crear la subcategoria
                resultado = CatalogoDAL.CrearSubcatalogo(catalogo);

            }

            //Almacenar en una variable de sesion
            Session["Resultado"] = resultado.Respuesta;
            Session["Estado"] = resultado.Estado.ToString();

            //Obtener Listado de Hijos del Catalogo 
            var numeroHijos = CatalogoDAL.ObtenerNumeroHijosCatalgo(Convert.ToInt32(idCatalogo));
            ViewBag.numeroHijos = numeroHijos;

            //Obtener listado de hijos del id seleccionado
            var ListadoHijosPadre = CatalogoDAL.ListadoHijosoCatalogoPorIdPadre(Convert.ToInt32(idCatalogo));
            ViewBag.ListadoHijosPadre = ListadoHijosPadre;

            //Obtener listado de hijos 
            var ListadoCatalogoPadre = CatalogoDAL.ListadoCatalogosPorIdSinOrdenar(Convert.ToInt32(idCatalogo));
            ViewBag.ListadoCatalogoPadre = ListadoCatalogoPadre;

            ViewBag.EtapaGeneral = new Catalogo();
            ViewBag.EstatusDetallado = new Catalogo();
            ViewBag.EstatusGeneral = new Catalogo();

            ViewBag.IdCatalogo = idCatalogo;

            ////Obtener Ruta PDF
            //string path = string.Empty;
            //string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
            //path = "../AdjuntosManual/" + controllerName + ".pdf";

            //var absolutePath = HttpContext.Server.MapPath(path);
            //bool rutaArchivo = System.IO.File.Exists(absolutePath);

            //if (!rutaArchivo)
            //{
            //    string path1 = "../AdjuntosManual/ManualUsuario.pdf";
            //    ViewBag.Iframe = path1;
            //}
            //else
            //{
            //    ViewBag.Iframe = path;
            //}

            if (resultado.Estado.ToString() == "True")
            {
                ViewBag.Resultado = resultado.Respuesta;
                ViewBag.Estado = resultado.Estado.ToString();

                return RedirectToAction("IndexSubcatalogo/" + idCatalogo, "Catalogo");
            }
            else
            {
                ViewBag.Resultado = resultado.Respuesta;
                ViewBag.Estado = resultado.Estado.ToString();

                Session["Resultado"] = "";
                Session["Estado"] = "";

                return View(catalogo);
            }
        }

        [Autenticado]
        [HttpPost]
        public ActionResult CreateSubCatalogo(Catalogo catalogo)
        {
            RespuestaTransaccion resultado = new RespuestaTransaccion();
            try
            {
                resultado = CatalogoDAL.CrearSubcatalogo(catalogo);

                return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        [Autenticado]
        // GET: Subcatalogo/Create
        public ActionResult _EditSubcatalogo(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                //Titulo de la Pantalla
                ViewBag.TituloModal = Etiquetas.TituloPanelCreacionCatalogo;

                var catalogo = CatalogoDAL.ConsultarCatalogo(id);

                if (catalogo == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    return PartialView(catalogo);
                }
            }

        }


        [Autenticado]
        [HttpPost]
        public ActionResult _EditSubcatalogo(Catalogo catalogo)
        {
            //Titulo de la Pantalla
            ViewBag.TituloModal = Etiquetas.TituloPanelCreacionCatalogo;

            if (ModelState.IsValid)
            {
                RespuestaTransaccion resultado = CatalogoDAL.ActualizarSubCatalogo(catalogo);

                //Almacenar en una variable de sesion
                Session["Resultado"] = resultado.Respuesta;
                Session["Estado"] = resultado.Estado.ToString();

                if (resultado.Estado.ToString() == "True")
                {
                    var id_catalogo = ViewData["IdCatalogo"] = System.Web.HttpContext.Current.Session["IdCatalogo"] as String;
                    return RedirectToAction("IndexSubcatalogo/" + id_catalogo, "Catalogo");
                }
                else
                {
                    ViewBag.Resultado = resultado.Respuesta;
                    ViewBag.Estado = resultado.Estado.ToString();
                    Session["Resultado"] = "";
                    Session["Estado"] = "";
                    return PartialView(catalogo);
                }
            }
            else
            {
                return View(catalogo);
            }
        }


        [Autenticado]
        [ActionName("BorrarSubcatalogo")]
        public ActionResult BorrarSubcatalogo(int? id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            RespuestaTransaccion resultado = CatalogoDAL.EliminarSubcatalogo(id.Value);

            //Almacenar en una variable de sesion
            Session["Resultado"] = resultado.Respuesta;
            Session["Estado"] = resultado.Estado.ToString();

            //return RedirectToAction("Index");
            return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);

        }

        [Autenticado]
        public JsonResult GetSubcatalogos(int id, string tipo)
        {
            var datos = CatalogoDAL.ListarCatalogosPorId(Convert.ToInt32(id), "");
            //datos = datos.Where(d => d.codigo_catalogo == tipo).ToList();
            ViewBag.ListadoCatalogos = datos;
            return Json(datos, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCatalogos(string query, string codigo)
        {
            query = !string.IsNullOrEmpty(query) ? query.ToLower().Trim() : string.Empty;

            var data = CatalogoDAL.ObtenerListadoCatalogosByCodigoSeleccion(codigo)
            //if "query" is null, get all records
            .Where(m => string.IsNullOrEmpty(query) || m.Text.ToLower().Contains(query))
            .OrderBy(m => m.Text);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #region REPORTES BASICOS
        [Autenticado]
        public ActionResult DescargarReporteFormatoExcel()
        {
            var collection = CatalogoDAL.ListadoReporteBasico();
            var package = GetEXCEL(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(package.GetAsByteArray(), XlsxContentType, "ListadoCatalogos.xlsx");
        }

        [Autenticado]
        public ActionResult DescargarReporteFormatoPDF()
        {
            // Seleccionar las columnas a exportar
            var collection = CatalogoDAL.ListadoReporteBasico();
            byte[] buffer = GetPDF(columnasReportesBasicos, collection.Cast<object>().ToList());

            return File(buffer, PDFContentType, "ListadoCatalogos.pdf");
        }

        [Autenticado]
        public ActionResult DescargarReporteFormatoCSV()
        {
            var collection = CatalogoDAL.ListadoReporteBasico();
            byte[] buffer = GetCSV(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(buffer, CSVContentType, $"ListadoCatalogos.csv");
        }
        #endregion

    }
}