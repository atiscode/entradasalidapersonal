using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.UI.Controllers
{
    public class EnviosMasivosNotificacionesController : BaseController
    {
        // GET: EnviosMasivosNotificaciones
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(string texto)
        {
            CargaMasiva carga = new CargaMasiva();
            try
            {
                List<string> listadoMasivoEmails = new List<string>();

                foreach (string item in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[item] as HttpPostedFileBase;
                    string fileName = file.FileName;

                    string extension = Path.GetExtension(fileName);

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

                if (!carga.GetEstado()) {
                    Resultado.Estado = false;
                    Resultado.Respuesta = Mensajes.MensajeCargaMasivaFallida;
                    Resultado.Adicional = "Filas inválidas: " + string.Join(" , ", carga.Detalles.Select(s => s.Fila).ToList());
                    return Json(new { Resultado }, JsonRequestBehavior.AllowGet);
                }

                //Verificar mails duplicados
                bool existenEmailsDuplicados = listadoMasivoEmails.GroupBy(x => x).Any(g => g.Count() > 1);
                if (existenEmailsDuplicados)
                {
                    //Obtener listado de mails duplicados
                    var emailsDuplicados = listadoMasivoEmails.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();

                    var resultadoMails = String.Join(" ; ", emailsDuplicados);

                    Resultado.Estado = false;
                    Resultado.Respuesta = Mensajes.MensajeErrorItemsRepetidos;
                    Resultado.Adicional = resultadoMails;

                    return Json(new { Resultado }, JsonRequestBehavior.AllowGet);
                }

                List<Notificaciones> listadoBatchNotificaciones = new List<Notificaciones>();

                string body = GetEmailTemplate("TemplateEnviosMasivosFocus");

                foreach (var mail in listadoMasivoEmails)
                {
                    listadoBatchNotificaciones.Add(new Notificaciones
                    {
                        NombreTarea = "Notificación Sorteos Focus Research",
                        DescripcionTarea = "Mail de notificación de resultados de sorteos a usuario de la Empresa Focus Research.",
                        NombreEmisor = nombreCorreoEmisorMasivo,
                        CorreoEmisor = correoEmisorMasivo,
                        ClaveCorreo = claveEmisorMasivo,
                        CorreosDestinarios = mail,
                        AsuntoCorreo = "TEST NOTIFICATION MAILING",
                        NombreArchivoPlantillaCorreo = "MailingSorteosFocus",
                        CuerpoCorreo = body,
                        AdjuntosCorreo = "",//ruta,
                        FechaEnvioCorreo = DateTime.Now,
                        Empresa = "FOCUS RESEARCH",
                        Canal = "MAILING MASIVO FOCUS RESEARCH",
                        Tipo = "NOTIFICACION DE SORTEO SIMPLE",
                    });
                }

                Resultado = NotificacionesDAL.SaveInBatchNotifications(listadoBatchNotificaciones);

                if (Resultado.Estado)
                    Resultado.Respuesta = Resultado.Respuesta + listadoBatchNotificaciones.Count + " puestas en cola.";

                return Json(new { Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}