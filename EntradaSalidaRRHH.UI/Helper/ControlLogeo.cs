using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EntradaSalidaRRHH.UI.Helper
{
    // Si no estamos logeado, regresamos al login
    public class AutenticadoAttribute : ActionFilterAttribute
    {
        //private SeguridadEntities db = new SeguridadEntities();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // Si el usuario no ha iniciado sesión o la sesión ya no está activa
            if (!SessionHelper.ValidarSesionUsuario())
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Login",
                    action = "Index"
                }));

                //Almacenar en una variable de sesion
                HttpContext.Current.Session["Resultado"] = "Su sesión ha caducado";
                HttpContext.Current.Session["Estado"] = "True";

            }
            //else
            //{
            //    UsuarioInfo usuario = SessionHelper.GetDatosUsuariosSession();


            //    var tipoRequestControlador = filterContext.RequestContext.HttpContext.Request.AcceptTypes.ToList();
            //    bool flag = false;

            //    if (tipoRequestControlador.Any())
            //    {
            //        flag = tipoRequestControlador.Contains("application/json");
            //    }

            //    string controlador = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            //    string accion = filterContext.ActionDescriptor.ActionName;
            //    List<UsuarioRolMenuPermisoInfo> listado = new List<UsuarioRolMenuPermisoInfo>();
            //    listado = ManejoPermisosDAL.ConsultarRolMenuPermiso(usuario.IdUsuario, controlador);

            //    int rolid = usuario.IdRol.Value;


            //    var ruta = MenuDAL.ConsultarRutaMenu(rolid);


            //    bool permisoOK = listado.Any(s => s.MetodoControlador == accion);
            //    bool RutaOK = ruta.Any(s => s.RUTAMENU == controlador);
            //    bool permisoIndex = false;

            //    //if (RutaOK)
            //    //{
            //    //    permisoIndex = true;
            //    //}


            //    if (!permisoOK && !accion.Equals("OrdenMenu") && !accion.Equals("CambiarClave") && !controlador.Equals("Bienvenida") && !controlador.Equals("Home") && !controlador.Equals("ManejoPermisos"))
            //    {
            //        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
            //        {
            //            controller = "Home",
            //            action = "Index"
            //        }));
            //    }

            //}

        }
    }

    // Si estamos logeado ya no podemos acceder a la página de Login
    public class NoLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (SessionHelper.ValidarSesionUsuario())
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Home",
                    action = "Index"
                }));
            }
        }
    }
}