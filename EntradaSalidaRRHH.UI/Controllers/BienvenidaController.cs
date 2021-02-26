using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using EntradaSalidaRRHH.UI.Helper;
using System;
using System.Configuration;
using System.IO;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.UI.Controllers
{
    [Autenticado]
    public class BienvenidaController : BaseController
    {
        // GET: Bienvenida
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult _Formulario(int? id)
        {
            var model = id.HasValue ? UsuarioDAL.ConsultarUsuario(id.Value) : new UsuarioInfo();

            ViewBag.TituloModal = "BIENVENIDA";
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Create(Usuario formulario)
        {
            try
            {                
                Catalogo catalogo = CatalogoDAL.ConsultarCatalogo(formulario.IdEmpresa.Value);

                //El nombre del archivo de acumulación de décimos tiene que ser igual al código del catálogo de la empresa seleccionada.
                string nombreArchivo = "AcumulacionDecimos_"+ catalogo.CodigoCatalogo;

                if (string.IsNullOrEmpty(nombreArchivo))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = "El código de catálogo es requerido para la empresa " + catalogo } }, JsonRequestBehavior.AllowGet);

                string basePath = ConfigurationManager.AppSettings["RepositorioDocumentos"];

                //SI LA RUTA EN DISCO NO EXISTE LOS ARCHIVOS SE ALMACENAN EN LA CARPETA MISMO DEL PROYECTO
                string rutaBase = basePath + "\\RRHH\\Documentos\\AcumulacionDecimos";

                nombreArchivo += ".docx";
                //string pathServidor = Path.Combine(rutaBase, nombreArchivo);

                //SI LA RUTA EN DISCO NO EXISTE LOS ARCHIVOS SE ALMACENAN EN LA CARPETA MISMO DEL PROYECTO
                string ruta = AppDomain.CurrentDomain.BaseDirectory + "Documentos/DocumentosIngreso/AcumulacionDecimos/" + nombreArchivo;

                // En caso de que no exista el directorio, crearlo.
                //bool directorio = Directory.Exists(pathServidor);

                string rutaBaseDocumentosIngreso = AppDomain.CurrentDomain.BaseDirectory + "Documentos/DocumentosIngreso/";

                // Obtener los archivos del directorio
                DirectoryInfo directorioDocumentosIngreso = new DirectoryInfo(rutaBaseDocumentosIngreso);
                FileInfo[] archivos = directorioDocumentosIngreso.GetFiles("*.*");
                foreach (FileInfo file in archivos)
                {
                    ruta = ruta + ";" + file.FullName;                    
                }

                bool existeUsuario = UsuarioDAL.VerificarCorreoUsuarioExistente(formulario.Mail);

                //Solo registra el usuario si no existe
                if (!existeUsuario)
                    Resultado = UsuarioDAL.CrearUsuario(formulario);
                else
                {
                    //Si el usuario ya existe, setear el ID, para que lo tome en el envio de la notificacion
                    var usuario = UsuarioDAL.ConsultaUsuarioByEmail(formulario.Mail);
                    Resultado.EntidadID = usuario.IdUsuario;

                    Resultado.Estado = true;
                    Resultado.Respuesta = Mensajes.MensajeBienvenidaUsuarioExistente;
                }

                //Siempre que el usuario haya sido creado con éxito.
                if (Resultado.Estado || existeUsuario)
                {
                    string enlace = GetUrlSitio(Url.Action("NuevoIngreso", "FichaIngreso", new { usuarioID = Resultado.EntidadID }));

                    string body = GetEmailTemplate("TemplateBienvenida");

                    var fechaMañana = DateTime.Now.AddDays(1).Date.ToString();
                    var fechaOchoDias = DateTime.Now.AddDays(7).Date.ToString();

                    body = body.Replace("@ViewBag.EnlaceDirecto", enlace);
                    body = body.Replace("@ViewBag.EnlaceSecundario", enlace);
                    body = body.Replace("@ViewBag.fechaMañana", fechaMañana);
                    body = body.Replace("@ViewBag.fechaOchoDias", fechaOchoDias);

                    var notificacion = NotificacionesDAL.CrearNotificacion(new Notificaciones
                    {
                        NombreTarea = "Bienvenida",
                        DescripcionTarea = "Correo de bienvenida a los nuevos usuarios que ingresan a formar parte del corporativo.",
                        NombreEmisor = nombreCorreoEmisor,
                        CorreoEmisor = correoEmisor,
                        ClaveCorreo = claveEmisor,
                        CorreosDestinarios = formulario.Mail,
                        AsuntoCorreo = "BIENVENIDA",
                        NombreArchivoPlantillaCorreo = TemplateNotificaciones,
                        CuerpoCorreo = body,
                        AdjuntosCorreo = ruta,
                        FechaEnvioCorreo = DateTime.Now,
                        Empresa = catalogo.NombreCatalogo,
                        Canal = CanalNotificaciones,
                        Tipo = "BIENVENIDA"
                    });
                }

                return Json(new { Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}