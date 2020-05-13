using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using EntradaSalidaRRHH.UI.Helper;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.UI.Controllers
{
    [Autenticado]
    public class EnviosMasivosNotificacionesController : BaseController
    {
        // GET: EnviosMasivosNotificaciones
        public ActionResult Index()
        {
            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EnviarPrueba(string Destinatarios, string description, string Asunto)
        {
            if (string.IsNullOrEmpty(Destinatarios)) {
                Resultado.Estado = false;
                Resultado.Respuesta = "Los campos con * son requeridos";
                return Json(new { Resultado }, JsonRequestBehavior.AllowGet);
            }

            var listado = Destinatarios.Split(',').ToList();
            List<string> mailNoValidos = new List<string>();

            foreach (var item in listado)
            {
                if (!Validaciones.ValidarMail(item))
                    mailNoValidos.Add(item);
            }

            if (mailNoValidos.Any())
            {
                var resultadoMails = String.Join(" ; ", mailNoValidos); // Invalidos
                Resultado.Estado = false;
                Resultado.Respuesta = Mensajes.MensajeTransaccionFallida + " Los siguientes emails no son válidos: " + resultadoMails;
                Resultado.Adicional = resultadoMails;
                return Json(new { Resultado }, JsonRequestBehavior.AllowGet);
            }
            else {
                Destinatarios = Destinatarios.Replace(",", ";");
            }
                
            description = !string.IsNullOrEmpty(description) ? description :  GetEmailTemplate("TemplateEnviosMasivosFocus");
            Resultado = NotificacionesDAL.CrearNotificacion(new Notificaciones
            {
                NombreTarea = "Notificación Focus de prueba generado por el usuario.",
                DescripcionTarea = "El usuario prueba un envío de notificación para verificar que todo esté funcional.",
                NombreEmisor = nombreCorreoEmisor,
                CorreoEmisor = correoEmisor,
                ClaveCorreo = claveEmisor,
                CorreosDestinarios = Destinatarios,
                AsuntoCorreo = string.IsNullOrEmpty(Asunto) ? "NOTIFICACIÓN SORTEO" : Asunto,
                NombreArchivoPlantillaCorreo = "MailingSorteosFocus",
                CuerpoCorreo = description,
                AdjuntosCorreo = "",//ruta,
                FechaEnvioCorreo = DateTime.Now,
                Empresa = "FOCUS AND RESEARCH",
                Canal = "PRUEBA NOTIFICACION FOCUS",
                Tipo = "PRUEBA",
            });

            return Json(new { Resultado }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(string texto)
        {
            CargaMasiva carga = new CargaMasiva();
            try
            {
                //En caso de que no haya ningún archivo cargado
                if (Request.Files.Count == 0) {
                    Resultado.Estado = false;
                    Resultado.Respuesta = Mensajes.MensajeTransaccionFallida;
                    return Json(new { error = Mensajes.MensajeErrorAdjuntosRequeridos +" Por favor, verificar el archivo.", Resultado = Resultado }, JsonRequestBehavior.AllowGet);
                }

                List<string> listadoMasivoEmails = new List<string>();

                foreach (string item in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[item] as HttpPostedFileBase;
                    string fileName = file.FileName;

                    string extension = Path.GetExtension(fileName);

                    if (extension != ".xlsx" && extension != ".xls") {
                        Resultado.Estado = false;
                        Resultado.Respuesta = Mensajes.MensajeTransaccionFallida;
                        return Json(new { error = "Formato no permitido.", Resultado = Resultado }, JsonRequestBehavior.AllowGet);
                    }

                    //SI LA RUTA EN DISCO NO EXISTE LOS ARCHIVOS SE ALMACENAN EN LA CARPETA MISMO DEL PROYECTO
                    string rutaBase = basePathRepositorioDocumentos + "\\RRHH\\ArchivosCargasMasivas";
                    bool directorio = Directory.Exists(rutaBase);
                    // En caso de que no exista el directorio, crearlo.
                    if (!directorio)
                        Directory.CreateDirectory(rutaBase);

                    string pathServidor = Path.Combine(rutaBase, fileName);

                    if (file.ContentLength > 0)
                    {
                        file.SaveAs(pathServidor);

                        FileInfo existingFile = new FileInfo(pathServidor);

                        using (ExcelPackage package = new ExcelPackage(existingFile))
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets.First();

                            if (worksheet != null)
                            {
                                int colCount = worksheet.Dimension.End.Column;  //get Column Count
                                int rowCount = worksheet.Dimension.End.Row;      //get row count - Cabecera
                                for (int row = 2; row <= rowCount; row++)
                                {
                                    for (int col = 1; col <= colCount; col++)
                                    {
                                        var error = string.Empty;
                                        string columna = (worksheet.Cells[1, col].Value ?? "").ToString().Trim(); // Nombre de la Columna
                                        string valorColumna = (worksheet.Cells[row, col].Value ?? "").ToString().Trim();

                                        bool mailValido = Validaciones.ValidarMail(valorColumna);

                                        //Fill errors mails
                                        if (!mailValido)
                                            carga.Detalles.Add(new DetallesCargaMasiva { Fila = row, Columna = col, Valor = valorColumna, Error = error });
                                        else
                                            listadoMasivoEmails.Add(valorColumna);
                                    }
                                }
                            }
                        }

                    }
                }

                List<string> erroresValidacionGenerales = new List<string>();

                if (!carga.GetEstado())
                    erroresValidacionGenerales.Add(string.Format(Mensajes.MensajeValidacionFilasInvalidas, string.Join(" , ", carga.Detalles.Select(s => s.Fila).ToList())));

                //Verificar mails duplicados
                List<string> emailsDuplicados = listadoMasivoEmails.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
                
                if (emailsDuplicados.Count > 0)
                    erroresValidacionGenerales.Add(string.Format(Mensajes.MensajeValidacionEmailsRepetidos, string.Join(" ; ", emailsDuplicados)));

                //Validación límite máximo de mails
                if (listadoMasivoEmails.Count > 2000)
                {
                    Resultado.Estado = false;
                    Resultado.Respuesta = Mensajes.MensajeTransaccionFallida + "Límite excedido.";
                    return Json(new {  Resultado }, JsonRequestBehavior.AllowGet);
                }

                List<Notificaciones> listadoBatchNotificaciones = new List<Notificaciones>();

                string body = GetEmailTemplate("TemplateEnviosMasivosFocus");

                int totalListado = listadoMasivoEmails.Count;

                //Filtrando en caso de haber repetidos
                listadoMasivoEmails = listadoMasivoEmails.Where(s => !emailsDuplicados.Contains(s)).ToList();

                foreach (var mail in listadoMasivoEmails)
                {
                    DateTime fechaEnvio = DateTime.Now;

                    Random rnd = new Random();
                    int segundos = rnd.Next(1, 150);
                    int minutos = 0;

                    if(totalListado >= 500 && totalListado <= 1000)
                        minutos = rnd.Next(1, 25);
                    if (totalListado > 1000 && totalListado <= 2000)
                        minutos = rnd.Next(1, 120);

                    fechaEnvio = totalListado > 50 ? fechaEnvio.AddSeconds(segundos) : fechaEnvio.AddSeconds(rnd.Next(1, 10));
                    fechaEnvio = fechaEnvio.AddMinutes(minutos);
                    fechaEnvio = fechaEnvio.AddMilliseconds(rnd.Next(1, 100));

                    listadoBatchNotificaciones.Add(new Notificaciones
                    {
                        NombreTarea = "Notificación Sorteos Focus Research",
                        DescripcionTarea = "Mail de notificación de resultados de sorteos a usuario de la Empresa Focus Research.",
                        NombreEmisor = nombreCorreoEmisorMasivo,
                        CorreoEmisor = correoEmisorMasivo,
                        ClaveCorreo = claveEmisorMasivo,
                        CorreosDestinarios = mail,
                        AsuntoCorreo = "NOTIFICACIÓN SORTEO",
                        NombreArchivoPlantillaCorreo = "MailingSorteosFocus",
                        CuerpoCorreo = body,
                        AdjuntosCorreo = "",//ruta,
                        FechaEnvioCorreo = fechaEnvio,
                        Empresa = "FOCUS RESEARCH",
                        Canal = "MAILING MASIVO FOCUS RESEARCH",
                        Tipo = "NOTIFICACION DE SORTEO SIMPLE",
                    });
                }

                Resultado = NotificacionesDAL.SaveInBatchNotifications(listadoBatchNotificaciones);

                if (Resultado.Estado) {
                    Resultado.Respuesta = Resultado.Respuesta + " " + listadoBatchNotificaciones.Count + " notificaciones preparadas para ser enviadas.";
                    Resultado.Adicional = erroresValidacionGenerales.Count > 0 ? string.Format(Mensajes.MensajeValidacionCargaMasivaMailsExitosaConErrores, string.Join(" | ", erroresValidacionGenerales.Select(s => s).ToList())) : string.Empty;
                }

                return Json(new { Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}