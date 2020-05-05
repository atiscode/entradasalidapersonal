using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.UI.Helper;
using NLog;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.UI.Controllers
{
    [Autenticado]
    public class HomeController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AnotherLink()
        {
            return View("Index");
        }

        public ActionResult Menu()
        {
            //Obtener el codigo de usuario que se logeo
            var user = UsuarioLogeadoSession.Mail; 

         
                //Obtener listado de opciones del menu
            var list = UsuarioDAL.OpcionesMenuUsuario(user);

            return PartialView("_Menu", list);
        }
    }
}
