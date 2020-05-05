using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using EntradaSalidaRRHH.UI.Helper;

namespace EntradaSalidaRRHH.UI.Controllers
{
    // GET: Menu
    [Autenticado]
    public class MenuController : BaseController
    {

        private AdministracionEntities db = new AdministracionEntities();

        private List<string> columnasReportesBasicos = new List<string> { "MENÚ PADRE", "OPCIÓN MENÚ", "RUTA ACCESO", "ESTADO" };
        // GET: Menu
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<PartialViewResult> _IndexGrid(String search)
        {
            //Permisos
            Permisos(ControllerContext.RouteData.Values["controller"].ToString());

            ViewBag.NombreListado = Etiquetas.TituloGridMenu;
            var listado = MenuDAL.ListarMenu();

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

        // GET: Menu/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Menu menu = db.Menu.Find(id);
            if (menu == null)
            {
                return HttpNotFound();
            }
            return View(menu);
        }

        public ActionResult Create()
        {
            var listado = MenuDAL.ListarMenu().Select(s => new Menu { IdMenu = s.IdMenu, NombreMenu = s.OpcionMenu + " ( " + s.RutaAcceso + " )" }).AsEnumerable();
            ViewBag.listadoMenu = new SelectList(listado, "IdMenu", "NombreMenu");

            return View();
        }

        [HttpPost]
        public ActionResult Create(Menu menu)
        {
            try
            {
                var listado = MenuDAL.ListarMenu().Select(s => new Menu { IdMenu = s.IdMenu, NombreMenu = s.OpcionMenu + " ( " + s.RutaAcceso + " )" }).AsEnumerable();
                ViewBag.listadoMenu = new SelectList(listado, "IdMenu", "NombreMenu");

                string nombreMenu = (menu.NombreMenu ?? string.Empty).ToLower().Trim();

                var validacionNombreRolUnico = MenuDAL.ListarMenu().Where(s => (s.OpcionMenu ?? string.Empty).ToLower().Trim() == nombreMenu).ToList();

                if (validacionNombreRolUnico.Count > 0)
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeResgistroExistente } }, JsonRequestBehavior.AllowGet);

                RespuestaTransaccion resultado = MenuDAL.CrearMenu(menu);

                return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var menu = MenuDAL.ConsultarMenu(id.Value);

            var listado = MenuDAL.ListarMenu().Select(s => new Menu { IdMenu = s.IdMenu, NombreMenu = s.OpcionMenu + " ( " + s.RutaAcceso + " )" }).AsEnumerable();
            ViewBag.listadoMenu = new SelectList(listado, "IdMenu", "NombreMenu", id.Value);

            if (menu == null)
            {
                return HttpNotFound();
            }
            return View(menu);
        }

        [HttpPost]
        public ActionResult Edit(Menu menu)
        {
            try
            {
                var listado = MenuDAL.ListarMenu().Select(s => new Menu { IdMenu = s.IdMenu, NombreMenu = s.OpcionMenu + " ( " + s.RutaAcceso + " )" }).AsEnumerable();
                ViewBag.listadoMenu = new SelectList(listado, "IdMenu", "NombreMenu");

                string nombreMenu = (menu.NombreMenu ?? string.Empty).ToLower().Trim();

                var validacionNombreRolUnico = MenuDAL.ListarMenu().Where(s => (s.OpcionMenu ?? string.Empty).ToLower().Trim() == nombreMenu && s.IdMenu != menu.IdMenu).ToList();

                if (validacionNombreRolUnico.Count > 0)
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeResgistroExistente } }, JsonRequestBehavior.AllowGet);

                RespuestaTransaccion resultado = MenuDAL.ActualizarMenu(menu);

                return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RespuestaTransaccion resultado = MenuDAL.EliminarMenu(id);// await db.Cabecera.FindAsync(id);

            //return RedirectToAction("Index");
            return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);

        }

        //GET: Menu/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Menu menu = db.Menu.Find(id);
            if (menu == null)
            {
                return HttpNotFound();
            }
            return View(menu);
        }

        // POST: Menu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Menu menu = db.Menu.Find(id);
            db.Menu.Remove(menu);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        

        public ActionResult OrdenMenu()
        {
            var item = db.ListadoOrdenMenu().ToList();
            var item2 = item.OrderBy(x => x.OrdenMenu);

            return View(item2);
        }

        public ActionResult ActualizarOrdenPadre(string itemIds)
        {
            try
            {
                Resultado = MenuDAL.ActualizarOrdenMenu(itemIds);

                return Json(new { Resultado = Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }


        }

        public ActionResult ActualizarOrdenHijo(string itemIds)
        {
            try
            {
                Resultado = MenuDAL.ActualizarOrdenMenu(itemIds);

                return Json(new { Resultado = Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        #region REPORTES BASICOS
        public ActionResult DescargarReporteFormatoExcel()
        {
            var collection = MenuDAL.ListadoReporteBasico();
            var package = GetEXCEL(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(package.GetAsByteArray(), XlsxContentType, "ListadoMenu.xlsx");
        }

        public ActionResult DescargarReporteFormatoPDF()
        {
            var collection = MenuDAL.ListadoReporteBasico();
            byte[] buffer = GetPDF(columnasReportesBasicos, collection.Cast<object>().ToList());

            return File(buffer, PDFContentType, "ListadoMenu.pdf");
        }

        public ActionResult DescargarReporteFormatoCSV()
        {
            var collection = MenuDAL.ListadoReporteBasico();
            byte[] buffer = GetCSV(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(buffer, CSVContentType, $"ListadoMenu.csv");
        }
        #endregion


    }
}