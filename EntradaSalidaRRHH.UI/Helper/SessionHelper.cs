using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Web.Security;
using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.DAL.Metodos;
using NLog;

namespace EntradaSalidaRRHH.UI.Helper
{
    public class SessionHelper
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        //Valida que el usuario esté logeado o que las sesión esté activa
        public static bool ValidarSesionUsuario()
        {
            try
            {
                if (!(HttpContext.Current.Session["UsuarioLogeado"] is UsuarioInfo usuario))
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                Log.Info(ex, "Se produjo una excepción.");
                return false;
            }
        }

        //Retorna todos los datos del Usuario en sesión
        public static UsuarioInfo GetDatosUsuariosSession()
        {
            try
            {
                UsuarioInfo usuario = (UsuarioInfo)HttpContext.Current.Session["UsuarioLogeado"];

                if (usuario != null)
                    return usuario;
                else
                    return new UsuarioInfo ();
            }
            catch (Exception ex)
            {
                Log.Info(ex, "Se produjo una excepción.");
                return new UsuarioInfo();
            }
        }

        public static void SetDatosCompletosUsuarioSession(UsuarioInfo usuario)
        {
            try
            {
                HttpContext.Current.Session["UsuarioLogeado"] = usuario;
            }
            catch (Exception ex)
            {
                Log.Info(ex, "Se produjo una excepción.");
            }
        }

        public static string GetPermisosOpciones()
        {
            try
            {
                if (!(HttpContext.Current.Session["PermisosOpcion"] is string permisos))
                    return "";
                else
                    return permisos;
            }
            catch (Exception ex)
            {
                Log.Info(ex, "Se produjo una excepción.");
                return string.Empty;
            }
        }

        public static void DestroyUserSession()
        {
            try
            {
                HttpContext.Current.Session["UsuarioLogeado"] = null;
                FormsAuthentication.SignOut();
            }
            catch (Exception ex)
            {
                Log.Info(ex, "Se produjo una excepción.");
            }
        }

        #region  Para Implementación Identity
        //Para implementación identity
        public static int GetUser()
        {
            int user_id = 0;
            if (HttpContext.Current.User != null && HttpContext.Current.User.Identity is FormsIdentity)
            {
                FormsAuthenticationTicket ticket = ((FormsIdentity)HttpContext.Current.User.Identity).Ticket;
                if (ticket != null)
                {
                    user_id = Convert.ToInt32(ticket.UserData);
                }
            }
            return user_id;
        }

        //Para implementación Identity
        public static bool ExistUserInSession()
        {
            return HttpContext.Current.User.Identity.IsAuthenticated;
        }

        public static void AddUserToSession(string id)
        {
            bool persist = true;
            var cookie = FormsAuthentication.GetAuthCookie("usuario", persist);

            cookie.Name = FormsAuthentication.FormsCookieName;
            cookie.Expires = DateTime.Now.AddMonths(3);

            var ticket = FormsAuthentication.Decrypt(cookie.Value);
            var newTicket = new FormsAuthenticationTicket(ticket.Version, ticket.Name, ticket.IssueDate, ticket.Expiration, ticket.IsPersistent, id);

            cookie.Value = FormsAuthentication.Encrypt(newTicket);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
        #endregion

        public static List<UsuarioRolMenuPermisoR> ObtenerRolMenuPermisos(string nombreControlador)
        {
            List<UsuarioRolMenuPermisoR> listadoFinal = new List<UsuarioRolMenuPermisoR>();
            List<UsuarioRolMenuPermisoInfo> listado = new List<UsuarioRolMenuPermisoInfo>();
            try
            {
                UsuarioInfo usuario = GetDatosUsuariosSession();
                var user = usuario.IdUsuario.ToString();

                if (user == null)
                    return listadoFinal;


                listado = ManejoPermisosDAL.ConsultarRolMenuPermiso(usuario.IdUsuario, nombreControlador);

                foreach (var item in listado)
                {
                    UsuarioRolMenuPermisoR tmp = new UsuarioRolMenuPermisoR();
                    tmp.IDRolMenuPermiso = item.IDRolMenuPermiso;
                    tmp.RolID = item.RolID;
                    tmp.NombreRol = item.NombreRol;
                    tmp.PerfilID = item.PerfilID;
                    tmp.NombrePerfil = item.NombrePerfil;
                    tmp.MenuID = item.MenuID;
                    tmp.NombreMenu = item.NombreMenu;
                    tmp.EnlaceMenu = item.EnlaceMenu;
                    tmp.MenuPadre = item.MenuPadre;
                    tmp.IDCatalogo = item.IDCatalogo;
                    tmp.CodigoCatalogo = item.CodigoCatalogo;
                    tmp.TextoCatalogoAccion = item.TextoCatalogoAccion;
                    tmp.CreadoPorID = item.CreadoPorID;
                    tmp.CreadoPor = item.CreadoPor;
                    tmp.ActualizadoPorID = item.ActualizadoPorID;
                    tmp.ActualizadoPor = item.ActualizadoPor;
                    tmp.CreatedAt = item.CreatedAt;
                    tmp.UpdatedAt = item.UpdatedAt;
                    tmp.Estado = item.Estado;
                    tmp.MetodoControlador = item.MetodoControlador;
                    tmp.NombreControlador = item.NombreControlador;
                    tmp.AccionEnlace = item.AccionEnlace;

                    listadoFinal.Add(tmp);
                }

                return listadoFinal;
            }
            catch (Exception ex)
            {
                return listadoFinal;
            }
        }

    }
}