using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using System.Text;
using OfficeOpenXml.Style;
using OfficeOpenXml;

//Extensión para Query string dinámico
using System.Linq.Dynamic;
using EntradaSalidaRRHH.Repositorios;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using System.IO;
using System.Web.Hosting;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Configuration;
using EntradaSalidaRRHH.UI.Helper;
using NLog;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace EntradaSalidaRRHH.UI.Controllers
{
    public class BaseController : Controller
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string PDFContentType = "application/pdf";
        public const string CSVContentType = "text/csv";

        //Repesitorio en el servidor para guardar y  buscar archivos
        public string basePathRepositorioDocumentos = ConfigurationManager.AppSettings["RepositorioDocumentos"];

        public string CanalNotificaciones = ConfigurationManager.AppSettings["CanalNotificaciones"];
        public string TemplateNotificaciones = ConfigurationManager.AppSettings["TemplateNotificaciones"];
        
        //IPS
        public string ipPublica = ConfigurationManager.AppSettings["IPDominioSalidaPublica"] ?? string.Empty;
        public string ipPrivada = ConfigurationManager.AppSettings["IPDominioPrivado"] ?? string.Empty;

        //Notificaciones normales
        public string correoEmisor = ConfigurationManager.AppSettings["CorreoEmisorNotificacionMasiva"];
        public string claveEmisor = ConfigurationManager.AppSettings["ClaveCorreoEmisorNotificacionMasiva"];
        public string nombreCorreoEmisor = ConfigurationManager.AppSettings["NombreCorreoEmisorMasivo"];

        //Notificaciones Masivas (mailing)
        public string correoEmisorMasivo = ConfigurationManager.AppSettings["CorreoEmisorNotificacionMasiva"];
        public string claveEmisorMasivo = ConfigurationManager.AppSettings["ClaveCorreoEmisorNotificacionMasiva"];
        public string nombreCorreoEmisorMasivo = ConfigurationManager.AppSettings["NombreCorreoEmisorMasivo"];

        //Objeto global de cada respuesta asincrona
        public RespuestaTransaccion Resultado = new RespuestaTransaccion();

        public JsonSerializerSettings settingsJsonDeserilize = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new BadDateFixingConverter("dd/MM/yyyy") }, //"dd/MM/yyyy HH:mm:ss"
            DateParseHandling = DateParseHandling.None
        };

        #region Variables de sesión

        private List<Equipo> CachedStaff
        {
            get { return Session["stafflist"] as List<Equipo>; }
            set { Session["stafflist"] = value; }
        }

        public UsuarioInfo UsuarioLogeadoSession
        {
            get { return Session["UsuarioLogeado"] as UsuarioInfo; }
            set { Session["UsuarioLogeado"] = value; }
        }

        public List<string> RecentAssetList
        {
            get
            {
                if (Session["RECENT_ASSET_LIST"] == null)
                    Session["RECENT_ASSET_LIST"] = new List<string>();

                return (List<string>)Session["RECENT_ASSET_LIST"];
            }
            set
            {
                Session["RECENT_ASSET_LIST"] = value;
            }
        }

        public string UserId
        {
            get
            {
                if (Session["Test"] != null)
                    return Session["Test"].ToString();
                else
                    return "";
            }

            set
            {
                Session["Test"] = value;
            }
        }

        //Guardar mensajes de respuestas de transacciones en Sesión
        public string ResponseMessage
        {
            get { return Session["ReponseMessage"] == null ? string.Empty : Session["ReponseMessage"].ToString(); }
            set { Session["ReponseMessage"] = value; }
        }
        #endregion

        //Setea a cada controlador en nombre del Grid , tal y como se encuentra escrito en el menú
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var opcion = filterContext.ActionParameters.FirstOrDefault(s => s.Key == "opcion");

            if (!string.IsNullOrEmpty(opcion.Key))
                ViewBag.NombreListado = opcion.Value; //Add whatever

            base.OnActionExecuting(filterContext);
        }
        #region Funcionalidades Genéricas para Grids
        public string BuildWhereDynamicClause(Dictionary<string, object> queryString)
        {
            string query = string.Empty;
            try
            {
                List<string> clausulas = new List<string>();

                string contains = "{0} LIKE '%{1}%'";
                string equals = "{0} = '{1}'";
                string NotEquals = "{0} != '{1}'";
                string StartsWith = "{0} LIKE '{1}%'";
                string EndWith = "{0} LIKE '%{1}'";

                string where = string.Empty;

                foreach (KeyValuePair<string, object> item in queryString)
                {
                    string columna = item.Key.Split('-').FirstOrDefault();

                    if (item.Key.Contains("contains"))
                    {
                        //query += string.Format(contains, columna, (item.Value ?? "").ToString());
                        clausulas.Add(string.Format(contains, columna, (item.Value ?? "").ToString().Trim()));
                    }
                    if (item.Key.Contains("equals"))
                    {
                        //query += string.Format(equals, columna, (item.Value ?? "").ToString());
                        clausulas.Add(string.Format(equals, columna, item.Value.ToString().Trim()));
                    }
                    if (item.Key.Contains("not-equals"))
                    {
                        //query += string.Format(NotEquals, columna, (item.Value ?? "").ToString());
                        clausulas.Add(string.Format(NotEquals, columna, (item.Value ?? "").ToString().Trim()));
                    }
                    if (item.Key.Contains("starts-with"))
                    {
                        //query += string.Format(StartsWith, columna, (item.Value ?? "").ToString());
                        clausulas.Add(string.Format(StartsWith, columna, (item.Value ?? "").ToString().Trim()));
                    }
                    if (item.Key.Contains("ends-with"))
                    {
                        //query += string.Format(EndWith, columna, (item.Value ?? "").ToString());
                        clausulas.Add(string.Format(EndWith, columna, (item.Value ?? "").ToString().Trim()));
                    }

                    where = " WHERE ";
                }

                query += clausulas.Any() ? where + string.Join(" AND ", clausulas.ToArray()) : string.Empty;

                return query;
            }
            catch (Exception ex)
            {
                return query;
            }
        }

        public Dictionary<string, object> GetQueryString(string queryString)
        {
            Dictionary<string, object> querystringDic = new Dictionary<string, object>();
            try
            {
                var parsed = HttpUtility.ParseQueryString(queryString);
                querystringDic = parsed.AllKeys.ToDictionary(k => k, k => (object)parsed[k]);

                querystringDic.Remove("_");

                //Parametros ya incluidos en el request del método IndexGrid
                querystringDic.Remove("search");
                querystringDic.Remove("page");
                querystringDic.Remove("order");
                querystringDic.Remove("sort");

                return querystringDic;
            }
            catch (Exception ex)
            {
                return querystringDic;
            }
        }
        #endregion

        #region Reportes Básicos Genéricos

        public ExcelPackage GetEXCEL(List<string> columnas, List<object> collection, string nombreHoja = "Listado")
        {
            ExcelPackage package = new ExcelPackage();
            var workSheet = package.Workbook.Worksheets.Add(nombreHoja);
            try
            {
                workSheet.TabColor = System.Drawing.Color.Black;
                workSheet.DefaultRowHeight = 10;

                workSheet.Row(1).Height = 20;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;

                int contador = 0;
                for (int i = 1; i <= columnas.Count; i++)
                {
                    workSheet.Cells[1, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Cells[1, i].Style.Font.Name = "Raleway";
                    workSheet.Cells[1, i].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    workSheet.Cells[1, i].Value = columnas.ElementAt(contador);
                    contador++;
                }

                //Body of table  
                CambiarColorFila(workSheet, 1, columnas.Count, System.Drawing.Color.FromArgb(240, 240, 240));

                int fila = 2;
                foreach (var item in collection)
                {
                    var objeto = Auxiliares.GetValoresCamposObjeto(item);
                    int columna = 1;
                    foreach (var valor in objeto)
                    {
                        workSheet.Cells[fila, columna].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        workSheet.Cells[fila, columna].Style.Font.Name = "Raleway";
                        workSheet.Cells[fila, columna].Value = valor;
                        columna++;
                    }
                    fila++;
                }

                for (int i = 1; i <= columnas.Count; i++)
                {
                    workSheet.Column(i).AutoFit();
                    workSheet.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                return package;
            }
            catch (Exception ex)
            {
                return package;
            }
        }

        private static void CambiarColorFila(ExcelWorksheet hoja, int fila, int totalColumnas, System.Drawing.Color color)
        {
            for (int i = 1; i <= totalColumnas; i++)
            {
                using (ExcelRange rowRange = hoja.Cells[fila, i])
                {
                    rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rowRange.Style.Fill.BackgroundColor.SetColor(color);
                }
            }
        }

        public byte[] GetPDF(List<string> columnas, List<object> collection, string tituloEncabezado = "Listado")
        {
            Document doc = new Document(PageSize.A4.Rotate(), 41, 41, 105, 55);

            using (MemoryStream output = new MemoryStream())
            {
                PdfWriter wri = PdfWriter.GetInstance(doc, output);

                wri.PageEvent = new ITextEvents(tituloEncabezado, Server.MapPath("/"));

                // Abrimos el archivo
                doc.Open();

                //Fonts para el PDF

                //Regular
                string fontpath = Server.MapPath("~/Content/fonts/ubuntu/Raleway-Regular.ttf");
                BaseFont customfont = BaseFont.CreateFont(fontpath, BaseFont.CP1252, BaseFont.EMBEDDED);
                //Bold
                string fontpathbold = Server.MapPath("~/Content/fonts/ubuntu/Raleway-Bold.ttf");
                BaseFont customfontbold = BaseFont.CreateFont(fontpathbold, BaseFont.CP1252, BaseFont.EMBEDDED);

                // Le colocamos el título y el autor
                // **Nota: Esto no será visible en el documento
                doc.AddTitle("Reporte PDF");
                doc.AddCreator("ATISCODE");

                //Cabecera
                Font _standardFontCabecera = new Font(customfontbold, 8f, 0, new BaseColor(0, 0, 0));
                //Listado
                Font _standardFont = new Font(customfont, 7f, 0, new BaseColor(0, 0, 0));

                int numeroColumnas = columnas.Count;

                // Tabla con los datos del listado
                PdfPTable tabla = new PdfPTable(numeroColumnas)
                {
                    WidthPercentage = 100
                };

                //Si el listado contiene elementos.
                if (collection.Any())
                {
                    for (int i = 0; i < numeroColumnas; i++)
                    {
                        tabla.AddCell(new PdfPCell(new Phrase(columnas[i], _standardFontCabecera))
                        {
                            //BorderWidth = 0.15f,
                            //BorderWidthBottom = 1.75f,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            BackgroundColor = new BaseColor(128, 128, 128),
                            BorderColor = new BaseColor(0, 0, 0),
                            FixedHeight = 30f
                        });
                    }

                    foreach (var item in collection)
                    {
                        var objeto = Auxiliares.GetValoresCamposObjeto(item);
                        foreach (var valor in objeto)
                        {
                            tabla.AddCell(new PdfPCell(new Phrase(valor.ToString(), _standardFont))
                            {
                                VerticalAlignment = Element.ALIGN_MIDDLE,
                                //BorderWidth = 0.15f,
                                //BorderWidthBottom = 1.75f,
                            });
                        }
                    }
                }
                else
                {
                    doc.Add(new Paragraph(Mensajes.MensajeNoDataListado, _standardFontCabecera));
                }

                // Finalmente, añadimos la tabla al documento PDF y cerramos el documento
                doc.Add(tabla);

                doc.Close();
                return output.ToArray();
            }
        }

        public byte[] GetCSV(List<string> columnas, List<object> collection)
        {
            // Build the file content
            var listadoCSV = new StringBuilder();
            collection.ForEach(line =>
            {
                listadoCSV.AppendLine(string.Join(",", line));
            });

            byte[] buffer = Encoding.Default.GetBytes($"{string.Join(",", columnas)}\r\n{listadoCSV.ToString()}");

            return buffer;
        }
        #endregion

        // Funcionalidad para Hacer Merge de varios archivos PDF
        private static bool MergeArhivosPDF(List<string> archivos, string EnRuta)
        {
            try
            {
                PdfReader reader = null;
                Document sourceDocument = null;
                PdfCopy pdfCopyProvider = null;
                PdfImportedPage importedPage;

                sourceDocument = new Document();
                pdfCopyProvider = new PdfCopy(sourceDocument, new FileStream(EnRuta, FileMode.Create));

                //output file Open  
                sourceDocument.Open();

                //files list wise Loop  
                for (int f = 0; f < archivos.Count; f++)
                {
                    int pages = TotalPageCount(archivos[f]);

                    reader = new PdfReader(archivos[f]);
                    //Add pages in new file  
                    for (int i = 1; i <= pages; i++)
                    {
                        importedPage = pdfCopyProvider.GetImportedPage(reader, i);
                        pdfCopyProvider.AddPage(importedPage);
                    }

                    reader.Close();
                }
                //save the output file  
                sourceDocument.Close();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        private static int TotalPageCount(string file)
        {
            using (StreamReader sr = new StreamReader(System.IO.File.OpenRead(file)))
            {
                Regex regex = new Regex(@"/Type\s*/Page[^s]");
                MatchCollection matches = regex.Matches(sr.ReadToEnd());

                return matches.Count;
            }
        }

        //Ruta Absoluta del Sitio publicado
        public string GetUrlSitio(string subRuta = null)
        {
            string domain = "172.16.36.18/"; //+ subRuta; //Request.Url.GetLeftPart(UriPartial.Authority); //  "172.16.36.18/" + subRuta;
            string urlPrincipal = string.Format("{0}{1}", domain, string.IsNullOrEmpty(subRuta) ? Url.Content("~") : subRuta);
            return urlPrincipal;
            //http:///recursos_humanos_pruebas/FichaIngreso/NuevoIngreso?usuarioID=15
        }

        #region Funcionalidades de Permisos Globales en la aplicación
        public List<string> GetMetodosControlador(string controlador)
        {
            List<string> NavItems = new List<string>();

            ReflectedControllerDescriptor controllerDesc = new ReflectedControllerDescriptor(this.GetType());
            foreach (ActionDescriptor action in controllerDesc.GetCanonicalActions())
            {
                bool validAction = true;

                object[] attributes = action.GetCustomAttributes(false);

                // Look at each attribute
                foreach (object filter in attributes)
                {
                    // Can we navigate to the action?
                    if (/*filter is HttpPostAttribute ||*/ filter is ChildActionOnlyAttribute)
                    {
                        validAction = false;
                        break;
                    }
                }
                if (validAction)
                    NavItems.Add(action.ActionName);
            }
            return NavItems;
        }

        public void Permisos(string nombreControlador)
        {
            //controlar permisos
            var usuario = UsuarioLogeadoSession.IdUsuario;

            ViewBag.NombreControlador = nombreControlador;
            ViewBag.AccionesUsuario = ManejoPermisosDAL.ListadoAccionesCatalogoUsuario(usuario, nombreControlador);
            ViewBag.AccionesControlador = GetMetodosControlador(nombreControlador);//Obtener Acciones del controlador
        }
        #endregion

        #region Notificaciones del Sistema
        public string GetEmailTemplate(string nombreTemplate)
        {
            try
            {
                var busquedaArchivo = Directory.GetFiles(HostingEnvironment.MapPath("~/Views/Base/"), nombreTemplate + ".cshtml", SearchOption.AllDirectories).FirstOrDefault();
                //string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/Content/templates/") + nombreTemplate + ".cshtml");
                string body = System.IO.File.ReadAllText(busquedaArchivo);
                return body.ToString();
            }
            catch (Exception ex)
            {
                string plantillaPorDefecto = "Default";
                string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/Views/Base/") + plantillaPorDefecto + ".cshtml");
                return body.ToString();
            }
        }

        public string GetBodyEmailTemplate(string template, string cuerpoCorreo)
        {
            try
            {
                var message = GetEmailTemplate(template);
                message = message.Replace("@ViewBag.CuerpoCorreo", cuerpoCorreo); //Cuerpo del correo
                return message;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public ActionResult TemplateBienvenida()
        {
            return View();
        }

        public ActionResult TemplateAprobacionEquipo()
        {
            return View();
        }

        public ActionResult TemplateAsignacionEquipo()
        {
            return View();
        }

        public ActionResult TemplateBienvenidaUsuario()
        {
            return View();
        }

        public ActionResult TemplateCambioClaveUsuario()
        {
            return View();
        }

        public ActionResult TemplateCodificacionEquipo()
        {
            return View();
        }

        public ActionResult TemplateComentarioAprobacion()
        {
            return View();
        }


        public ActionResult TemplateDesvinculacionPersonal()
        {
            return View();
        }

        public ActionResult TemplateFormularioIngreso()
        {
            return View();
        }

        public ActionResult TemplateIngreso()
        {
            return View();
        }

        public ActionResult TemplateRegistroUsuario()
        {
            return View();
        }
        public ActionResult TemplateRequerimientoEquipo()
        {
            return View();
        }
        public ActionResult TemplateResetClaveUsuario()
        {
            return View();
        }
        public ActionResult TemplateSolicitudAprobacion()
        {
            return View();
        }

        public ActionResult TemplateEnviosMasivosFocus()
        {
            return View();
        }
        #endregion

        //Convertir archivo de imagen a un Drawing Image Objeto de Excel
        public static System.Drawing.Image ByteArrayToImage(byte[] bArray)
        {
            if (bArray == null)
                return null;
            System.Drawing.Image img;
            try
            {
                MemoryStream bipimag = new MemoryStream(bArray);
                img = new System.Drawing.Bitmap(bipimag);
            }
            catch (Exception ex)
            {
                img = null;
            }
            return img;
        }
    }
}