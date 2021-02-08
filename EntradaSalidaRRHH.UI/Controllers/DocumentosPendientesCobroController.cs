using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using EntradaSalidaRRHH.UI.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.UI.Controllers
{
    public class ReporteExcel
    {
        public string FechaEmision { get; set; }
        public string ReferenciaFactura { get; set; }
        public string DocumentoDelComprador { get; set; }
        public string NombreDelComprador { get; set; }
        public int? Cantidad { get; set; }
        public decimal? PrecioUnitario { get; set; }
        //public decimal? PorcentajeIVA { get; set; }
        public decimal? ValorIVA { get; set; }
        public decimal? ValorFacturaDecimal { get; set; }
        public string FechaVencimientoSinFormato { get; set; }
        public string FechaCorteSinFormato { get; set; }
        public string Tipo { get; set; }
    }


    [Autenticado]
    public class DocumentosPendientesCobroController : BaseController
    {
        // GET: DocumentosPendientesCobro
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult _ReporteFinanciero()
        {
            ViewBag.TituloModal = "REPORTE";
            return PartialView();
        }

        [HttpPost]
        public ActionResult DescargarReporteFormatoCSV(int? TipoReferencia, DateTime fechaInicio, DateTime fechaFin, string descripcionServicio)
        {
            List<DocumentosPendientesCobroP2P> reporte = new List<DocumentosPendientesCobroP2P>();

            try
            {
                reporte = DocumentosPendientesCobroFinancieroDAL.ListarDocumentos(fechaInicio, fechaFin, TipoReferencia == 0 ? null : TipoReferencia);

                string FechaGeneracion = DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "");
                string ValorObligatorio1 = "1000";
                string ValorObligatorio2 = "A";
                string RucComercio = "1791219058001";
                string NumeroFacturasImportadas = reporte.Count.ToString();
                string CodigoServicio = TipoReferencia.ToString();
                string DescripcionCodigoServicio = descripcionServicio;
                string SumaTotalReporte = reporte.Select(s => s.ValorFacturaDecimal).Sum().ToString().Replace(",", "").Replace(".", "");

                var comlumHeadrs = new string[]
                {
                FechaGeneracion,
                ValorObligatorio1,
                ValorObligatorio2,
                RucComercio,
                NumeroFacturasImportadas,
                CodigoServicio,
                DescripcionCodigoServicio,
                SumaTotalReporte + " "
                };

                var listado = (from item in reporte
                               select new object[]
                                   {
                               item.ReferenciaFactura,
                               item.ReferenciaAlterna,
                               item.DocumentoDelComprador,
                               item.NombreDelComprador,
                               item.ValorFactura,
                               item.FechaVencimiento,
                               item.ValorRecargo,
                               item.FechaCorte,
                               item.CodigoRecargo,
                                   }).ToList();

                // Build the file content
                var objetoCSV = new StringBuilder();

                //Agregando cabecera
                objetoCSV.AppendLine(string.Join(",", comlumHeadrs.ToList()));
                //Agregando detalles
                listado.ForEach(line =>
                {
                    objetoCSV.AppendLine(string.Join(",", line));
                });

                byte[] buffer = Encoding.Default.GetBytes($"{objetoCSV.ToString()}");


                var package = GetEXCEL(new List<string> {
                "FECHA EMISIÓN",
                "NÚMERO DE DOCUMENTO",
                "DOCUMENTO DEL COMPRADOR",
                "NOMBRE DEL COMPRADOR",
                "CANTIDAD",
                "PRECIO UNITARIO",
                //"PORCENTAJE IVA",
                "VALOR IVA",
                "VALOR",
                "FECHA DE VENCIMIENTO",
                "FECHA CORTE",
                "TIPO",
            }, reporte.Select(s => new ReporteExcel
            {
                FechaEmision = s.FechaEmision.Value.ToString("yyyy-MM-dd"),
                ReferenciaFactura = s.ReferenciaFactura,
                DocumentoDelComprador = s.DocumentoDelComprador,
                NombreDelComprador = s.NombreDelComprador,
                Cantidad = s.Cantidad,
                PrecioUnitario = s.PrecioUnitario,
                //PorcentajeIVA = s.PorcentajeIVA,
                ValorIVA = s.ValorIVA,
                ValorFacturaDecimal = s.ValorFacturaDecimal,
                FechaVencimientoSinFormato = s.FechaVencimientoSinFormato,
                FechaCorteSinFormato = s.FechaCorteSinFormato,
                Tipo = s.Tipo
            }).Cast<object>().ToList());


                string nombreArchivoExcel = "ReportePendientesCobroP2P_" + DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "") + ".xlsx";
                string nombreArchivoCSV = "ReportePendientesCobroP2P_" + DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "") + ".csv";

                // Get the complete folder path and store the file inside it.    
                string pathExcel = Path.Combine(Server.MapPath("~/Documentos/Temporales/"), nombreArchivoExcel);
                string pathCSV = Path.Combine(Server.MapPath("~/Documentos/Temporales/"), nombreArchivoCSV);

                //Guardar excel en disco
                package.SaveAs(new FileInfo(pathExcel));
                System.IO.File.WriteAllBytes(pathCSV, buffer);

                return Json(new { Resultado = new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa }, Files = new List<string> { pathExcel, pathCSV } }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Auxiliares.RegisterException(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida }, Files = new List<string> { } }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}