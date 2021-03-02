using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
//Extensión para Query string dinámico
using System.Linq.Dynamic;
using EntradaSalidaRRHH.Repositorios;
using EntradaSalidaRRHH.UI.Helper;

namespace EntradaSalidaRRHH.UI.Controllers
{
    public class ModificacionEquipoController : BaseController
    {
        // GET: ModificacionEquipo
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<PartialViewResult> _IndexGrid(string search, string sort = "", string order = "", long? page = 1)
        {
            //Permisos
            Permisos(ControllerContext.RouteData.Values["controller"].ToString());

            var listado = new List<IngresosInfo>();
            ViewBag.NombreListado = Etiquetas.TituloGridIngreso;
            page = page > 0 ? page - 1 : page;
            int totalPaginas = 1;
            try
            {
                var query = (HttpContext.Request.Params.Get("QUERY_STRING") ?? "").ToString();

                var dynamicQueryString = GetQueryString(query);
                var whereClause = BuildWhereDynamicClause(dynamicQueryString);

                //Siempre y cuando no haya filtros definidos en el Grid
                if (string.IsNullOrEmpty(whereClause))
                {
                    if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
                        listado = IngresoDAL.ListadoIngreso(page.Value).OrderBy(sort + " " + order).ToList();
                    else
                        listado = IngresoDAL.ListadoIngreso(page.Value).ToList();
                }

                search = !string.IsNullOrEmpty(search) ? search.Trim() : "";

                if (!string.IsNullOrEmpty(search))//filter
                {
                    listado = IngresoDAL.ListadoIngreso(null, search);
                }

                if (!string.IsNullOrEmpty(whereClause) && string.IsNullOrEmpty(search))
                {
                    if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
                        listado = IngresoDAL.ListadoIngreso(null, null, whereClause).OrderBy(sort + " " + order).ToList();
                    else
                        listado = IngresoDAL.ListadoIngreso(null, null, whereClause);
                }
                else
                {

                    if (string.IsNullOrEmpty(search))
                        totalPaginas = IngresoDAL.ObtenerTotalRegistrosListadoIngreso();
                }

                ViewBag.TotalPaginas = totalPaginas;

                // Only grid query values will be available here.
                return PartialView(await Task.Run(() => listado));
            }
            catch (Exception ex)
            {
                ViewBag.TotalPaginas = totalPaginas;
                // Only grid query values will be available here.
                return PartialView(await Task.Run(() => listado));
            }
        }
    }
}