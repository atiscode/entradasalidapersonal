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
using System.Drawing;
using OfficeOpenXml.Drawing;

namespace EntradaSalidaRRHH.UI.Controllers
{
    [Autenticado]
    public class CodificacionEquipoController : BaseController
    {
        private List<string> columnasReportesBasicos = new List<string> { "FECHA DE CODIFICACION", "USUARIO RESPONSABLE", "FECHA SOLICITUD", "USUARIO SOLICITANTE", "EQUIPOS", "HERRAMIENTAS ADICIONALES", "FECHA ASIGNACION", "CREDENCIAL ACCESO" };

        // GET: CodificacionEquipo
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<PartialViewResult> _IndexGrid(string search, string sort = "", string order = "", long? page = 1)
        {
            //Permisos
            Permisos(ControllerContext.RouteData.Values["controller"].ToString());

            var listado = new List<CodificacionEquipoInfo>();
            ViewBag.NombreListado = Etiquetas.TituloGridCodificacionEquipos;
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
                        listado = CodificacionEquipoDAL.ListadoCodificacionEquipo(page.Value).OrderBy(sort + " " + order).ToList();
                    else
                        listado = CodificacionEquipoDAL.ListadoCodificacionEquipo(page.Value).ToList();
                }

                search = !string.IsNullOrEmpty(search) ? search.Trim() : "";

                if (!string.IsNullOrEmpty(search))//filter
                {
                    listado = CodificacionEquipoDAL.ListadoCodificacionEquipo(null, search);
                }

                if (!string.IsNullOrEmpty(whereClause) && string.IsNullOrEmpty(search))
                {
                    if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
                        listado = CodificacionEquipoDAL.ListadoCodificacionEquipo(null, null, whereClause).OrderBy(sort + " " + order).ToList();
                    else
                        listado = CodificacionEquipoDAL.ListadoCodificacionEquipo(null, null, whereClause);
                }
                else
                {

                    if (string.IsNullOrEmpty(search))
                        totalPaginas = CodificacionEquipoDAL.ObtenerTotalRegistrosListadoCodificacionEquipo();
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

        public ActionResult Formulario(int? id, int requerimientoID)
        {
            ViewBag.TituloPanel = Etiquetas.TituloPanelFormularioCodificacionEquipos;

            int UsuarioID = 0;
            List<int> equipos = new List<int>();
            List<int> herrmientasAccesorios = new List<int>();

            try
            {
                CodificacionEquipoCompleta model = new CodificacionEquipoCompleta();
                model.Codificacion.FechaCodificacionEquipo = DateTime.Now;

                var requerimiento = RequerimientoEquipoDAL.ListadoRequerimientoEquipo(null, null, null, requerimientoID).FirstOrDefault();

                if (requerimiento != null)
                {
                    if (!string.IsNullOrWhiteSpace(requerimiento.IDsEquipos))
                    {
                        equipos = requerimiento.IDsEquipos.Split(',').Select(s => int.Parse(s)).ToList();
                    }
                    if (!string.IsNullOrWhiteSpace(requerimiento.IDsHerramientasAdicionales))
                    {
                        herrmientasAccesorios = requerimiento.IDsHerramientasAdicionales.Split(',').Select(s => int.Parse(s)).ToList();
                    }

                    UsuarioID = requerimiento.UsuarioID;
                }

                if (id.HasValue)
                    model = CodificacionEquipoDAL.ConsultarCodificacionEquipoCompleta(id);
                else
                {
                    foreach (var item in equipos)
                    {
                        model.Detalles.Add(new DetalleCodificacionEquipoInfo
                        {
                            EquipoID = item,
                            FechaCompra = DateTime.Now,
                            Observaciones = "N/A",
                        });
                    }

                    foreach (var item in herrmientasAccesorios)
                    {
                        model.Detalles.Add(new DetalleCodificacionEquipoInfo
                        {
                            EquipoID = item,
                            FechaCompra = DateTime.Now,
                            Observaciones = "N/A",
                        });
                    }
                }

                ViewBag.EquiposRequeridos = equipos.Select(s => s.ToString()).ToList();

                ViewBag.requerimientoID = requerimientoID;
                ViewBag.UsuarioID = UsuarioID;

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.EquiposRequeridos = new List<string>();
                return View(new CodificacionEquipoCompleta());
            }
        }

        public ActionResult _InformacionEquipo(int id)
        {
            ViewBag.TituloModal = "Información del Equipo";

            EquiposInfo model = EquipoDAL.ListadoEquipo(null, null, null, id).FirstOrDefault() ?? new EquiposInfo();

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult CreateCodificacion(CodificacionEquipo formulario, List<DetalleCodificacionEquipo> detalles, List<string> archivos)
        {
            try
            {
                detalles = detalles ?? new List<DetalleCodificacionEquipo>();
                archivos = archivos ?? new List<string>();

                var seriesEquiposDuplicadas = detalles.GroupBy(x => x.SerieEquipo).Any(g => g.Count() > 1);
                if (seriesEquiposDuplicadas)
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeValidacionSerieEquipoUnica } }, JsonRequestBehavior.AllowGet);

                //Validar que todos los archivos adjuntos sean requeridos
                int totalDetalles = detalles.Count;
                int totalArchivos = archivos.Count;

                if(totalDetalles != totalArchivos)
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeErrorAdjuntosRequeridos } }, JsonRequestBehavior.AllowGet);

                #region Guardar archivos adjuntos
                string rutaBase = basePathRepositorioDocumentos + "\\ENTRADA_SALIDAPERSONAL_RRHH\\FacturasAdjuntosCodificaciones";     
                
                int contador = 0;

                foreach (var item in detalles)
                {
                    string adjuntoDetalle = archivos.ElementAt(contador);
                    string pathFinal = Path.Combine(rutaBase, item.SerieEquipo + ".pdf");

                    System.IO.File.Move(adjuntoDetalle, pathFinal);

                    //Solo si el archivo se logra decodificar correctamente
                    item.Adjunto = pathFinal;                   

                    contador++;
                }
                #endregion

                formulario.UsuarioCodificadorID = UsuarioLogeadoSession.IdUsuario;

                Resultado = CodificacionEquipoDAL.CrearCodificacionEquipo(formulario, detalles);

                var requerimiento = RequerimientoEquipoDAL.ConsultarRequerimientoEquipo(formulario.RequerimientoEquipoID);
                var usuario = UsuarioDAL.ConsultarUsuario(requerimiento.UsuarioID);
                var destinatarios = PerfilesDAL.ConsultarCorreoNotificacion(13);
                string enlace = GetUrlSitio(Url.Action("Index", "Login"));

                string body = GetEmailTemplate("TemplateCodificacionEquipo");
                body = body.Replace("@ViewBag.EnlaceDirecto", enlace);
                body = body.Replace("@ViewBag.EnlaceSecundario", enlace);
                body = body.Replace("@ViewBag.Empleado", usuario.NombresApellidos);

                //Siempre que la ficha de ingreso haya sido creado con éxito.
                if (Resultado.Estado)
                {


                    var notificacion = NotificacionesDAL.CrearNotificacion(new Notificaciones
                    {
                        NombreTarea = "Codificación de Equipo",
                        DescripcionTarea = "Correo de notificación de codificación de equipos.",
                        NombreEmisor = nombreCorreoEmisor,
                        CorreoEmisor = correoEmisor,
                        ClaveCorreo = claveEmisor,
                        CorreosDestinarios = destinatarios,
                        AsuntoCorreo = "NOTIFICACIÓN DE CODFICACIONES DE EQUIPOS",
                        NombreArchivoPlantillaCorreo = TemplateNotificaciones,
                        CuerpoCorreo = body,
                        AdjuntosCorreo = "",//ruta,
                        FechaEnvioCorreo = DateTime.Now,
                        Empresa = usuario.NombreEmpresa,
                        Canal = CanalNotificaciones,
                        Tipo = "CODIFICACIÓN",
                    });
                }

                return Json(new { Resultado = Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult _Cargar ()
        {
            var resultado = new RespuestaTransaccion { Estado = true };

            HttpPostedFileBase file = null;

            for (int i = 0; i < Request.Files.Count; i++)
            {
                file = Request.Files[i];
            }

            if (file != null && file.ContentLength > 0)
                try
                {
                    string rutaBase = basePathRepositorioDocumentos + "\\ENTRADA_SALIDAPERSONAL_RRHH\\FacturasAdjuntosCodificaciones";

                    if (!Directory.Exists(rutaBase))
                    {
                        Directory.CreateDirectory(rutaBase);
                    }                    

                    string ruta = Path.Combine(rutaBase, Path.GetFileName(file.FileName));

                    if (!System.IO.File.Exists(ruta))
                    {
                        file.SaveAs(ruta);
                        resultado.Respuesta = ruta;
                    }
                    else
                    {
                        resultado.Estado = false;
                        resultado.Respuesta = "Ya existe un archivo con el nombre seleccionado, escoja otro archivo o cambie el nombre del nuevo.";
                    }
                }
                catch (Exception ex)
                {
                    resultado.Estado = false;
                    resultado.Respuesta = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                resultado.Estado = false;
                resultado.Respuesta = "ERROR.";
            }

            return Json(new { resultado }, JsonRequestBehavior.AllowGet);
        }

            [HttpPost]
        public ActionResult Edit(CodificacionEquipo formulario, List<DetalleCodificacionEquipo> detalles, List<string> archivos)
        {
            try
            {
                #region Guardar archivos adjuntos
                string rutaBase = basePathRepositorioDocumentos + "\\RRHH\\FacturasAdjuntosCodificaciones";
                bool existeRutaDisco = Directory.Exists(rutaBase); // VERIFICAR SI ESA RUTA EXISTE

                if (!existeRutaDisco)
                    Directory.CreateDirectory(Server.MapPath(rutaBase));

                int contador = 0;
                foreach (var item in detalles)
                {
                    string adjuntoDetalle = archivos.ElementAt(contador);
                    if (!string.IsNullOrEmpty(adjuntoDetalle))
                    {
                        string pathFinal = Path.Combine(rutaBase, item.SerieEquipo + ".pdf");

                        bool ok = Auxiliares.Base64Decode(adjuntoDetalle, pathFinal);

                        //Solo si el archivo se logra decodificar correctamente
                        if (ok)
                            item.Adjunto = pathFinal;
                        else //Si existen errores al adjuntar el archivo
                            return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = string.Format(Mensajes.MensajeAdjuntoEspecificoFallido, item.SerieEquipo) } }, JsonRequestBehavior.AllowGet);
                    }
                    contador++;
                }
                #endregion

                detalles = detalles ?? new List<DetalleCodificacionEquipo>();

                formulario.UsuarioCodificadorID = UsuarioLogeadoSession.IdUsuario;

                Resultado = CodificacionEquipoDAL.ActualizarCodificacionEquipo(formulario, detalles);

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

            RespuestaTransaccion resultado = CodificacionEquipoDAL.EliminarCodificacionEquipo(id);
            return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
        }

        #region REPORTE EXCEL CODIFICACIÓN EQUIPO
        [HttpPost]
        public ActionResult GenerarReporte(int id)
        {
            try
            {
                var package = new ExcelPackage();

                var informacionCompleta = CodificacionEquipoDAL.ConsultarCodificacionEquipoCompleta(id);


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

                var ws = package.Workbook.Worksheets.Add(informacionCompleta.Codificacion.NombresApellidos.ToString());

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

                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Merge = true;
                    range.Value = "CODIFICACIÓN DE EQUIPO";
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

                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 4].Merge = true;
                    ws.Cells[range.Start.Row, range.Columns + 1].Value = informacionCompleta.Codificacion.FechaSolicitud != null ? informacionCompleta.Codificacion.FechaSolicitud.Value.ToString("dd/MM/yyyy") : string.Empty;  // Valor campo
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
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.Codificacion.FechaIngreso != null ? informacionCompleta.Codificacion.FechaIngreso.Value.ToString("dd/MM/yyyy") : string.Empty;// Valor campo
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

                    ws.Cells[range.Start.Row, range.Columns + 1, range.End.Row, range.Columns + 4].Merge = true;
                    ws.Cells[range.Start.Row, range.Columns + 1].Value = informacionCompleta.Codificacion.NombresApellidos;  // Valor campo
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
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.Codificacion.Identificacion; // Valor campo
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
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.Codificacion.TextoCatalogoCargo; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                }

                using (var range = ws.Cells[cabeceraparte1, 7, cabeceraparte1, 8])
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
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.Codificacion.NombreEmpresa;  // Valor campo
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

                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Merge = true;
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.Codificacion.TextoCatalogoArea; // Valor campo
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
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.Codificacion.TextoCatalogoJefeDirecto; // Valor campo
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
                    range.Value = "MAIL CORPORATIVO:";
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
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.Codificacion.MailCorporativo; // Valor campo
                    ws.Cells[range.Start.Row, range.End.Column + 1].Style.Indent = 1;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                    ws.Cells[range.Start.Row, range.End.Column + 1, range.End.Row, range.Columns + 4].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

                }

                using (var range = ws.Cells[cabeceraparte1, 7, cabeceraparte1, 8])
                {

                    range.Merge = true;
                    range.Value = "GRUPO CORREO:";
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
                    ws.Cells[range.Start.Row, range.End.Column + 1].Value = informacionCompleta.Codificacion.TextoCatalogoGrupoCorreo.ToString().ToLower(); // Valor campo
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
                                ws.Cells[finCabecera, 3].Value = "CÓDIGO";
                                ws.Cells[finCabecera, 3, finCabecera, 4].Merge = true;

                                ws.Cells[finCabecera, 3, finCabecera, 4].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 3, finCabecera, 4].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 3, finCabecera, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 3, finCabecera, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                //Assign borders
                                ws.Cells[finCabecera, 3, finCabecera, 4].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 3, finCabecera, 4].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 3, finCabecera, 4].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 3, finCabecera, 4].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 3, finCabecera, 4].Style.Font.Bold = true;
                                break;
                            case 3:
                                ws.Cells[finCabecera, 5].Value = "FACTURA";
                                ws.Cells[finCabecera, 5, finCabecera, 6].Merge = true;
                                ws.Cells[finCabecera, 5, finCabecera, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, 5, finCabecera, 6].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 5, finCabecera, 6].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 5, finCabecera, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 5, finCabecera, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                // Assign borders
                                ws.Cells[finCabecera, 5, finCabecera, 6].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 5, finCabecera, 6].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 5, finCabecera, 6].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 5, finCabecera, 6].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 5, finCabecera, 6].Style.Font.Bold = true;

                                break;

                            case 4:
                                ws.Cells[finCabecera, 7].Value = "PROVEEDOR";
                                ws.Cells[finCabecera, 7, finCabecera, 8].Merge = true;
                                ws.Cells[finCabecera, 7, finCabecera, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, 7, finCabecera, 8].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 7, finCabecera, 8].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 7, finCabecera, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 7, finCabecera, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                // Assign borders
                                ws.Cells[finCabecera, 7, finCabecera, 8].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 7, finCabecera, 8].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 7, finCabecera, 8].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 7, finCabecera, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 7, finCabecera, 8].Style.Font.Bold = true;
                                ws.Cells[finCabecera, 7, finCabecera, 8].Style.WrapText = true;

                                break;
                            case 5:
                                ws.Cells[finCabecera, 9].Value = "SERIE DE EQUIPO";
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
                                ws.Cells[finCabecera, 9, finCabecera, 10].Style.WrapText = true;

                                break;
                            case 6:
                                ws.Cells[finCabecera, 11].Value = "FECHA COMPRA";
                                ws.Cells[finCabecera, 11, finCabecera, 11].Merge = true;
                                ws.Cells[finCabecera, 11, finCabecera, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, 11, finCabecera, 11].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 11, finCabecera, 11].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 11, finCabecera, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 11, finCabecera, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                // Assign borders
                                ws.Cells[finCabecera, 11, finCabecera, 11].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 11, finCabecera, 11].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 11, finCabecera, 11].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 11, finCabecera, 11].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 11, finCabecera, 11].Style.Font.Bold = true;
                                ws.Cells[finCabecera, 11, finCabecera, 11].Style.WrapText = true;
                                break;
                            case 7:
                                ws.Cells[finCabecera, 12].Value = "VALOR";
                                ws.Cells[finCabecera, 12, finCabecera, 12].Merge = true;
                                ws.Cells[finCabecera, 12, finCabecera, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;

                                ws.Cells[finCabecera, 12, finCabecera, 12].Style.Fill.BackgroundColor.SetColor(colorGrisClaro3EstiloQPH);
                                ws.Cells[finCabecera, 12, finCabecera, 12].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);

                                ws.Cells[finCabecera, 12, finCabecera, 12].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                ws.Cells[finCabecera, 12, finCabecera, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                // Assign borders
                                ws.Cells[finCabecera, 12, finCabecera, 12].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 12, finCabecera, 12].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 12, finCabecera, 12].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 12, finCabecera, 12].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                ws.Cells[finCabecera, 12, finCabecera, 12].Style.Font.Bold = true;
                                ws.Cells[finCabecera, 12, finCabecera, 12].Style.WrapText = true;

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

                                    ws.Cells[finCabecera, col].Value = column.NombreEquipo;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Merge = true;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[finCabecera, col, finCabecera, 2].Style.WrapText = true;
                                    var caracteresD = column.Equipos.Count();

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
                                case 2:
                                    ws.Cells[finCabecera, 3].Value = column.Codigo;
                                    ws.Cells[finCabecera, 3, finCabecera, 4].Merge = true;
                                    ws.Cells[finCabecera, 3, finCabecera, 4].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 3, finCabecera, 4].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 3, finCabecera, 4].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 3, finCabecera, 4].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 3, finCabecera, 4].Style.WrapText = true;



                                    break;
                                case 3:
                                    ws.Cells[finCabecera, 5].Value = column.Factura;
                                    ws.Cells[finCabecera, 5, finCabecera, 6].Merge = true;
                                    ws.Cells[finCabecera, 5, finCabecera, 6].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 5, finCabecera, 6].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 5, finCabecera, 6].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 5, finCabecera, 6].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 5, finCabecera, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[finCabecera, 5, finCabecera, 6].Style.WrapText = true;

                                    break;
                                case 4:
                                    ws.Cells[finCabecera, 7].Value = column.TextoCatalogoProveedor;
                                    ws.Cells[finCabecera, 7, finCabecera, 8].Merge = true;
                                    ws.Cells[finCabecera, 7, finCabecera, 8].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 7, finCabecera, 8].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 7, finCabecera, 8].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 7, finCabecera, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 7, finCabecera, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[finCabecera, 7, finCabecera, 8].Style.WrapText = true;

                                    break;
                                case 5:
                                    ws.Cells[finCabecera, 9].Value = column.SerieEquipo;
                                    ws.Cells[finCabecera, 9, finCabecera, 10].Merge = true;
                                    ws.Cells[finCabecera, 9, finCabecera, 10].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 9, finCabecera, 10].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 9, finCabecera, 10].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 9, finCabecera, 10].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 9, finCabecera, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[finCabecera, 9, finCabecera, 10].Style.WrapText = true;

                                    break;
                                case 6:
                                    ws.Cells[finCabecera, 11].Value = column.FechaCompra != null ? column.FechaCompra.ToString("dd/MM/yyyy") : string.Empty; column.FechaCompra.ToString("dd/MM/yyyy");
                                    ws.Cells[finCabecera, 11, finCabecera, 11].Merge = true;
                                    ws.Cells[finCabecera, 11, finCabecera, 11].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 11, finCabecera, 11].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 11, finCabecera, 11].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 11, finCabecera, 11].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 11, finCabecera, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[finCabecera, 11, finCabecera, 11].Style.WrapText = true;

                                    break;
                                case 7:
                                    ws.Cells[finCabecera, 12].Value = column.Costo;
                                    ws.Cells[finCabecera, 12, finCabecera, 12].Merge = true;
                                    ws.Cells[finCabecera, 12, finCabecera, 12].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 12, finCabecera, 12].Style.Border.Left.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 12, finCabecera, 12].Style.Border.Right.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 12, finCabecera, 12].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                                    ws.Cells[finCabecera, 12, finCabecera, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                    ws.Cells[finCabecera, 12, finCabecera, 12].Style.Numberformat.Format = "0.00";
                                    ws.Cells[finCabecera, 12, finCabecera, 12].Style.WrapText = true;

                                    break;

                                default:
                                    ws.Cells[finCabecera, col++].Value = "";
                                    break;
                            }


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
                ws.Cells[finCabecera, 2, finCabecera, 4].Value = "LIZETH BRICEÑO";
                ws.Cells[finCabecera, 2, finCabecera, 4].Style.Font.Bold = true;
                ws.Cells[finCabecera, 2, finCabecera, 4].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                ws.Cells[finCabecera, 2, finCabecera, 4].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[finCabecera, 2, finCabecera, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells[finCabecera, 6, finCabecera, 7].Merge = true;
                ws.Cells[finCabecera, 6, finCabecera, 7].Value = "DAVID OÑA";
                ws.Cells[finCabecera, 6, finCabecera, 7].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                ws.Cells[finCabecera, 6, finCabecera, 7].Style.Font.Bold = true;
                ws.Cells[finCabecera, 6, finCabecera, 7].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[finCabecera, 6, finCabecera, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells[finCabecera, 9, finCabecera, 11].Merge = true;
                ws.Cells[finCabecera, 9, finCabecera, 11].Value = informacionCompleta.Codificacion.NombresApellidos;
                ws.Cells[finCabecera, 9, finCabecera, 11].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                ws.Cells[finCabecera, 9, finCabecera, 11].Style.Font.Bold = true;
                ws.Cells[finCabecera, 9, finCabecera, 11].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[finCabecera, 9, finCabecera, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                finCabecera++;

                ws.Cells[finCabecera, 2, finCabecera, 4].Merge = true;
                ws.Cells[finCabecera, 2, finCabecera, 4].Value = "ANALISTA DE ADQUISICIONES Y ADMINISTRACIÓN";
                ws.Cells[finCabecera, 2, finCabecera, 4].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                ws.Cells[finCabecera, 2, finCabecera, 4].Style.Font.Bold = true;
                ws.Cells[finCabecera, 2, finCabecera, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;


                ws.Cells[finCabecera, 6, finCabecera, 7].Merge = true;
                ws.Cells[finCabecera, 6, finCabecera, 7].Value = "TÉCNICO DE HELP-DESK";
                ws.Cells[finCabecera, 6, finCabecera, 7].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                ws.Cells[finCabecera, 6, finCabecera, 7].Style.Font.Bold = true;
                ws.Cells[finCabecera, 6, finCabecera, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                ws.Cells[finCabecera, 9, finCabecera, 11].Merge = true;
                ws.Cells[finCabecera, 9, finCabecera, 11].Value = informacionCompleta.Codificacion.TextoCatalogoCargo; ;
                ws.Cells[finCabecera, 9, finCabecera, 11].Style.Font.Color.SetColor(colorGrisOscuroEstiloQPH);
                ws.Cells[finCabecera, 9, finCabecera, 11].Style.Font.Bold = true;
                ws.Cells[finCabecera, 9, finCabecera, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;


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
                string rutaArchivos = basePath + "\\ENTRADA_SALIDAPERSONAL_RRHH\\CODIFICACION_EQUIPO";

                string nombreficha = informacionCompleta.Codificacion.NombresApellidos;

                var anioActual = DateTime.Now.Year.ToString();
                var almacenFisicoTemporal = Auxiliares.CrearCarpetasDirectorio(rutaArchivos, new List<string>() { anioActual, nombreficha });

                string pathExcel = Path.Combine(almacenFisicoTemporal, "CODIFICACION-EQUIPO-" + informacionCompleta.Codificacion.NombresApellidos + ".xlsx");

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
        #region REPORTES BASICOS
        public ActionResult DescargarReporteFormatoExcel()
        {
            var collection = CodificacionEquipoDAL.ListadoReporteBasico();
            var package = GetEXCEL(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(package.GetAsByteArray(), XlsxContentType, "Listado.xlsx");
        }

        public ActionResult DescargarReporteFormatoPDF()
        {
            // Seleccionar las columnas a exportar
            var collection = CodificacionEquipoDAL.ListadoReporteBasico();
            byte[] buffer = GetPDF(columnasReportesBasicos, collection.Cast<object>().ToList(), "Listado de Asignaciones");

            return File(buffer, PDFContentType, "ReportePDF.pdf");
        }

        public ActionResult DescargarReporteFormatoCSV()
        {
            var collection = CodificacionEquipoDAL.ListadoReporteBasico();
            byte[] buffer = GetCSV(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(buffer, CSVContentType, $"Listado.csv");
        }
        #endregion

    }
}