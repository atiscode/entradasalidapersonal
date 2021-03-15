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
using OfficeOpenXml;
using OfficeOpenXml.Style;
using EntradaSalidaRRHH.UI.Helper;
using System.IO;
using System.Configuration;
using System.Drawing;
using OfficeOpenXml.Drawing;


namespace EntradaSalidaRRHH.UI.Controllers
{
    [Autenticado]
    public class AprobacionRequerimientoEquipoController : BaseController
    {
        // GET: AprobacionRequerimientoEquipo
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<PartialViewResult> _IndexGrid(string search, string sort = "", string order = "", long? page = 1)
        {
            //Permisos
            Permisos(ControllerContext.RouteData.Values["controller"].ToString());

            var listado = new List<AprobacionRequerimientoEquipoInfo>();
            ViewBag.NombreListado = Etiquetas.TituloGridAprobacionEquipo;
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
                        listado = AprobacionRequerimientoEquipoDAL.ListadoAprobacionRequerimientoEquipo(page.Value).OrderBy(sort + " " + order).ToList();
                    else
                        listado = AprobacionRequerimientoEquipoDAL.ListadoAprobacionRequerimientoEquipo(page.Value).ToList();
                }

                search = !string.IsNullOrEmpty(search) ? search.Trim() : "";

                if (!string.IsNullOrEmpty(search))//filter
                {
                    listado = AprobacionRequerimientoEquipoDAL.ListadoAprobacionRequerimientoEquipo(null, search);
                }

                if (!string.IsNullOrEmpty(whereClause) && string.IsNullOrEmpty(search))
                {
                    if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
                        listado = AprobacionRequerimientoEquipoDAL.ListadoAprobacionRequerimientoEquipo(null, null, whereClause).OrderBy(sort + " " + order).ToList();
                    else
                        listado = AprobacionRequerimientoEquipoDAL.ListadoAprobacionRequerimientoEquipo(null, null, whereClause);
                }
                else
                {

                    if (string.IsNullOrEmpty(search))
                        totalPaginas = AprobacionRequerimientoEquipoDAL.ObtenerTotalRegistrosListadoAprobacionRequerimientoEquipo();
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

        public ActionResult _AprobarRequerimiento(int id)
        {
            ViewBag.TituloModal = "Aprobación de Requerimiento";
            try
            {
                var model = ComentariosRequerimientoEquipoDAL.ConsultarComentariosRequerimientoEquipoByRequerimientoID(id); //RequerimientoEquipoDAL.ConsultarRequerimientoEquipoCompleto(id);
                ViewBag.RequerimientoEquipoID = id;
                return PartialView(model);
            }
            catch (Exception ex)
            {
                ViewBag.RequerimientoEquipoID = id;
                return PartialView(new List<ComentariosRequerimientoEquipoInfo>());
            }
        }

        public ActionResult _ComparativoEquipos(int id, string equipos, string equiposSugeridos)
        {
            ViewBag.TituloModal = "Comparativo de Equipos";
            try
            {
                ViewBag.RequerimientoEquipoID = id;

                List<EquiposInfo> listadoEquipos = new List<EquiposInfo>();
                List<EquiposInfo> listadoEquiposSugeridos = new List<EquiposInfo>();

                var idsEquipos = !string.IsNullOrEmpty(equipos) ? equipos.Split(',').Select(int.Parse).ToList() : new List<int>();
                var idsEquiposSugeridos = !string.IsNullOrEmpty(equiposSugeridos) ? equiposSugeridos.Split(',').Select(int.Parse).ToList() : new List<int>();

                foreach (var item in idsEquipos)
                {
                    listadoEquipos.Add(EquipoDAL.ListadoEquipo(null, null, null, item).FirstOrDefault());
                }

                foreach (var item in idsEquiposSugeridos)
                {
                    listadoEquiposSugeridos.Add(EquipoDAL.ListadoEquipo(null, null, null, item).FirstOrDefault());
                }

                ViewBag.listadoEquipos = listadoEquipos;
                ViewBag.listadoEquiposSugeridos = listadoEquiposSugeridos;

                return PartialView();
            }
            catch (Exception ex)
            {
                ViewBag.RequerimientoEquipoID = id;
                ViewBag.listadoEquipos = new List<EquiposInfo>();
                ViewBag.listadoEquiposSugeridos = new List<EquiposInfo>();
                return PartialView();
            }
        }

        [HttpPost]
        public ActionResult AprobarRequerimiento(int id)
        {
            RespuestaTransaccion resultado = RequerimientoEquipoDAL.AprobarRequerimientoEquipo(id);
            var requerimiento = RequerimientoEquipoDAL.ConsultarRequerimientoEquipo(id);
            var usuario = UsuarioDAL.ConsultarUsuario(requerimiento.UsuarioID);
            var destinatarios = PerfilesDAL.ConsultarCorreoNotificacion(13);
            string enlace = GetUrlSitio(Url.Action("Index", "Login"));

            string body = GetEmailTemplate("TemplateAprobacionEquipo");
            body = body.Replace("@ViewBag.EnlaceDirecto", enlace);
            body = body.Replace("@ViewBag.EnlaceSecundario", enlace);
            body = body.Replace("@ViewBag.Empleado", usuario.Nombres + " " + usuario.Apellidos);

            if (resultado.Estado)
            {
                var notificacion = NotificacionesDAL.CrearNotificacion(new Notificaciones
                {
                    NombreTarea = "Aprobación Requerimiento de Equipo",
                    DescripcionTarea = "Correo de notificación de aprobacion de un requerimiento de equipo.",
                    NombreEmisor = nombreCorreoEmisor,
                    CorreoEmisor = correoEmisor,
                    ClaveCorreo = claveEmisor,
                    CorreosDestinarios = destinatarios,
                    AsuntoCorreo = "NOTIFICACION DE APROBACION DE REQUERIMIENTO",
                    NombreArchivoPlantillaCorreo = TemplateNotificaciones,
                    CuerpoCorreo = body,
                    AdjuntosCorreo = "",//ruta,
                    FechaEnvioCorreo = DateTime.Now,
                    Empresa = usuario.NombreEmpresa,
                    Canal = CanalNotificaciones,
                    Tipo = "APROBACION",
                });
            }
            return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GuardarComentario(ComentariosRequerimientoEquipo formulario)
        {
            formulario.Fecha = DateTime.Now;
            formulario.UsuarioID = UsuarioLogeadoSession.IdUsuario;

            RespuestaTransaccion resultado = ComentariosRequerimientoEquipoDAL.CrearComentariosRequerimientoEquipo(formulario);

            var usuario = UsuarioDAL.ConsultarUsuario(formulario.UsuarioID);
            var destinatarios = PerfilesDAL.ConsultarCorreoNotificacion(14);
            string enlace = GetUrlSitio(Url.Action("Index", "Login"));

            string body = GetEmailTemplate("TemplateComentarioAprobacion");
            body = body.Replace("@ViewBag.EnlaceDirecto", enlace);
            body = body.Replace("@ViewBag.EnlaceSecundario", enlace);
            body = body.Replace("@ViewBag.Empleado", usuario.NombresApellidos);
            body = body.Replace("@ViewBag.Comentario", formulario.Comentario);

            if (resultado.Estado)
            {
                var notificacion = NotificacionesDAL.CrearNotificacion(new Notificaciones
                {
                    NombreTarea = "Nuevo Comentario",
                    DescripcionTarea = "Correo de notificación de nuevo comentario para aprobación de requerimiento.",
                    NombreEmisor = nombreCorreoEmisor,
                    CorreoEmisor = correoEmisor,
                    ClaveCorreo = claveEmisor,
                    CorreosDestinarios = destinatarios,
                    AsuntoCorreo = "NUEVO COMENTARIO",
                    NombreArchivoPlantillaCorreo = TemplateNotificaciones,
                    CuerpoCorreo = body,
                    AdjuntosCorreo = "",//ruta,
                    FechaEnvioCorreo = DateTime.Now,
                    Empresa = usuario.NombreEmpresa,
                    Canal = CanalNotificaciones,
                    Tipo = "COMENTARIO",
                });
            }
            return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
        }


        #region REPORTE EXCEL REQUIRIMIENTO EQUIPO
        [HttpPost]
        public ActionResult GenerarReporte(int id)
        {
            try
            {
                var package = new ExcelPackage();

                var informacionCompleta = RequerimientoEquipoDAL.ConsultarRequerimientoEquipoCompleto(id);


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

                var ws = package.Workbook.Worksheets.Add(informacionCompleta.RequerimientoEquipo.NombresApellidos.ToString());

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
                using (var range = ws.Cells[1, 4, 2, 10])
                {

                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Merge = true;
                    range.Value = "REQUERIMIENTO DE EQUIPO";
                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);

                    range.Style.Font.Name = "Raleway";
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    range.Style.Font.Bold = true;
                    range.Style.Font.Size = 18;

                }

                #endregion


                #region Datos AREA SOLICITANTE

                ws.Cells[cabeceraparte1, 1, cabeceraparte1, columnaFinalDocumentoExcel].Merge = true;
                ws.Cells[cabeceraparte1, 1, cabeceraparte1, columnaFinalDocumentoExcel].Value = "DATOS DEL ÁREA SOLICITANTE";
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
                    range.Value = "FECHA DE SOLICITUD: ";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 3].Merge = true;
                    ws.Cells[range.Start.Row, range.Columns + 1].Value = informacionCompleta.RequerimientoEquipo.FechaSolicitud != null ? informacionCompleta.RequerimientoEquipo.FechaSolicitud.ToString("dd/MM/yyyy") : string.Empty; // Valor campo
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
                    range.Value = "FECHA DE INGRESO";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;


                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;


                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.RequerimientoEquipo.FechaIngreso != null ? informacionCompleta.RequerimientoEquipo.FechaIngreso.Value.ToString("dd/MM/yyyy") : string.Empty; ; // Valor campo
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
                    range.Value = "NOMBRE: ";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 3].Merge = true;
                    ws.Cells[range.Start.Row, range.Columns + 1].Value = informacionCompleta.RequerimientoEquipo.NombresApellidos;  // Valor campo
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
                    range.Value = "CÉDULA DE IDENTIDAD";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;


                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.RequerimientoEquipo.Identificacion; // Valor campo
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

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.RequerimientoEquipo.TextoCatalogoCargo; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                }

                using (var range = ws.Cells[cabeceraparte1, 6, cabeceraparte1, 7])
                {

                    range.Merge = true;
                    range.Value = "EMPRESA: ";
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
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.RequerimientoEquipo.NombreEmpresa;  // Valor campo
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
                    range.Value = "ÁREA:";
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
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.RequerimientoEquipo.TextoCatalogoArea; // Valor campo
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
                    range.Value = "JEFE DIRECTO:";
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
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.RequerimientoEquipo.TextoCatalogoJefeDirecto; // Valor campo
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

                #region MOTIVOINGRESO
                ws.Cells[cabeceraparte2, 1, cabeceraparte2, columnaFinalDocumentoExcel].Merge = true;
                ws.Cells[cabeceraparte2, 1, cabeceraparte2, columnaFinalDocumentoExcel].Value = "";
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
                    range.Value = "MOTIVO:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.RequerimientoEquipo.TextoCatalogoTipoIngreso; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(cabeceraparte1).Height = valorfiladetalles;
                }


                cabeceraparte2++;

                using (var range = ws.Cells[cabeceraparte2, 1, cabeceraparte2, 2])
                {

                    range.Merge = true;
                    range.Value = "A QUIÉN REEMPLAZA:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.RequerimientoEquipo.PersonaReemplazante; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                }

                using (var range = ws.Cells[cabeceraparte2, 6, cabeceraparte2, 7])
                {

                    range.Merge = true;
                    range.Value = "TIPO DE CONTRATO:";
                    range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    range.Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                    range.Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                    range.Style.Font.Bold = true;

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.RequerimientoEquipo.TextoCatalogoTipoContrato;// Valor campo
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



                Int32 col = 1;
                int contador = 1;
                #region DetalleEquipoUsuario

                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Merge = true;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Value = "REQUERIMIENTOS DE EQUIPO";
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

                if (informacionCompleta.EquipoUsuario.Any())
                {
                    col = 1;
                    int totalColumnasDetalleEstudios = 4;

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
                                ws.Cells[finCabecera, col].Value = "EQUIPO";
                                ws.Cells[finCabecera, col, finCabecera, 2].Merge = true;
                                ws.Cells[finCabecera, col, finCabecera, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, col, finCabecera, 2].Style.Fill.BackgroundColor.SetColor(colorGrisClaro2EstiloQPH);
                                ws.Cells[finCabecera, col, finCabecera, 2].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, col, finCabecera, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, col, finCabecera, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                //Assign borders
                                ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, col, finCabecera, 2].Style.Font.Bold = true;
                                break;

                            case 2:
                                ws.Cells[finCabecera, 3].Value = "CARACTERÍSTICAS";
                                ws.Cells[finCabecera, 3, finCabecera, 5].Merge = true;

                                ws.Cells[finCabecera, 3, finCabecera, 5].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 3, finCabecera, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 3, finCabecera, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                //Assign borders
                                ws.Cells[finCabecera, 3, finCabecera, 5].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 3, finCabecera, 5].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 3, finCabecera, 5].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 3, finCabecera, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 3, finCabecera, 5].Style.Font.Bold = true;
                                break;
                            case 3:
                                ws.Cells[finCabecera, 6].Value = "DESCRIPCIÓN";
                                ws.Cells[finCabecera, 6, finCabecera, 8].Merge = true;
                                ws.Cells[finCabecera, 6, finCabecera, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, 6, finCabecera, 8].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 6, finCabecera, 8].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 6, finCabecera, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 6, finCabecera, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                // Assign borders
                                ws.Cells[finCabecera, 6, finCabecera, 8].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 6, finCabecera, 8].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 6, finCabecera, 8].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 6, finCabecera, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 6, finCabecera, 8].Style.Font.Bold = true;

                                break;

                            case 4:
                                ws.Cells[finCabecera, 9].Value = "OBSERVACIONES";
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
                    foreach (var column in informacionCompleta.EquipoUsuario)
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
                                    ws.Cells[finCabecera, col].Value = column.NombreEquipo;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Merge = true;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.WrapText = true;
                                    break;
                                case 2:
                                    ws.Cells[finCabecera, 3].Value = column.Caracteristicas;
                                    ws.Cells[finCabecera, 3, finCabecera, 5].Merge = true;
                                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 3, finCabecera, 5].Style.WrapText = true;
                                    var caracteres = column.Caracteristicas.Count();


                                    break;
                                case 3:
                                    ws.Cells[finCabecera, 6].Value = column.DescripcionEquipo;
                                    ws.Cells[finCabecera, 6, finCabecera, 8].Merge = true;
                                    ws.Cells[finCabecera, 6, finCabecera, 8].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 6, finCabecera, 8].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 6, finCabecera, 8].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 6, finCabecera, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 6, finCabecera, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[finCabecera, 3, finCabecera, 8].Style.WrapText = true;
                                    var caracteresD = column.DescripcionEquipo.Count();

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
                                case 4:
                                    ws.Cells[finCabecera, 9].Value = column.Observaciones;
                                    ws.Cells[finCabecera, 9, finCabecera, 10].Merge = true;
                                    ws.Cells[finCabecera, 9, finCabecera, 10].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 9, finCabecera, 10].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 9, finCabecera, 10].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 9, finCabecera, 10].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 9, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                    break;

                                default:
                                    ws.Cells[finCabecera, col++].Value = "";
                                    break;
                            }


                        }


                        finCabecera++;
                        contador++;

                    }

                    finCabecera += 1;
                }

                using (var range = ws.Cells[finCabecera, 1, finCabecera, 2])
                {

                    range.Merge = true;
                    range.Value = "CREDENCIAL ACCESO QPH: ";
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
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.RequerimientoEquipo.CredencialAcceso;
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 3].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                }
                using (var range = ws.Cells[finCabecera, 6, finCabecera, 7])
                {

                    range.Merge = true;
                    range.Value = "GRUPO CORREO: ";
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
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.RequerimientoEquipo.TextoCatalogoGrupoCorreo.ToString().ToLower();
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[range.Start.Row, range.Columns + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, columnaFinalDocumentoExcel].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    ws.Row(finCabecera).Height = valorfiladetalles;
                }

                ws.Row(finCabecera).Height = valorfilatitulos;
                finCabecera++;
                ws.Row(finCabecera).Height = espaciofilas;
                finCabecera += 1;

                #endregion


                #region Detalle Herramientas Adicionales

                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Merge = true;
                ws.Cells[finCabecera, 1, finCabecera, columnaFinalDocumentoExcel].Value = "HERRAMIENTAS ADICIONALES";
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

                if (informacionCompleta.HerramientasAdicionales.Any())
                {
                    col = 1;
                    int totalColumnasDetalleExperiencia = 2;

                    for (int i = 1; i <= totalColumnasDetalleExperiencia; i++)
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
                                ws.Cells[finCabecera, col].Value = "N.";
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
                                ws.Cells[finCabecera, 3].Value = "  NOMBRE HERRAMIENTA";
                                ws.Cells[finCabecera, 3, finCabecera, 10].Merge = true;
                                ws.Cells[finCabecera, 3, finCabecera, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, 3, finCabecera, 10].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 3, finCabecera, 10].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 3, finCabecera, 10].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 3, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                // Assign borders
                                ws.Cells[finCabecera, 3, finCabecera, 10].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 3, finCabecera, 10].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 3, finCabecera, 10].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 3, finCabecera, 10].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 3, finCabecera, 10].Style.Font.Bold = true;
                                break;

                            default:
                                ws.Cells[finCabecera, col++].Value = "";
                                break;
                        }

                        ws.Row(finCabecera).Height = 20.25;
                    }

                    finCabecera++;

                    contador = 1;
                    foreach (var column in informacionCompleta.HerramientasAdicionales)
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
                                    ws.Cells[finCabecera, col].Value = contador;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Merge = true;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.WrapText = true;
                                    break;
                                case 2:
                                    ws.Cells[finCabecera, 3].Value = column.TextoCatalogoHerramientaAdicional;
                                    ws.Cells[finCabecera, 3, finCabecera, 10].Merge = true;
                                    ws.Cells[finCabecera, 3, finCabecera, 10].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 3, finCabecera, 10].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 3, finCabecera, 10].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 3, finCabecera, 10].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 3, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                    ws.Cells[finCabecera, 3, finCabecera, 10].Style.WrapText = true;
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
                ws.Cells[finCabecera, 1].Value = "F I R M A S";

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

                ws.Cells[finCabecera, 1, finBorder, columnaFinalDocumentoExcel].Style.Border.BorderAround(ExcelBorderStyle.Hair);
                finCabecera += 6;

                ws.Row(finCabecera).Height = 20.25;

                ws.Cells[finCabecera, 2, finCabecera, 4].Merge = true;
                ws.Cells[finCabecera, 2, finCabecera, 4].Value = "SOLICITANTE:";
                ws.Cells[finCabecera, 2, finCabecera, 4].Style.Font.Bold = true;
                ws.Cells[finCabecera, 2, finCabecera, 4].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                ws.Cells[finCabecera, 2, finCabecera, 4].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[finCabecera, 2, finCabecera, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells[finCabecera, 7, finCabecera, 9].Merge = true;
                ws.Cells[finCabecera, 7, finCabecera, 9].Value = "AUTORIZA:";
                ws.Cells[finCabecera, 7, finCabecera, 9].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                ws.Cells[finCabecera, 7, finCabecera, 9].Style.Font.Bold = true;
                ws.Cells[finCabecera, 7, finCabecera, 9].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[finCabecera, 7, finCabecera, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                finCabecera++;

                ws.Cells[finCabecera, 2, finCabecera, 4].Merge = true;
                ws.Cells[finCabecera, 2, finCabecera, 4].Value = "CARGO:";
                ws.Cells[finCabecera, 2, finCabecera, 4].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                ws.Cells[finCabecera, 2, finCabecera, 4].Style.Font.Bold = true;
                ws.Cells[finCabecera, 2, finCabecera, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;


                ws.Cells[finCabecera, 7, finCabecera, 9].Merge = true;
                ws.Cells[finCabecera, 7, finCabecera, 9].Value = "CARGO:";
                ws.Cells[finCabecera, 7, finCabecera, 9].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                ws.Cells[finCabecera, 7, finCabecera, 9].Style.Font.Bold = true;
                ws.Cells[finCabecera, 7, finCabecera, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;


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

                using (var range = ws.Cells[3, 1, finCabecera, columnaFinalDocumentoExcel])
                {
                    range.Style.Font.Size = 10;
                    range.Style.Font.Name = "Raleway";
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                string basePath = ConfigurationManager.AppSettings["RepositorioDocumentos"];
                string rutaArchivos = basePath + "\\ENTRADA_SALIDAPERSONAL_RRHH\\REQUERIMIENTO_EQUIPO";

                string nombreficha = informacionCompleta.RequerimientoEquipo.NombresApellidos;

                var anioActual = DateTime.Now.Year.ToString();
                var almacenFisicoTemporal = Auxiliares.CrearCarpetasDirectorio(rutaArchivos, new List<string>() { anioActual, nombreficha });
                var almacenFisicoOfficeToPDF = Auxiliares.CrearCarpetasDirectorio(Server.MapPath("~/OfficeToPDF/"), new List<string>());

                string pathExcel = Path.Combine(almacenFisicoTemporal, "REQUERIMIENTO-EQUIPO-" + informacionCompleta.RequerimientoEquipo.NombresApellidos + ".xlsx");

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
    }
}