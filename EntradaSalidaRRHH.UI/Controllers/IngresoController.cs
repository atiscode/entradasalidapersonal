using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

//Extensión para Query string dinámico
using System.Linq.Dynamic;

using EntradaSalidaRRHH.Repositorios;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Text;
using EntradaSalidaRRHH.UI.Helper;
using System.IO;
using System.Configuration;
using System.Drawing;
using OfficeOpenXml.Drawing;

namespace EntradaSalidaRRHH.UI.Controllers
{
    [Autenticado]
    public class IngresoController : BaseController
    {
        private List<string> columnasReportesBasicos = new List<string> { "TIPO DE INGRESO", "EMPRESA", "ÁREA", "DEPARTAMENTO", "PERSONA A LA QUE REEMPLAZA" };

        // GET: Ingreso
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

        public ActionResult _Formulario(int? id)
        {
            ViewBag.TituloModal = "DATOS INGRESO";
            try
            {
                Ingreso model = new Ingreso();
                int? cargoUsuario = 0;
                int? empresa = 0;

                if (id.HasValue)
                {
                    model = IngresoDAL.ConsultarIngreso(id.Value);

                    //Setear cargo usuario
                    var usuario = IngresoDAL.ListadoIngreso(null, null, null, id).FirstOrDefault();
                    if (usuario != null)
                    {
                        cargoUsuario = usuario.Cargo;
                        empresa = usuario.Empresa;
                    }
                }

                ViewBag.Cargo = cargoUsuario;
                ViewBag.Empresa = empresa;

                return PartialView(model);
            }
            catch (Exception ex)
            {
                return PartialView(new Ingreso());
            }
        }

        public ActionResult Formulario(int? id, int? usuarioID)
        {
            try
            {
                Ingreso model = new Ingreso();
                int? cargoUsuario = 0;
                int? empresa = 0;

                if (id.HasValue)
                {
                    model = IngresoDAL.ConsultarIngreso(id.Value);

                    //Setear cargo usuario
                    var usuario = IngresoDAL.ListadoIngreso(null, null, null, id).FirstOrDefault();
                    if (usuario != null)
                    {
                        cargoUsuario = usuario.Cargo;
                        empresa = usuario.Empresa;
                    }
                }

                ViewBag.Cargo = cargoUsuario;
                ViewBag.Empresa = empresa;

                return View(model);
            }
            catch (Exception ex)
            {
                return View(new Ingreso());
            }
        }

        [HttpPost]
        public ActionResult Create(Ingreso formulario)
        {
            try
            {
                bool ingresoExistente = IngresoDAL.IngresoExistente(formulario.FichaIngresoID);
                bool emailAsignadoExistente = IngresoDAL.EmailAsignadoExistente(formulario.CorreoAsignado);
                bool emailExistenteUsuario = UsuarioDAL.CorreoExistente(formulario.CorreoAsignado);
                //bool emailExistenteEnOtroUsuario = UsuarioDAL.ExisteCorreoNormalCorporativo(formulario.CorreoAsignado);

                if (!Validaciones.ValidarMail(formulario.CorreoAsignado))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeCorreoIncorrecto } }, JsonRequestBehavior.AllowGet);

                //Si el email ya lo tiene otro usuario (normal o corporativo)
                //if (emailExistenteEnOtroUsuario)
                //    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeCorreoIncorrecto } }, JsonRequestBehavior.AllowGet);
                
                //Si el email ya está asignado al Ingreso de otro usuario
                if (emailAsignadoExistente || emailExistenteUsuario)
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeEmailExistenteAsociadoUsuario } }, JsonRequestBehavior.AllowGet);

                if (ingresoExistente)
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeIngresoUsuarioExistente } }, JsonRequestBehavior.AllowGet);

                var fichaIngreso = FichaIngresoDAL.ListadoFichaIngreso(null, null, null, formulario.FichaIngresoID).FirstOrDefault();

                var actualizacionDatosUsuario = UsuarioDAL.ActualizarDatosUsuarioFichaIngreso(new Usuario
                {
                    IdUsuario = fichaIngreso.UsuarioID,
                    Departamento = formulario.Departamento,
                    Area = formulario.Area,
                    IdEmpresa = formulario.Empresa,
                    MailCorporativo = formulario.CorreoAsignado,
                    Cargo = formulario.Cargo,
                }, true);

                if (actualizacionDatosUsuario.Estado)
                    Resultado = IngresoDAL.CrearIngreso(formulario);

                string enlace = GetUrlSitio(Url.Action("Index", "Login"));
                var destinatarios = PerfilesDAL.ConsultarCorreoNotificacion(14);
                string body = GetEmailTemplate("TemplateIngreso");
                body = body.Replace("@ViewBag.EnlaceDirecto", enlace);
                body = body.Replace("@ViewBag.EnlaceSecundario", enlace);
                body = body.Replace("@ViewBag.Empleado", fichaIngreso.NombresApellidosUsuario);

                //Siempre que la ficha de ingreso haya sido creado con éxito.
                if (Resultado.Estado)
                {
                    var usuario = UsuarioDAL.ConsultarUsuario(fichaIngreso.UsuarioID);

                    var notificacion = NotificacionesDAL.CrearNotificacion(new Notificaciones
                    {
                        NombreTarea = "Datos de Ingreso",
                        DescripcionTarea = "Correo de notificación de registro de datos de ingreso un nuevo empleado.",
                        NombreEmisor = nombreCorreoEmisor,
                        CorreoEmisor = correoEmisor,
                        ClaveCorreo = claveEmisor,
                        CorreosDestinarios = destinatarios,
                        AsuntoCorreo = "NOTIFICACION DE INGRESO",
                        NombreArchivoPlantillaCorreo = TemplateNotificaciones,
                        CuerpoCorreo = body,
                        AdjuntosCorreo = "",//ruta,
                        FechaEnvioCorreo = DateTime.Now,
                        Empresa = usuario.NombreEmpresa,
                        Canal = CanalNotificaciones,
                        Tipo = "DATOS INGRESO",
                    });
                }
                else
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = actualizacionDatosUsuario.Respuesta } }, JsonRequestBehavior.AllowGet);

                return Json(new { Resultado = Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Edit(Ingreso formulario)
        {
            try
            {
                if (!Validaciones.ValidarMail(formulario.CorreoAsignado))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeCorreoIncorrecto } }, JsonRequestBehavior.AllowGet);

                var fichaIngreso = FichaIngresoDAL.ListadoFichaIngreso(null, null, null, formulario.FichaIngresoID).FirstOrDefault();

                var actualizacionDatosUsuario = UsuarioDAL.ActualizarDatosUsuarioFichaIngreso(new Usuario
                {
                    IdUsuario = fichaIngreso.UsuarioID,
                    Departamento = formulario.Departamento,
                    Area = formulario.Area,
                    Cargo = formulario.Cargo,
                    IdEmpresa = formulario.Empresa,
                    MailCorporativo = formulario.CorreoAsignado,
                }, true);

                if (actualizacionDatosUsuario.Estado)
                    Resultado = IngresoDAL.ActualizarIngreso(formulario);
                else
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = actualizacionDatosUsuario.Respuesta } }, JsonRequestBehavior.AllowGet);

                return Json(new { Resultado = Resultado }, JsonRequestBehavior.AllowGet);
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
                return Json(new { Resultado = Resultado }, JsonRequestBehavior.AllowGet);

            RespuestaTransaccion resultado = IngresoDAL.EliminarIngreso(id);
            return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
        }

        #region REPORTE EXCEL
        [Autenticado]
        [HttpPost]
        public ActionResult GenerarReporte(int id)
        {
            try
            {
                var package = new ExcelPackage();

                var informacionCompleta = FichaIngresoDAL.ConsultarFichaIngresoCompleta(id);

                //PALETA DE COLORES QPH
                var colorGrisOscuroEstiloQPH = Color.FromArgb(89, 89, 89);
                var colorGrisClaroEstiloQPH = Color.FromArgb(240, 240, 240);
                var colorGrisClaro2EstiloQPH = Color.FromArgb(112, 117, 128);
                var colorGrisClaro4EstiloQPH = Color.FromArgb(178, 178, 178);
                var colorGrisClaro3EstiloQPH = Color.FromArgb(220, 220, 220);
                var colorBlancoEstiloQPH = Color.FromArgb(255, 255, 255);
                var colorNegroEstiloQPH = Color.FromArgb(0, 0, 0);
                var colorNaranjaEstiloQPH = Color.FromArgb(247, 148, 29);
                #region Cabecera

                var ws = package.Workbook.Worksheets.Add(informacionCompleta.FichaIngreso.NombresApellidosUsuario);

                int columnaFinalDocumentoExcel = 10;
                int columnaInicialDocumentoExcel = 1;
                var espaciofilas = 4.25;
                var valorfilatitulos = 20.25;
                var valorfiladetalles = 18;
                var cabeceraparte1 = 4;

                ws.PrinterSettings.PaperSize = ePaperSize.A4;//ePaperSize.A3;
                ws.PrinterSettings.Orientation = eOrientation.Portrait;
                ws.PrinterSettings.HorizontalCentered = true;
                ws.PrinterSettings.FitToPage = true;
                ws.PrinterSettings.FitToWidth = 1;
                ws.PrinterSettings.FitToHeight = 0;
                ws.PrinterSettings.FooterMargin = 0.70M;
                ws.PrinterSettings.TopMargin = 0.50M;
                ws.PrinterSettings.LeftMargin = 0.70M;
                ws.Column(10).PageBreak = true;
                ws.PrinterSettings.Scale = 75;


                ws.Cells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells.Style.Fill.BackgroundColor.SetColor(Color.White);
                ws.Cells[3, 3, 1000, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;

                var pathUbicacion = Server.MapPath("~/Content/img/LogoQPHReporteFinal.png");

                Image img = System.Drawing.Image.FromFile(@pathUbicacion);
                ExcelPicture pic = ws.Drawings.AddPicture("Sample", img);

                pic.SetPosition(1, 1, 0, 40);
                pic.SetSize(184, 70);
                ws.Row(2).Height = 60;

                //LOGO
                using (var range = ws.Cells[1, 1, 2, 3])
                {
                    range.Style.Fill.BackgroundColor.SetColor(colorGrisOscuroEstiloQPH);
                    range.Merge = true;
                }

                // TITULO FICHA
                using (var range = ws.Cells[1, 4, 2, 10])
                {

                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Merge = true;
                    range.Value = "FICHA DE INGRESO";
                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);

                    range.Style.Font.Name = "Raleway";
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    range.Style.Font.Bold = true;
                    range.Style.Font.Size = 18;

                }

                #endregion


                #region Datos personales

                ws.Cells[cabeceraparte1, 1, cabeceraparte1, columnaFinalDocumentoExcel].Merge = true;
                ws.Cells[cabeceraparte1, 1, cabeceraparte1, columnaFinalDocumentoExcel].Value = "DATOS PERSONALES";
                ws.Cells[cabeceraparte1, 1, cabeceraparte1, columnaFinalDocumentoExcel].Style.Font.Size = 12;
                ws.Cells[cabeceraparte1, 1, cabeceraparte1, columnaFinalDocumentoExcel].Style.Font.Bold = true;
                ws.Cells[cabeceraparte1, 1, cabeceraparte1, columnaFinalDocumentoExcel].Style.Fill.BackgroundColor.SetColor(colorGrisOscuroEstiloQPH);
                ws.Cells[cabeceraparte1, 1, cabeceraparte1, columnaFinalDocumentoExcel].Style.Font.Color.SetColor(colorBlancoEstiloQPH);
                ws.Cells[cabeceraparte1, 1, cabeceraparte1, columnaFinalDocumentoExcel].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                ws.Cells[cabeceraparte1, 1, cabeceraparte1, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                ws.Cells[cabeceraparte1, 1, cabeceraparte1, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                ws.Cells[cabeceraparte1, 1, cabeceraparte1, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                ws.Cells[cabeceraparte1, 1, cabeceraparte1, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                ws.Row(cabeceraparte1).Height = valorfilatitulos;

                cabeceraparte1++;


                using (var range = ws.Cells[cabeceraparte1, 9, 12, 10])
                {
                    if (informacionCompleta.DatosIngreso.Foto != null)
                    {
                        //obtener foto
                        byte[] bytes = (byte[])informacionCompleta.DatosIngreso.Foto; ;
                        String strBase64 = Convert.ToBase64String(bytes);
                        Image image = ByteArrayToImage(bytes);
                        ExcelPicture photo = ws.Drawings.AddPicture("Sample1", image);

                        photo.SetPosition(4, 4, 8, 4);
                        photo.SetSize(175, 155);
                    }

                    range.Merge = true;
                    range.Value = "FOTO";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;


                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    range.Style.Fill.BackgroundColor.SetColor(colorBlancoEstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                }
                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, 2])
                {

                    range.Merge = true;
                    range.Value = "Nombres:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Merge = true;
                    ws.Cells[range.Start.Row, range.Columns + 1].Value = informacionCompleta.FichaIngreso.NombresUsuario; // Valor campo
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;

                }

                cabeceraparte1++;



                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, 2])
                {

                    range.Merge = true;
                    range.Value = "Apellidos: ";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;


                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Merge = true;
                    ws.Cells[range.Start.Row, range.Columns + 1].Value = informacionCompleta.FichaIngreso.ApellidosUsuario; // Valor campo
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.Indent = 1;

                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;
                }

                cabeceraparte1++;

                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, 2])
                {

                    range.Merge = true;

                    range.Value = "Cédula de Identidad:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 6].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.FichaIngreso.identificacion;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;
                }

                cabeceraparte1++;

                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, 2])
                {
                    range.Merge = true;
                    range.Value = "Género:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 6].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.FichaIngreso.TextoCatalogoGenero; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 6].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 6].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 6].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 6].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;
                }
                cabeceraparte1++;

                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, 2])
                {
                    range.Merge = true;
                    range.Value = "Estado Civil:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Merge = true;
                    ws.Cells[range.Start.Row, range.Columns + 1].Value = informacionCompleta.FichaIngreso.TextoCatalogoEstadoCivil;  // Valor campo
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;

                }
                cabeceraparte1++;

                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, 2])
                {

                    range.Merge = true;
                    range.Value = "Tipo de Sangre: ";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Merge = true;
                    ws.Cells[range.Start.Row, range.Columns + 1].Value = informacionCompleta.FichaIngreso.TextoCatalogoTipoSangre;  // Valor campo
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;
                }

                cabeceraparte1++;

                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, 2])
                {
                    range.Merge = true;
                    range.Value = "Fecha de Nacimiento: ";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Merge = true;
                    ws.Cells[range.Start.Row, range.Columns + 1].Value = informacionCompleta.FichaIngreso.FechaNacimiento != null ? informacionCompleta.FichaIngreso.FechaNacimiento.Value.ToString("dd/MM/yyyy") : string.Empty;   // Valor campo
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[range.Start.Row, range.Columns + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;

                }
                cabeceraparte1++;

                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, 2])
                {

                    range.Merge = true;
                    range.Value = "Nacionalidad: ";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Merge = true;
                    ws.Cells[range.Start.Row, range.Columns + 1].Value = informacionCompleta.FichaIngreso.TextoCatalogoNacionalidad;  // Valor campo
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[range.Start.Row, range.Columns + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 6].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;



                }
                cabeceraparte1++;
                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, 2])
                {

                    range.Merge = true;
                    range.Value = "Lugar de Nacimiento: ";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 3].Merge = true;
                    ws.Cells[range.Start.Row, range.Columns + 1].Value = informacionCompleta.FichaIngreso.TextoCatalogoLugarNacimiento;  // Valor campo
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 3].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 3].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 3].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                }


                using (var range = ws.Cells[cabeceraparte1, 6, cabeceraparte1, 7])
                {

                    range.Merge = true;
                    range.Value = "Ciudad de residencia:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;


                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.FichaIngreso.TextoCatalogoCiudadResidencia; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;

                }
                cabeceraparte1++;



                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, 2])
                {

                    range.Merge = true;
                    range.Value = "Teléfono:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.FichaIngreso.Telefono; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;
                }

                using (var range = ws.Cells[cabeceraparte1, 6, cabeceraparte1, 7])
                {
                    range.Merge = true;

                    range.Value = "N. Celular:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.FichaIngreso.Celular; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;
                }

                cabeceraparte1++;


                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, 2])
                {
                    range.Merge = true;
                    range.Value = "Correo Electrónico:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.FichaIngreso.Mail; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                }

                using (var range = ws.Cells[cabeceraparte1, 6, cabeceraparte1, 7])
                {

                    range.Merge = true;
                    range.Value = "Fecha de Ingreso: ";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.FichaIngreso.FechaIngreso != null ? informacionCompleta.FichaIngreso.FechaIngreso.Value.ToString("dd/MM/yyyy") : string.Empty;  // Valor campo
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;
                }
                cabeceraparte1++;

                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, 2])
                {
                    range.Merge = true;
                    range.Value = "Banco:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;

                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.FichaIngreso.TextoCatalogoBanco; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;
                }
                cabeceraparte1++;


                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, 2])
                {

                    range.Merge = true;
                    range.Value = "Número de Cuenta:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.FichaIngreso.NumeroCuentaBancaria; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                }

                using (var range = ws.Cells[cabeceraparte1, 6, cabeceraparte1, 7])
                {

                    range.Merge = true;
                    range.Value = "Tipo de Cuenta:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.FichaIngreso.NumeroCuentaBancaria; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;
                }

                cabeceraparte1++;



                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, columnaFinalDocumentoExcel])
                {

                    range.Merge = true;
                    range.Value = "DIRECCIÓN ";
                    range.Style.Font.Bold = true;
                    range.Style.Fill.BackgroundColor.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Color.SetColor(colorBlancoEstiloQPH);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfilatitulos; ;
                }
                cabeceraparte1++;

                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, 2])
                {

                    range.Merge = true;
                    range.Value = "Calle Principal:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;

                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.FichaIngreso.DireccionCallePrincipal; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                }

                using (var range = ws.Cells[cabeceraparte1, 6, cabeceraparte1, 7])
                {

                    range.Merge = true;
                    range.Value = "Calle Secundaria:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.FichaIngreso.DireccionCalleSecundaria; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;
                }
                cabeceraparte1++;

                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, 2])
                {

                    range.Merge = true;
                    range.Value = "N. Casa:";
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.FichaIngreso.DireccionNumeroCasa; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                }

                using (var range = ws.Cells[cabeceraparte1, 6, cabeceraparte1, 7])
                {

                    range.Merge = true;
                    range.Value = "Conjunto Residencial:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.FichaIngreso.DireccionConjuntoResidencial; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;
                }

                cabeceraparte1++;

                #endregion

                int cabeceraparte2 = cabeceraparte1;
                ws.Row(cabeceraparte2).Height = espaciofilas;
                cabeceraparte2++;
                #region ContactoEmergencia
                ws.Cells[cabeceraparte2, 1, cabeceraparte2, columnaFinalDocumentoExcel].Merge = true;
                ws.Cells[cabeceraparte2, 1, cabeceraparte2, columnaFinalDocumentoExcel].Value = "EN CASO DE EMERGENCIA LLAMAR A";
                ws.Cells[cabeceraparte2, 1, cabeceraparte2, columnaFinalDocumentoExcel].Style.Font.Size = 12;
                ws.Cells[cabeceraparte2, 1, cabeceraparte2, columnaFinalDocumentoExcel].Style.Font.Bold = true;
                ws.Cells[cabeceraparte2, 1, cabeceraparte2, columnaFinalDocumentoExcel].Style.Fill.BackgroundColor.SetColor(colorGrisOscuroEstiloQPH);
                ws.Cells[cabeceraparte2, 1, cabeceraparte2, columnaFinalDocumentoExcel].Style.Font.Color.SetColor(colorBlancoEstiloQPH);
                ws.Cells[cabeceraparte2, 1, cabeceraparte2, columnaFinalDocumentoExcel].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                ws.Cells[cabeceraparte2, 1, cabeceraparte2, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                ws.Cells[cabeceraparte2, 1, cabeceraparte2, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                ws.Cells[cabeceraparte2, 1, cabeceraparte2, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                ws.Cells[cabeceraparte2, 1, cabeceraparte2, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                ws.Row(cabeceraparte2).Height = valorfilatitulos;
                cabeceraparte2++;

                using (var range = ws.Cells[cabeceraparte2, 1, cabeceraparte2, 2])
                {

                    range.Merge = true;
                    range.Value = "Nombre:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.FichaIngreso.NombresContactoEmergencia; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                }

                using (var range = ws.Cells[cabeceraparte2, 6, cabeceraparte2, 7])
                {

                    range.Merge = true;
                    range.Value = "Parentesco:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.FichaIngreso.TextoCatalogoParentescoContactoEmergencia; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte2).Height = valorfiladetalles;
                }

                cabeceraparte2++;

                using (var range = ws.Cells[cabeceraparte2, 1, cabeceraparte2, 2])
                {

                    range.Merge = true;
                    range.Value = "Teléfono:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.FichaIngreso.TelefonoContactoEmergencia; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                }

                using (var range = ws.Cells[cabeceraparte2, 6, cabeceraparte2, 7])
                {

                    range.Merge = true;
                    range.Value = "N. Celular:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.FichaIngreso.CelularContactoEmergencia; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte2).Height = valorfiladetalles;
                }

                cabeceraparte2++;


                #endregion

                int finCabecera = cabeceraparte2;
                ws.Row(finCabecera).Height = espaciofilas;

                finCabecera++;

                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Merge = true;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Value = "ESTUDIOS";
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Font.Size = 12;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Font.Bold = true;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Fill.BackgroundColor.SetColor(colorGrisOscuroEstiloQPH);
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Font.Color.SetColor(colorBlancoEstiloQPH);
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                ws.Row(finCabecera).Height = valorfilatitulos;
                finCabecera++;

                Int32 col = 1;
                int contador = 1;
                #region DetalleEstudios
                if (informacionCompleta.Estudios.Any())
                {
                    col = 1;
                    int totalColumnasDetalleEstudios = 5;

                    for (int i = 1; i <= totalColumnasDetalleEstudios; i++)
                    {
                        //ws.Column(col).Width = 18;

                        ws.Cells[finCabecera, col].Style.Fill.PatternType = ExcelFillStyle.Solid;

                        ws.Cells[finCabecera, col].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                        ws.Cells[finCabecera, col].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                        ws.Cells[finCabecera, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[finCabecera, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        // Assign borders
                        ws.Cells[finCabecera, col].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                        ws.Cells[finCabecera, col].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                        ws.Cells[finCabecera, col].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                        ws.Cells[finCabecera, col].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                        switch (i)
                        {
                            case 1:
                                ws.Cells[finCabecera, col].Value = "TIPO";
                                ws.Cells[finCabecera, col, finCabecera, 2].Merge = true;
                                ws.Cells[finCabecera, col, finCabecera, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, col, finCabecera, 2].Style.Fill.BackgroundColor.SetColor(colorGrisClaro2EstiloQPH);
                                ws.Cells[finCabecera, col, finCabecera, 2].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, col, finCabecera, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, col, finCabecera, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                // Assign borders
                                ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, col, finCabecera, 2].Style.Font.Bold = true;
                                break;

                            case 2:
                                ws.Cells[finCabecera, 3].Value = "INSTITUCIÓN";
                                ws.Cells[finCabecera, 3, finCabecera, 5].Merge = true;

                                ws.Cells[finCabecera, 3, finCabecera, 5].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 3, finCabecera, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 3, finCabecera, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                // Assign borders
                                ws.Cells[finCabecera, 3, finCabecera, 5].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 3, finCabecera, 5].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 3, finCabecera, 5].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 3, finCabecera, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Bold = true;
                                break;
                            case 3:
                                ws.Cells[finCabecera, 6].Value = "TÍTULO";
                                ws.Cells[finCabecera, 6, finCabecera, 7].Merge = true;
                                ws.Cells[finCabecera, 6, finCabecera, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, 6, finCabecera, 7].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 6, finCabecera, 7].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 6, finCabecera, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 6, finCabecera, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                // Assign borders
                                ws.Cells[finCabecera, 6, finCabecera, 7].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 6, finCabecera, 7].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 6, finCabecera, 7].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 6, finCabecera, 7].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 6, finCabecera, 7].Style.Font.Bold = true;

                                break;

                            case 4:
                                ws.Cells[finCabecera, 8].Value = "AÑO";
                                ws.Cells[finCabecera, 8, finCabecera, 8].Merge = true;
                                ws.Cells[finCabecera, 8, finCabecera, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, 8, finCabecera, 8].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 8, finCabecera, 8].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 8, finCabecera, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 8, finCabecera, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                // Assign borders
                                ws.Cells[finCabecera, 8, finCabecera, 8].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 8, finCabecera, 8].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 8, finCabecera, 8].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 8, finCabecera, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 8, finCabecera, 8].Style.Font.Bold = true;

                                break;

                            case 5:
                                ws.Cells[finCabecera, 9].Value = "PAÍS";
                                ws.Cells[finCabecera, 9, finCabecera, 10].Merge = true;
                                ws.Cells[finCabecera, 9, finCabecera, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, 9, finCabecera, 10].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 9, finCabecera, 10].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 9, finCabecera, 10].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 9, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                // Assign borders
                                ws.Cells[finCabecera, 9, finCabecera, 10].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 9, finCabecera, 10].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 9, finCabecera, 10].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 9, finCabecera, 10].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 9, finCabecera, 10].Style.Font.Bold = true;

                                break;
                            default:
                                ws.Cells[finCabecera, col++].Value = "";
                                break;
                        }

                        ws.Row(finCabecera).Height = 20.25;
                    }

                    finCabecera++;

                    contador = 1;
                    foreach (var column in informacionCompleta.Estudios)
                    {
                        col = 1;
                        for (int i = 1; i <= totalColumnasDetalleEstudios; i++)
                        {
                            ws.Cells[finCabecera, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            ws.Cells[finCabecera, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            // Assign borders
                            ws.Cells[finCabecera, col].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                            ws.Cells[finCabecera, col].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                            ws.Cells[finCabecera, col].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                            ws.Cells[finCabecera, col].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                            switch (i)
                            {
                                case 1:
                                    ws.Cells[finCabecera, col].Value = column.TextoCatalogoTipoEstudio;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Merge = true;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.WrapText = true;
                                    break;
                                case 2:
                                    ws.Cells[finCabecera, 3].Value = column.TextoCatalogoInstitucion;
                                    ws.Cells[finCabecera, 3, finCabecera, 5].Merge = true;
                                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.WrapText = true;
                                    break;
                                case 3:
                                    ws.Cells[finCabecera, 6].Value = column.TextoCatalogoTitulo;
                                    ws.Cells[finCabecera, 6, finCabecera, 7].Merge = true;
                                    ws.Cells[finCabecera, 6, finCabecera, 7].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 6, finCabecera, 7].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 6, finCabecera, 7].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 6, finCabecera, 7].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 6, finCabecera, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.WrapText = true;
                                    break;
                                case 4:
                                    ws.Cells[finCabecera, 8].Value = column.AnioFinalizacion;
                                    ws.Cells[finCabecera, 8, finCabecera, 8].Merge = true;
                                    ws.Cells[finCabecera, 8, finCabecera, 8].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 8, finCabecera, 8].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 8, finCabecera, 8].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 8, finCabecera, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 8, finCabecera, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    break;
                                case 5:
                                    ws.Cells[finCabecera, 9].Value = column.TextoCatalogoPais;
                                    ws.Cells[finCabecera, 9, finCabecera, 10].Merge = true;
                                    ws.Cells[finCabecera, 9, finCabecera, 10].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 9, finCabecera, 10].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 9, finCabecera, 10].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 9, finCabecera, 10].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 9, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.WrapText = true;
                                    break;
                                default:
                                    ws.Cells[finCabecera, col++].Value = "";
                                    break;
                            }
                            ws.Row(finCabecera).Height = 20.25;

                        }


                        finCabecera++;
                        contador++;

                    }
                    ws.Row(finCabecera).Height = espaciofilas;
                    finCabecera += 1;
                }
                #endregion

                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Merge = true;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Value = "EXPERIENCIA:   (EMPRESAS ANTERIORES)";
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Font.Size = 12;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Font.Bold = true;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Fill.BackgroundColor.SetColor(colorGrisOscuroEstiloQPH);
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Font.Color.SetColor(colorBlancoEstiloQPH);
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                ws.Row(finCabecera).Height = valorfilatitulos;
                finCabecera++;

                #region Detalle Experiencia
                if (informacionCompleta.Experiencias.Any())
                {
                    col = 1;
                    int totalColumnasDetalleExperiencia = 3;

                    for (int i = 1; i <= totalColumnasDetalleExperiencia; i++)
                    {
                        //ws.Column(col).Width = 18;

                        ws.Cells[finCabecera, col].Style.Fill.PatternType = ExcelFillStyle.Solid;

                        ws.Cells[finCabecera, col].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                        ws.Cells[finCabecera, col].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                        ws.Cells[finCabecera, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[finCabecera, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        // Assign borders
                        ws.Cells[finCabecera, col].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                        ws.Cells[finCabecera, col].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                        ws.Cells[finCabecera, col].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                        ws.Cells[finCabecera, col].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                        switch (i)
                        {
                            case 1:
                                ws.Cells[finCabecera, col].Value = "EMPRESA";
                                ws.Cells[finCabecera, col, finCabecera, 3].Merge = true;
                                ws.Cells[finCabecera, col, finCabecera, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, col, finCabecera, 3].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, col, finCabecera, 3].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, col, finCabecera, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, col, finCabecera, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                // Assign borders
                                ws.Cells[finCabecera, col, finCabecera, 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, col, finCabecera, 3].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, col, finCabecera, 3].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, col, finCabecera, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, col, finCabecera, 3].Style.Font.Bold = true;
                                break;

                            case 2:
                                ws.Cells[finCabecera, 4].Value = "PERIODO";
                                ws.Cells[finCabecera, 4, finCabecera, 7].Merge = true;
                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                // Assign borders
                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.Font.Bold = true;
                                break;
                            case 3:
                                ws.Cells[finCabecera, 8].Value = "CARGO";
                                ws.Cells[finCabecera, 8, finCabecera, 10].Merge = true;
                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                // Assign borders
                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Bold = true;

                                break;
                            default:
                                ws.Cells[finCabecera, col++].Value = "";
                                break;
                        }

                        ws.Row(finCabecera).Height = 20.25;
                    }

                    finCabecera++;

                    contador = 1;
                    foreach (var column in informacionCompleta.Experiencias)
                    {
                        col = 1;
                        for (int i = 1; i <= totalColumnasDetalleExperiencia; i++)
                        {
                            ws.Cells[finCabecera, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            ws.Cells[finCabecera, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            // Assign borders
                            ws.Cells[finCabecera, col].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                            ws.Cells[finCabecera, col].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                            ws.Cells[finCabecera, col].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                            ws.Cells[finCabecera, col].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                            switch (i)
                            {
                                case 1:
                                    ws.Cells[finCabecera, col].Value = column.Empresa;
                                    ws.Cells[finCabecera, col, finCabecera, 3].Merge = true;
                                    ws.Cells[finCabecera, col, finCabecera, 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 3].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 3].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[finCabecera, col, finCabecera, 3].Style.WrapText = true;
                                    break;
                                case 2:
                                    ws.Cells[finCabecera, 4].Value = column.FechaInicio != null ? column.FechaInicio.Value.ToString("DESDE " + "dd/MM/yyyy") : string.Empty
                                        + " HASTA  " + column.FechaFin != null ? column.FechaFin.Value.ToString("DESDE " + "dd/MM/yyyy") : string.Empty;
                                    ws.Cells[finCabecera, 4, finCabecera, 7].Merge = true;
                                    ws.Cells[finCabecera, 4, finCabecera, 7].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 4, finCabecera, 7].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 4, finCabecera, 7].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 4, finCabecera, 7].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[finCabecera, 4, finCabecera, 7].Style.WrapText = true;
                                    break;
                                case 3:
                                    ws.Cells[finCabecera, 8].Value = column.TextoCatalogoCargo;
                                    ws.Cells[finCabecera, 8, finCabecera, 10].Merge = true;
                                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    var caracteresD = column.TextoCatalogoCargo.Count();

                                    if (caracteresD <= 30)
                                    {
                                        ws.Row(finCabecera).Height = 20.25;
                                    }
                                    else if (caracteresD <= 60)
                                    {
                                        ws.Row(finCabecera).Height = 35;
                                    }
                                    else if (caracteresD <= 120)
                                    {
                                        ws.Row(finCabecera).Height = 50;
                                    }
                                    else
                                    {
                                        ws.Row(caracteresD).Height = 65;
                                    }
                                    break;
                                default:
                                    ws.Cells[finCabecera, col++].Value = "";
                                    break;
                            }
                            ws.Row(finCabecera).Height = 20.25;

                        }


                        finCabecera++;
                        contador++;

                    }
                    ws.Row(finCabecera).Height = espaciofilas;
                    finCabecera += 1;
                }
                #endregion

                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Merge = true;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Value = "DESCRIPCIÓN DE CARGAS FAMILIARES (CONYUGE E HIJOS)";
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Font.Size = 12;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Font.Bold = true;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Fill.BackgroundColor.SetColor(colorGrisOscuroEstiloQPH);
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Font.Color.SetColor(colorBlancoEstiloQPH);
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                ws.Row(finCabecera).Height = valorfilatitulos;
                finCabecera++;

                #region Detalle CargasFamiliares
                if (informacionCompleta.CargasFamiliares.Any())
                {
                    col = 1;
                    int totalColumnasDetalleCargasFamiliares = 3;

                    for (int i = 1; i <= totalColumnasDetalleCargasFamiliares; i++)
                    {
                        //ws.Column(col).Width = 18;

                        ws.Cells[finCabecera, col].Style.Fill.PatternType = ExcelFillStyle.Solid;

                        ws.Cells[finCabecera, col].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                        ws.Cells[finCabecera, col].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                        ws.Cells[finCabecera, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[finCabecera, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        // Assign borders
                        ws.Cells[finCabecera, col].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                        ws.Cells[finCabecera, col].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                        ws.Cells[finCabecera, col].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                        ws.Cells[finCabecera, col].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                        switch (i)
                        {
                            case 1:
                                ws.Cells[finCabecera, col].Value = "NOMBRE";
                                ws.Cells[finCabecera, col, finCabecera, 3].Merge = true;
                                ws.Cells[finCabecera, col, finCabecera, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, col, finCabecera, 3].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, col, finCabecera, 3].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, col, finCabecera, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, col, finCabecera, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                // Assign borders
                                ws.Cells[finCabecera, col, finCabecera, 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, col, finCabecera, 3].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, col, finCabecera, 3].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, col, finCabecera, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, col, finCabecera, 3].Style.Font.Bold = true;
                                break;

                            case 2:
                                ws.Cells[finCabecera, 4].Value = "SEXO";
                                ws.Cells[finCabecera, 4, finCabecera, 7].Merge = true;
                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                // Assign borders
                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 4, finCabecera, 7].Style.Font.Bold = true;
                                break;
                            case 3:
                                ws.Cells[finCabecera, 8].Value = "FECHA DE NACIMIENTO";
                                ws.Cells[finCabecera, 8, finCabecera, 10].Merge = true;
                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                // Assign borders
                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Bold = true;

                                break;
                            default:
                                ws.Cells[finCabecera, col++].Value = "";
                                break;
                        }

                        ws.Row(finCabecera).Height = 20.25;
                    }

                    finCabecera++;

                    contador = 1;
                    foreach (var column in informacionCompleta.CargasFamiliares)
                    {
                        col = 1;
                        for (int i = 1; i <= totalColumnasDetalleCargasFamiliares; i++)
                        {
                            ws.Cells[finCabecera, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            ws.Cells[finCabecera, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            // Assign borders
                            ws.Cells[finCabecera, col].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                            ws.Cells[finCabecera, col].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                            ws.Cells[finCabecera, col].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                            ws.Cells[finCabecera, col].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                            switch (i)
                            {
                                case 1:
                                    ws.Cells[finCabecera, col].Value = column.Nombres + " " + column.Apellidos;
                                    ws.Cells[finCabecera, col, finCabecera, 3].Merge = true;
                                    ws.Cells[finCabecera, col, finCabecera, 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 3].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 3].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                    ws.Cells[finCabecera, col, finCabecera, 3].Style.WrapText = true;
                                    break;
                                case 2:
                                    ws.Cells[finCabecera, 4].Value = column.TextoCatalogoGeneroCargaFamiliar;
                                    ws.Cells[finCabecera, 4, finCabecera, 7].Merge = true;
                                    ws.Cells[finCabecera, 4, finCabecera, 7].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 4, finCabecera, 7].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 4, finCabecera, 7].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 4, finCabecera, 7].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 4, finCabecera, 7].Style.WrapText = true;
                                    break;
                                case 3:
                                    ws.Cells[finCabecera, 8].Value = column.FechaNacimiento != null ? column.FechaNacimiento.Value.ToString("dd/MM/yyyy") : string.Empty;
                                    ws.Cells[finCabecera, 8, finCabecera, 10].Merge = true;
                                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    break;
                                default:
                                    ws.Cells[finCabecera, col++].Value = "";
                                    break;
                            }

                            ws.Row(finCabecera).Height = 20.25;

                        }


                        finCabecera++;
                        contador++;
                        ws.Row(finCabecera).Height = espaciofilas;

                    }

                    finCabecera += 1;
                }
                #endregion

                #region DATOS INGRESO

                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Merge = true;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Value = "DATOS DE INGRESO";
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Font.Size = 12;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Font.Bold = true;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Fill.BackgroundColor.SetColor(colorGrisOscuroEstiloQPH);
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Font.Color.SetColor(colorBlancoEstiloQPH);
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                ws.Row(finCabecera).Height = valorfilatitulos;
                finCabecera++;

                using (var range = ws.Cells[finCabecera, 1, finCabecera, 2])
                {

                    range.Merge = true;
                    range.Value = "Empresa:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.DatosIngreso.TextoCatalogoEmpresa; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(finCabecera).Height = valorfiladetalles;
                }


                finCabecera++;

                using (var range = ws.Cells[finCabecera, 1, finCabecera, 2])
                {

                    range.Merge = true;
                    range.Value = "Cargo:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.DatosIngreso.TextoCatalogoCargo; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                }

                using (var range = ws.Cells[finCabecera, 6, finCabecera, 7])
                {

                    range.Merge = true;
                    range.Value = "Área:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.DatosIngreso.TextoCatalogoArea; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(finCabecera).Height = valorfiladetalles;
                }

                finCabecera++;


                using (var range = ws.Cells[finCabecera, 1, finCabecera, 2])
                {

                    range.Merge = true;
                    range.Value = "Tipo de Ingreso:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.DatosIngreso.TextoCatalogoTipoIngreso; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                }

                using (var range = ws.Cells[finCabecera, 6, finCabecera, 7])
                {

                    range.Merge = true;
                    range.Value = "Sueldo:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.DatosIngreso.Sueldo; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Numberformat.Format = "0.00";
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(finCabecera).Height = valorfiladetalles;
                }

                finCabecera++;


                #endregion

                //ancho de columnas
                ws.Column(1).Width = 13;
                ws.Column(2).Width = 13;
                ws.Column(3).Width = 13;
                ws.Column(4).Width = 13;
                ws.Column(5).Width = 13;
                ws.Column(6).Width = 13;
                ws.Column(7).Width = 13;
                ws.Column(8).Width = 13;
                ws.Column(9).Width = 13;
                ws.Column(10).Width = 13;

                //ancho de filas
                ws.Row(3).Height = 4.25;


                #region Pie de Pagina 


                #endregion

                //FORMATO FUENTE TEXTO DE TODO EL DOCUMENTO
                using (var range = ws.Cells[3, 1, finCabecera, columnaFinalDocumentoExcel])
                {
                    range.Style.Font.Size = 10;
                    range.Style.Font.Name = "Raleway";
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }


                string basePath = ConfigurationManager.AppSettings["RepositorioDocumentos"];
                string rutaArchivos = basePath + "\\ENTRADA_SALIDAPERSONAL_RRHH\\FICHA_INGRESO";

                string nombreficha = informacionCompleta.FichaIngreso.NombresApellidosUsuario;

                var anioActual = DateTime.Now.Year.ToString();
                var almacenFisicoTemporal = Auxiliares.CrearCarpetasDirectorio(rutaArchivos, new List<string>() { anioActual, nombreficha });
                var almacenFisicoOfficeToPDF = Auxiliares.CrearCarpetasDirectorio(Server.MapPath("~/OfficeToPDF/"), new List<string>());

                string pathExcel = Path.Combine(almacenFisicoTemporal, "FICHA-INGRESO-" + informacionCompleta.FichaIngreso.NombresApellidosUsuario + ".xlsx");

                FileInfo fi = new FileInfo(pathExcel);
                package.SaveAs(fi);



                return Json(new { Resultado = new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa }, PathsArchivos = pathExcel }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message }, PathsArchivos = new List<string> { } }, JsonRequestBehavior.AllowGet);
            }
        }

        [Autenticado]
        public ActionResult DescargarArchivo(string path)
        {
            try
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                string fileName = Path.GetFileName(path);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception ex)
            {
                string mensaje = "Verifique que el servidor tenga instaladas las dependencias necesarias que requiere la aplicación, revisar archivo OfficeToPDF ({0})";
                ViewBag.Excepcion = string.Format(mensaje, ex.Message.ToString());
                return View("~/Views/Error/InternalServerError.cshtml");
            }
        }
        #endregion

        #region REPORTES BASICOS
        public ActionResult DescargarReporteFormatoExcel()
        {
            var collection = IngresoDAL.ListadoReporteBasico();
            var package = GetEXCEL(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(package.GetAsByteArray(), XlsxContentType, "Listado.xlsx");
        }

        public ActionResult DescargarReporteFormatoPDF()
        {
            // Seleccionar las columnas a exportar
            var collection = IngresoDAL.ListadoReporteBasico();
            byte[] buffer = GetPDF(columnasReportesBasicos, collection.Cast<object>().ToList(), "Listado de Ingresos");

            return File(buffer, PDFContentType, "ReportePDF.pdf");
        }

        public ActionResult DescargarReporteFormatoCSV()
        {
            var collection = IngresoDAL.ListadoReporteBasico();
            byte[] buffer = GetCSV(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(buffer, CSVContentType, $"Listado.csv");
        }
        #endregion
    }
}