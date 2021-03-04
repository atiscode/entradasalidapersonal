using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Linq.Dynamic;
using EntradaSalidaRRHH.Repositorios;
using System.Net;
using EntradaSalidaRRHH.UI.Enums;

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

        [HttpGet]
        public ActionResult Formulario(int idUsuario)
        {
            ViewBag.TituloPanel = Etiquetas.TituloPanelFormularioModificacionEquipos;
            try
            {
                var model = new EquiposUsuario();

                var equiposUsuario = RequerimientoEquipoDAL.ListadoEquiposAsignadosPorUsuario(idUsuario);

                if (equiposUsuario != null)
                {
                    model.Equipos = equiposUsuario;
                }

                ViewBag.UsuarioID = idUsuario;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.EquiposRequeridos = new List<string>();
                return View(new CodificacionEquipoCompleta());
            }
        }

        [HttpPost]
        public ActionResult _CambiarEstado(int idUsuario, int idEstado, int idRequerimientoEquipo, string tipoEquipo, int idEquipo, string observaciones)
        {
            var respuesta = new RespuestaTransaccion();
            switch (tipoEquipo)
            {
                case TipoEquipoEnum.Equipo:
                    respuesta = RequerimientoEquipoDAL.ActualizarRequerimientoEquipoUsuario(idRequerimientoEquipo, idEstado, observaciones);
                    break;

                case TipoEquipoEnum.HerramientaAdicional:
                    respuesta = RequerimientoEquipoDAL.ActualizarRequerimientoEquipoHerramientaAdicional(idRequerimientoEquipo, idEstado, observaciones);
                    break;
            }           

            if (Response.StatusCode == (int)HttpStatusCode.OK)
            {
                UsuarioInfo usuario = UsuarioDAL.ConsultarUsuario(idUsuario);
                var equipo = EquipoDAL.ConsultarEquipo(idEquipo);
                
                string body = GetEmailTemplate("TemplateReasignacionEquipo");

                string[] roles = { "COORDINADOR RRHH", "COORDINADOR HELP DESK" };

                var usuariosDestinatarios = UsuarioDAL.ObtenerMailCorporativosPorRoles(roles);

                body = body.Replace("@ViewBag.Usuario", usuario.NombresApellidos);
                body = body.Replace("@ViewBag.Equipo", equipo.Nombre);

                NotificacionesDAL.CrearNotificacion(new Notificaciones
                {
                    NombreTarea = "Cambio Estado de Asignación de Equipo",
                    DescripcionTarea = "Notificación de cambio de Estado de Asignación para el usuario.",
                    NombreEmisor = nombreCorreoEmisor,
                    CorreoEmisor = correoEmisor,
                    ClaveCorreo = claveEmisor,
                    AdjuntosCorreo = "",
                    CorreosDestinarios = string.Join(";", usuariosDestinatarios),
                    AsuntoCorreo = "REASIGNACION EQUIPO",
                    NombreArchivoPlantillaCorreo = TemplateNotificaciones,
                    CuerpoCorreo = body,
                    FechaEnvioCorreo = DateTime.Now,
                    Canal = CanalNotificaciones,
                    Tipo = "REASIGNACIÓN EQUIPO"
                });

            }

            return Json(respuesta, JsonRequestBehavior.DenyGet);
        }
    }
}