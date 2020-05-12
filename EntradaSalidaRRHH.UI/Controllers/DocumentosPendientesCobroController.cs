using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.UI.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.UI.Controllers
{
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

        public ActionResult DescargarReporteFormatoCSV(int TipoReferencia, DateTime fechaInicio, DateTime fechaFin, string descripcionServicio)
        {
            List<DocumentosPendientesCobroP2P> reporte = DocumentosPendientesCobroFinancieroDAL.ListarDocumentos(fechaInicio, fechaFin, TipoReferencia);

            string FechaGeneracion = DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "");
            string ValorObligatorio1 = "1000";
            string ValorObligatorio2 = "A";
            string RucComercio = "1791219058001";
            string NumeroFacturasImportadas = reporte.Count.ToString();
            string CodigoServicio = TipoReferencia.ToString();
            string DescripcionCodigoServicio = descripcionServicio;
            string SumaTotalReporte = reporte.Select(s => s.ValorFacturaDecimal).Sum().ToString().Replace(".", "");

            var comlumHeadrs = new string[]
            {
                FechaGeneracion,
                ValorObligatorio1,
                ValorObligatorio2,
                RucComercio,
                NumeroFacturasImportadas,
                CodigoServicio,
                DescripcionCodigoServicio,
                SumaTotalReporte,
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
            listado.ForEach(line =>
            {
                objetoCSV.AppendLine(string.Join(",", line));
            });

            byte[] buffer = Encoding.Default.GetBytes($"{string.Join(",", comlumHeadrs)}\r\n{objetoCSV.ToString()}");
            return File(buffer, CSVContentType, $"ReporteFinancieroP2P.csv");
        }

    }
}