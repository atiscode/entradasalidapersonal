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
using System.Configuration;
using System.IO;
using OfficeOpenXml.Drawing;
using System.Drawing;

namespace EntradaSalidaRRHH.UI.Controllers
{
    [Autenticado]
    public class DesvinculacionPersonalController : BaseController
    {
        private List<string> columnasReportesBasicos = new List<string> { "FECHA DE SALIDA", "MOTIVO", "PERSONA DESVINCULADA", "USUARIO RESPONSABLE DESVINCULACION", "SEGURO MEDICO", "ENCUESTA SALIDA" };

        // GET: DesvinculacionPersonal
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<PartialViewResult> _IndexGrid(string search, string sort = "", string order = "", long? page = 1)
        {
            //Permisos
            Permisos(ControllerContext.RouteData.Values["controller"].ToString());

            var listado = new List<DesvinculacionPersonalInfo>();
            ViewBag.NombreListado = Etiquetas.TituloGridDesvinculacionPersonal;
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
                        listado = DesvinculacionPersonalDAL.ListadoDesvinculacionPersonal(page.Value).OrderBy(sort + " " + order).ToList();
                    else
                        listado = DesvinculacionPersonalDAL.ListadoDesvinculacionPersonal(page.Value).ToList();
                }

                search = !string.IsNullOrEmpty(search) ? search.Trim() : "";

                if (!string.IsNullOrEmpty(search))//filter
                {
                    listado = DesvinculacionPersonalDAL.ListadoDesvinculacionPersonal(null, search);
                }

                if (!string.IsNullOrEmpty(whereClause) && string.IsNullOrEmpty(search))
                {
                    if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
                        listado = DesvinculacionPersonalDAL.ListadoDesvinculacionPersonal(null, null, whereClause).OrderBy(sort + " " + order).ToList();
                    else
                        listado = DesvinculacionPersonalDAL.ListadoDesvinculacionPersonal(null, null, whereClause);
                }
                else
                {

                    if (string.IsNullOrEmpty(search))
                        totalPaginas = DesvinculacionPersonalDAL.ObtenerTotalRegistrosListadoDesvinculacionPersonal();
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

        public ActionResult Formulario(int? id, int usuarioID)
        {
            ViewBag.TituloPanel = Etiquetas.TituloPanelFormularioDesvinculacionPersonal;

            int UsuarioID = usuarioID;

            List<string> equipos = new List<string>();
            try
            {
                DesvinculacionPersonalCompleta model = new DesvinculacionPersonalCompleta();
                model.Desvinculacion.FechaSalida = DateTime.Now;

                if (id.HasValue)
                    model = DesvinculacionPersonalDAL.ConsultarDesvinculacionPersonalCompleta(id);
                else
                {
                    var listadoEquiposAsignadosUsuario = RequerimientoEquipoDAL.ListadoEquiposAsignadosUsuariosRequerimientos(usuarioID);
                    foreach (var item in listadoEquiposAsignadosUsuario)
                    {
                        model.Detalles.Add(new DetalleEquiposEntregadosInfo
                        {
                            EquipoID = item.EquipoID,
                            Observaciones = "N/A",
                        });
                    }
                }

                ViewBag.UsuarioID = UsuarioID;

                return View(model);
            }
            catch (Exception ex)
            {
                return View(new DesvinculacionPersonalCompleta());
            }
        }

        public ActionResult _InformacionEquipo(int id, int usuarioID)
        {
            ViewBag.TituloModal = "Información del Equipo";

            InformacionEquipoCodificacionInfo model = CodificacionEquipoDAL.ConsultarInformacionEquipoCodificado(id, usuarioID) ?? new InformacionEquipoCodificacionInfo();

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Create(DesvinculacionPersonal formulario, List<DetalleEquiposEntregados> detalles)
        {
            try
            {
                detalles = detalles ?? new List<DetalleEquiposEntregados>();

                formulario.UsuarioResponsableID = UsuarioLogeadoSession.IdUsuario;

                var usuario = UsuarioDAL.ConsultarUsuario(formulario.UsuarioID);
                var destinatarios = PerfilesDAL.ConsultarCorreoNotificacion(16);
                string enlace = GetUrlSitio(Url.Action("Index", "Login"));

                string body = GetEmailTemplate("TemplateDesvinculacionPersonal");
                body = body.Replace("@ViewBag.EnlaceDirecto", enlace);
                body = body.Replace("@ViewBag.EnlaceSecundario", enlace);
                body = body.Replace("@ViewBag.Empresa", usuario.NombreEmpresa);

                Resultado = DesvinculacionPersonalDAL.CrearDesvinculacionPersonal(formulario, detalles);
                //Siempre que la ficha de ingreso haya sido creado con éxito.
                if (Resultado.Estado)
                {
                    var notificacion = NotificacionesDAL.CrearNotificacion(new Notificaciones
                    {
                        NombreTarea = "Desvinculación de Personal",
                        DescripcionTarea = "Correo de notificación de desvinculación de personal.",
                        NombreEmisor = nombreCorreoEmisor,
                        CorreoEmisor = correoEmisor,
                        ClaveCorreo = claveEmisor,
                        CorreosDestinarios = destinatarios,
                        AsuntoCorreo = "NOTIFICACION DE DESVINCULACION",
                        NombreArchivoPlantillaCorreo = TemplateNotificaciones,
                        CuerpoCorreo = body,
                        AdjuntosCorreo = "",//ruta,
                        FechaEnvioCorreo = DateTime.Now,
                        Empresa = usuario.NombreEmpresa,
                        Canal = CanalNotificaciones,
                        Tipo = "DESVINCULACION",
                    });
                }

                //var desactivarUsuario = UsuarioDAL.DesactivarUsuario(formulario.UsuarioID);

                //if (desactivarUsuario.Estado)
                //{
                //    Resultado = DesvinculacionPersonalDAL.CrearDesvinculacionPersonal(formulario, detalles);
                //    //Siempre que la ficha de ingreso haya sido creado con éxito.
                //    if (Resultado.Estado)
                //    {
                //        var notificacion = NotificacionesDAL.CrearNotificacion(new Notificaciones
                //        {
                //            NombreTarea = "Desvinculación de Personal",
                //            DescripcionTarea = "Correo de notificación de desvinculación de personal.",
                //            NombreEmisor = nombreCorreoEmisor,
                //            CorreoEmisor = correoEmisor,
                //            ClaveCorreo = claveEmisor,
                //            CorreosDestinarios = destinatarios,
                //            AsuntoCorreo = "NOTIFICACION DE DESVINCULACION",
                //            NombreArchivoPlantillaCorreo = TemplateNotificaciones,
                //            CuerpoCorreo = body,
                //            AdjuntosCorreo = "",//ruta,
                //            FechaEnvioCorreo = DateTime.Now,
                //            Empresa = usuario.NombreEmpresa,
                //            Canal = CanalNotificaciones,
                //            Tipo = "DESVINCULACION",
                //        });
                //    }
                //}
                //else
                //{
                //    desactivarUsuario.Respuesta += " El usuario no pudo ser desactivado.";
                //    return Json(new { Resultado = desactivarUsuario }, JsonRequestBehavior.AllowGet);
                //}

                return Json(new { Resultado = Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Edit(DesvinculacionPersonal formulario, List<DetalleEquiposEntregados> detalles)
        {
            try
            {
                detalles = detalles ?? new List<DetalleEquiposEntregados>();

                formulario.UsuarioResponsableID = UsuarioLogeadoSession.IdUsuario;

                Resultado = DesvinculacionPersonalDAL.ActualizarDesvinculacionPersonal(formulario, detalles);

                //var desactivarUsuario = UsuarioDAL.DesactivarUsuario(formulario.UsuarioID);

                //if (desactivarUsuario.Estado)
                //    Resultado = DesvinculacionPersonalDAL.ActualizarDesvinculacionPersonal(formulario, detalles);
                //else
                //{
                //    desactivarUsuario.Respuesta += " .El usuario no pudo ser desactivado.";
                //    return Json(new { Resultado = desactivarUsuario }, JsonRequestBehavior.AllowGet);
                //}

                return Json(new { Resultado = Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult _SeleccionarUsuario()
        {
            ViewBag.TituloModal = "Seleccionar el Usuario.";
            return PartialView();
        }

        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            if (id == 0)
                return Json(new { Resultado = Resultado }, JsonRequestBehavior.AllowGet);

            RespuestaTransaccion resultado = DesvinculacionPersonalDAL.EliminarDesvinculacionPersonal(id);
            return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
        }

        #region REPORTE EXCEL CODIFICACIÓN EQUIPO
        [HttpPost]
        public ActionResult GenerarReporte(int id)
        {
            try
            {
                var package = new ExcelPackage();

                var informacionCompleta = DesvinculacionPersonalDAL.ConsultarDesvinculacionPersonalCompleta(id);


                //  PALETA DE COLORES QPH
                var colorGrisOscuroEstiloQPH = Color.FromArgb(89, 89, 89);
                var colorGrisClaroEstiloQPH = Color.FromArgb(240, 240, 240);
                var colorGrisClaro2EstiloQPH = Color.FromArgb(112, 117, 128);
                var colorGrisClaro4EstiloQPH = Color.FromArgb(178, 178, 178);
                var colorGrisClaro3EstiloQPH = Color.FromArgb(220, 220, 220);
                var colorBlancoEstiloQPH = Color.FromArgb(255, 255, 255);
                var colorNegroEstiloQPH = Color.FromArgb(0, 0, 0);
                var colorNaranjaEstiloQPH = Color.FromArgb(247, 148, 29);
                #region Cabecera

                var ws = package.Workbook.Worksheets.Add(informacionCompleta.Desvinculacion.UsuarioDesvinculado.ToString());

                int columnaFinalDocumentoExcel = 12;
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

                System.Drawing.Image img = System.Drawing.Image.FromFile(@pathUbicacion);
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

                //TITULO FICHA
                using (var range = ws.Cells[1, 4, 2, columnaFinalDocumentoExcel])
                {
                    var motivo = informacionCompleta.Desvinculacion.TextoMotivo;


                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Merge = true;
                    range.Value = "DESVINCULACIÓN DE PERSONAL (" + motivo + ")";
                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);

                    range.Style.Font.Name = "Raleway";
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    range.Style.Font.Bold = true;
                    range.Style.Font.Size = 18;

                }

                #endregion


                #region Datos EX COLABORADOR

                ws.Cells[cabeceraparte1, 1, cabeceraparte1, columnaFinalDocumentoExcel].Merge = true;
                ws.Cells[cabeceraparte1, 1, cabeceraparte1, columnaFinalDocumentoExcel].Value = "DATOS DEL EX COLABORADOR";
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


                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, 2])
                {

                    range.Merge = true;
                    range.Value = "NOMBRE Y APELLIDO: ";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 4].Merge = true;
                    ws.Cells[range.Start.Row, range.Columns + 1].Value = informacionCompleta.Desvinculacion.UsuarioDesvinculado; // Valor campo
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 4].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 4].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 4].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 4].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                }


                using (var range = ws.Cells[cabeceraparte1, 7, cabeceraparte1, 8])
                {

                    range.Merge = true;
                    range.Value = "EMPRESA";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;


                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.Desvinculacion.TextoCatalogoEmpresa; // Valor campo
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
                    range.Value = "CÉDULA: ";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 4].Merge = true;
                    ws.Cells[range.Start.Row, range.Columns + 1].Value = informacionCompleta.Desvinculacion.IdentificacionUsuario;  // Valor campo
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 4].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 4].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 4].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 4].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                }


                using (var range = ws.Cells[cabeceraparte1, 7, cabeceraparte1, 8])
                {

                    range.Merge = true;
                    range.Value = "CIUDAD";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;


                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.Desvinculacion.TextoCatalogoCiudadResidencia; // Valor campo
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
                    range.Value = "CARGO:";
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

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.Desvinculacion.TextoCatalogoCargo; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                }

                using (var range = ws.Cells[cabeceraparte1, 7, cabeceraparte1, 8])
                {

                    range.Merge = true;
                    range.Value = "ÁREA: ";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.Desvinculacion.TextoCatalogoArea;  // Valor campo
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
                    range.Value = "JEFE INMEDIATO:";
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

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.Desvinculacion.TextoCatalogoJefeDirecto; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;
                }

                using (var range = ws.Cells[cabeceraparte1, 7, cabeceraparte1, 8])
                {

                    range.Merge = true;
                    range.Value = "TEL. CONTACTO:";
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
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.Desvinculacion.CelularUsuario; // Valor campo
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
                    range.Value = "FECHA INGRESO:";
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

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.Desvinculacion.FechaIngreso != null ? informacionCompleta.Desvinculacion.FechaIngreso.Value.ToString("dd/MM/yyyy") : string.Empty; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;
                }


                using (var range = ws.Cells[cabeceraparte1, 7, cabeceraparte1, 8])
                {

                    range.Merge = true;
                    range.Value = "FECHA SALIDA:";
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
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.Desvinculacion.FechaSalida != null ? informacionCompleta.Desvinculacion.FechaSalida.ToString("dd/MM/yyyy") : string.Empty; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;
                }

                cabeceraparte1++;
                ws.Row(cabeceraparte1).Height = espaciofilas;
                cabeceraparte1++;

                #endregion

                #region MOTIVO

                ws.Cells[cabeceraparte1, 1, cabeceraparte1, columnaFinalDocumentoExcel].Merge = true;
                ws.Cells[cabeceraparte1, 1, cabeceraparte1, columnaFinalDocumentoExcel].Value = "CONTROL DE PROCESOS Y DOCUMENTOS";
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


                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, 2])
                {

                    range.Merge = true;
                    range.Value = "DESCRIPCIÓN: ";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.Columns + 1].Value = informacionCompleta.Desvinculacion.TextoMotivo; // Valor campo
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;
                }

                cabeceraparte1++;


                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, 2])
                {

                    range.Merge = true;
                    range.Value = "NOTIFICACIÓN DE SALIDA DEL SEGURO MÉDICO: ";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    range.Style.WrapText = true;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 4].Merge = true;
                    ws.Cells[range.Start.Row, range.Columns + 1].Value = informacionCompleta.Desvinculacion.TextoEstadoSalidaSeguroMedico;  // Valor campo
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 4].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 4].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 4].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 4].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                }


                using (var range = ws.Cells[cabeceraparte1, 7, cabeceraparte1, 8])
                {

                    range.Merge = true;
                    range.Value = "ENCUESTA DE SALIDA";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    range.Style.WrapText = true;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.Desvinculacion.TextoEstadoEncuestaSalida; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = 30;

                }
                cabeceraparte1++;



                using (var range = ws.Cells[cabeceraparte1, 1, cabeceraparte1, 2])
                {
                    range.Merge = true;
                    range.Value = "OBSERVACIONES:";
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
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = ""; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;
                }


                cabeceraparte1++;

                #endregion

                ws.Row(cabeceraparte1).Height = espaciofilas;
                cabeceraparte1++;



                int finCabecera = cabeceraparte1;
                ws.Row(finCabecera).Height = espaciofilas;

                finCabecera++;



                Int32 col = 1;
                int contador = 1;
                #region DetalleEquipoUsuario

                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Merge = true;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Value = "ENTREGA DE EQUIPOS";
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


                if (informacionCompleta.Detalles.Any())
                {

                    col = 1;
                    int totalColumnasDetalleEstudios = 7;

                    for (int i = 1; i <= totalColumnasDetalleEstudios; i++)
                    {
                        ws.Column(col).Width = 18;

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
                                ws.Cells[finCabecera, 1].Value = "N";
                                ws.Cells[finCabecera, 1, finCabecera, 1].Merge = true;
                                ws.Cells[finCabecera, 1, finCabecera, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, 1, finCabecera, 1].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 1, finCabecera, 1].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 1, finCabecera, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 1, finCabecera, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                //Assign borders
                                ws.Cells[finCabecera, 1, finCabecera, 1].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 1, finCabecera, 1].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 1, finCabecera, 1].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 1, finCabecera, 1].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 1, finCabecera, 1].Style.Font.Bold = true;
                                break;
                            case 2:
                                ws.Cells[finCabecera, 2].Value = "EQUIPO";
                                ws.Cells[finCabecera, 2, finCabecera, 3].Merge = true;
                                ws.Cells[finCabecera, 2, finCabecera, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, 2, finCabecera, 3].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 2, finCabecera, 3].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 2, finCabecera, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 2, finCabecera, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                //Assign borders
                                ws.Cells[finCabecera, 2, finCabecera, 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 2, finCabecera, 3].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 2, finCabecera, 3].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 2, finCabecera, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 2, finCabecera, 3].Style.Font.Bold = true;
                                break;

                            case 3:
                                ws.Cells[finCabecera, 4].Value = "SERIE";
                                ws.Cells[finCabecera, 4, finCabecera, 5].Merge = true;

                                ws.Cells[finCabecera, 4, finCabecera, 5].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 4, finCabecera, 5].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 4, finCabecera, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 4, finCabecera, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                //Assign borders
                                ws.Cells[finCabecera, 4, finCabecera, 5].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 4, finCabecera, 5].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 4, finCabecera, 5].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 4, finCabecera, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 4, finCabecera, 5].Style.Font.Bold = true;
                                break;
                            case 4:
                                ws.Cells[finCabecera, 6].Value = "PROVEEDOR";
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
                            case 5:
                                ws.Cells[finCabecera, 8].Value = "PRECIO";
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
                                ws.Cells[finCabecera, 8, finCabecera, 8].Style.WrapText = true;

                                break;


                            case 6:
                                ws.Cells[finCabecera, 9].Value = "ENTREGADO";
                                ws.Cells[finCabecera, 9, finCabecera, 9].Merge = true;
                                ws.Cells[finCabecera, 9, finCabecera, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, 9, finCabecera, 9].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 9, finCabecera, 9].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 9, finCabecera, 9].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 9, finCabecera, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                // Assign borders
                                ws.Cells[finCabecera, 9, finCabecera, 9].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 9, finCabecera, 9].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 9, finCabecera, 9].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 9, finCabecera, 9].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 9, finCabecera, 9].Style.Font.Bold = true;
                                ws.Cells[finCabecera, 9, finCabecera, 9].Style.WrapText = true;

                                break;
                            case 7:
                                ws.Cells[finCabecera, 10].Value = "OBSERVACIONES";
                                ws.Cells[finCabecera, 10, finCabecera, 12].Merge = true;
                                ws.Cells[finCabecera, 10, finCabecera, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, 10, finCabecera, 12].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 10, finCabecera, 12].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 10, finCabecera, 12].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 10, finCabecera, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                // Assign borders
                                ws.Cells[finCabecera, 10, finCabecera, 12].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 10, finCabecera, 12].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 10, finCabecera, 12].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 10, finCabecera, 12].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 10, finCabecera, 12].Style.Font.Bold = true;
                                ws.Cells[finCabecera, 10, finCabecera, 12].Style.WrapText = true;

                                break;

                            default:
                                ws.Cells[finCabecera, col++].Value = "";
                                break;
                        }

                        ws.Row(finCabecera).Height = 20.25;
                    }

                    finCabecera++;

                    contador = 1;
                    foreach (var column in informacionCompleta.Detalles)
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

                                    ws.Cells[finCabecera, col].Value = contador;
                                    ws.Cells[finCabecera, 1, finCabecera, 1].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 1, finCabecera, 1].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 1, finCabecera, 1].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 1, finCabecera, 1].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    break;
                                case 2:

                                    ws.Cells[finCabecera, 2].Value = column.NombreEquipo;
                                    ws.Cells[finCabecera, 2, finCabecera, 3].Merge = true;
                                    ws.Cells[finCabecera, 2, finCabecera, 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 2, finCabecera, 3].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 2, finCabecera, 3].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 2, finCabecera, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 2, finCabecera, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[finCabecera, 2, finCabecera, 3].Style.WrapText = true;

                                    break;
                                case 3:
                                    ws.Cells[finCabecera, 4].Value = column.SerieEquipo;
                                    ws.Cells[finCabecera, 4, finCabecera, 5].Merge = true;
                                    ws.Cells[finCabecera, 4, finCabecera, 5].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 4, finCabecera, 5].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 4, finCabecera, 5].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 4, finCabecera, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 4, finCabecera, 5].Style.WrapText = true;



                                    break;
                                case 4:
                                    ws.Cells[finCabecera, 6].Value = column.TextoCatalogoProveedor;
                                    ws.Cells[finCabecera, 6, finCabecera, 7].Merge = true;
                                    ws.Cells[finCabecera, 6, finCabecera, 7].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 6, finCabecera, 7].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 6, finCabecera, 7].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 6, finCabecera, 7].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 6, finCabecera, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[finCabecera, 6, finCabecera, 7].Style.WrapText = true;

                                    break;
                                case 5:
                                    ws.Cells[finCabecera, 8].Value = column.Costo;
                                    ws.Cells[finCabecera, 8, finCabecera, 8].Merge = true;
                                    ws.Cells[finCabecera, 8, finCabecera, 8].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 8, finCabecera, 8].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 8, finCabecera, 8].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 8, finCabecera, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 8, finCabecera, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                    ws.Cells[finCabecera, 8, finCabecera, 8].Style.Numberformat.Format = "0.00";
                                    ws.Cells[finCabecera, 8, finCabecera, 8].Style.WrapText = true;

                                    break;
                                case 6:
                                    ws.Cells[finCabecera, 9].Value = column.TextoCatalogoEntregado;
                                    ws.Cells[finCabecera, 9, finCabecera, 9].Merge = true;
                                    ws.Cells[finCabecera, 9, finCabecera, 9].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 9, finCabecera, 9].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 9, finCabecera, 9].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 9, finCabecera, 9].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 9, finCabecera, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[finCabecera, 9, finCabecera, 9].Style.WrapText = true;

                                    break;
                                case 7:
                                    ws.Cells[finCabecera, 10].Value = column.Observaciones;
                                    ws.Cells[finCabecera, 10, finCabecera, 12].Merge = true;
                                    ws.Cells[finCabecera, 10, finCabecera, 12].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 10, finCabecera, 12].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 10, finCabecera, 12].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 10, finCabecera, 12].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 10, finCabecera, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[finCabecera, 10, finCabecera, 12].Style.WrapText = true;

                                    break;

                                default:
                                    ws.Cells[finCabecera, col++].Value = "";
                                    break;
                            }
                            ws.Row(finCabecera).Height = valorfiladetalles;

                        }


                        finCabecera++;
                        contador++;

                    }

                    ws.Row(finCabecera).Height = espaciofilas;
                    finCabecera += 1;
                }
                #endregion


                #region firmas

                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Merge = true;
                ws.Cells[finCabecera, 1].Value = "FIRMAS DE RESPONSABILIDAD:";


                ws.Cells[finCabecera, 1].Style.Fill.BackgroundColor.SetColor(colorGrisOscuroEstiloQPH);
                ws.Cells[finCabecera, 1].Style.Font.Color.SetColor(colorBlancoEstiloQPH);
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                ws.Row(finCabecera).Height = valorfilatitulos;
                finCabecera++;


                int finBorder = finCabecera + 9;
                if (informacionCompleta.Desvinculacion.TextoMotivo == "RENUNCIA")
                {
                    finBorder = finCabecera + 17;
                }

                ws.Cells[finCabecera, 1, finBorder, columnaFinalDocumentoExcel].Style.Border.BorderAround(ExcelBorderStyle.Hair);
                finCabecera += 5;

                ws.Row(finCabecera).Height = 20.25;

                if (informacionCompleta.Desvinculacion.TextoMotivo == "RENUNCIA")
                {

                    ws.Cells[finCabecera, 3, finCabecera, 5].Merge = true;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Value = informacionCompleta.Desvinculacion.UsuarioDesvinculado;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Bold = true;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Merge = true;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Value = "NOMBRES/APELLIDOS:";
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Bold = true;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    finCabecera++;

                    ws.Cells[finCabecera, 3, finCabecera, 5].Merge = true;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Value = informacionCompleta.Desvinculacion.TextoCatalogoCargo;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Bold = true;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;


                    ws.Cells[finCabecera, 8, finCabecera, 10].Merge = true;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Value = "CARGO";
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Bold = true;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    finCabecera++;

                    ws.Cells[finCabecera, 3, finCabecera, 5].Merge = true;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Value = "EX-COLABORADOR";
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Bold = true;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;


                    ws.Cells[finCabecera, 8, finCabecera, 10].Merge = true;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Value = "COMPRAS";
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Bold = true;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    finCabecera++;
                    finCabecera += 5;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Merge = true;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Value = "NOMBRES Y APELLIDOS";
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Bold = true;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Merge = true;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Value = "NOMBRES Y APELLIDOS:";
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Bold = true;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    finCabecera++;

                    ws.Cells[finCabecera, 3, finCabecera, 5].Merge = true;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Value = informacionCompleta.Desvinculacion.TextoCatalogoCargo;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Bold = true;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;


                    ws.Cells[finCabecera, 8, finCabecera, 10].Merge = true;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Value = "CARGO";
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Bold = true;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    finCabecera++;

                    ws.Cells[finCabecera, 3, finCabecera, 5].Merge = true;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Value = "TECNOLOGÍA";
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Bold = true;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;


                    ws.Cells[finCabecera, 8, finCabecera, 10].Merge = true;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Value = "RRHH";
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Bold = true;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                }

                if (informacionCompleta.Desvinculacion.TextoMotivo == "DESPIDO")
                {

                    ws.Cells[finCabecera, 3, finCabecera, 5].Merge = true;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Value = informacionCompleta.Desvinculacion.UsuarioDesvinculado;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Bold = true;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Merge = true;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Value = "NOMBRES Y APELLIDOS:";
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Bold = true;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    finCabecera++;

                    ws.Cells[finCabecera, 3, finCabecera, 5].Merge = true;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Value = informacionCompleta.Desvinculacion.TextoCatalogoCargo;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Bold = true;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;


                    ws.Cells[finCabecera, 8, finCabecera, 10].Merge = true;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Value = "CARGO";
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Bold = true;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    finCabecera++;

                    ws.Cells[finCabecera, 3, finCabecera, 5].Merge = true;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Value = "EX-COLABORADOR";
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Bold = true;
                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;


                    ws.Cells[finCabecera, 8, finCabecera, 10].Merge = true;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Value = "RRHH";
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.Font.Bold = true;
                    ws.Cells[finCabecera, 8, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;


                }

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
                ws.Column(11).Width = 16;
                ws.Column(12).Width = 13;

                //ancho de filas
                ws.Row(3).Height = 4.25;

                using (var range = ws.Cells[3, 1, finCabecera, columnaFinalDocumentoExcel])
                {
                    range.Style.Font.Size = 10;
                    range.Style.Font.Name = "Raleway";
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                string basePath = ConfigurationManager.AppSettings["RepositorioDocumentos"];
                string rutaArchivos = basePath + "\\ENTRADASALIDAPERSONAL_RRHH\\DESVINCULACIÓN-EQUIPO";

                string nombreficha = informacionCompleta.Desvinculacion.UsuarioDesvinculado;

                var anioActual = DateTime.Now.Year.ToString();
                var almacenFisicoTemporal = Auxiliares.CrearCarpetasDirectorio(rutaArchivos, new List<string>() { anioActual, nombreficha });

                string pathExcel = Path.Combine(almacenFisicoTemporal, "DESVINCULACIÓN-EQUIPO-" + informacionCompleta.Desvinculacion.UsuarioDesvinculado + ".xlsx");

                FileInfo fi = new FileInfo(pathExcel);
                package.SaveAs(fi);



                return Json(new { Resultado = new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa }, PathsArchivos = pathExcel }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message }, PathsArchivos = new List<string> { } }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

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

        public ActionResult GetUsuariosValidadosDesvinculacion(string query)
        {
            query = !string.IsNullOrEmpty(query) ? query.ToLower().Trim() : string.Empty;

            var data = DesvinculacionPersonalDAL.ObtenerListadoUsuariosValidadosDesvinculacion()
            //if "query" is null, get all records
            .Where(m => string.IsNullOrEmpty(query) || m.Text.ToLower().Contains(query))
            .OrderBy(m => m.Text);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #region REPORTES BASICOS
        public ActionResult DescargarReporteFormatoExcel()
        {
            var collection = DesvinculacionPersonalDAL.ListadoReporteBasico();
            var package = GetEXCEL(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(package.GetAsByteArray(), XlsxContentType, "Listado.xlsx");
        }

        public ActionResult DescargarReporteFormatoPDF()
        {
            // Seleccionar las columnas a exportar
            var collection = DesvinculacionPersonalDAL.ListadoReporteBasico();
            byte[] buffer = GetPDF(columnasReportesBasicos, collection.Cast<object>().ToList(), "Listado de Requerimientos");

            return File(buffer, PDFContentType, "ReportePDF.pdf");
        }

        public ActionResult DescargarReporteFormatoCSV()
        {
            var collection = DesvinculacionPersonalDAL.ListadoReporteBasico();
            byte[] buffer = GetCSV(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(buffer, CSVContentType, $"Listado.csv");
        }
        #endregion
    }
}