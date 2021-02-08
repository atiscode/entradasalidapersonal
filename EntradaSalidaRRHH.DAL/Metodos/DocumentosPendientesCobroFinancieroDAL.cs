using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.DAL.Metodos
{
    public class DocumentosPendientesCobroFinancieroDAL
    {
        private static readonly SAFIEntities db = new SAFIEntities();

        public static List<DocumentosPendientesCobroP2P> ListarDocumentos(DateTime FechaInicio, DateTime FechaFin, int? tipo)
        {
            List<DocumentosPendientesCobroP2P> listado = new List<DocumentosPendientesCobroP2P>();

            //Verificar los filtros en procedimiento almacenado - Quitarlos si la consulta se vuelve muy lenta
            var filtroSoloPendientesCobro = db.ListadoDocumentosPendientesCobroP2P(FechaInicio, FechaFin).Select(s => s.NumeroFactura).ToList();

            listado = db.ReporteDocumentosPendientesCobroP2P(null, null, tipo).Where(s => filtroSoloPendientesCobro.Contains(s.ReferenciaFactura)).ToList();
            listado = listado.Where(s => s.FechaEmision >= FechaInicio && s.FechaEmision <= FechaFin).ToList();

            return listado;

        }

        public static IEnumerable<SelectListItem> ObtenerListadoTipoReferencia()
        {
            List<SelectListItem> ListadoCatalogo = new List<SelectListItem> { new SelectListItem { Text = Etiquetas.TituloComboVacio, Value = string.Empty } };
            try
            {
                ListadoCatalogo.AddRange(
                    new List<SelectListItem> {
                        new SelectListItem{ Text = "MENSUALIDAD O TRANSACCIÓN", Value = "1" },
                        new SelectListItem{ Text = "CERTIFICACIÓN", Value = "2" },
                        new SelectListItem{ Text = "CONSOLIDADO", Value = "0" },
                    }
                    );
                return ListadoCatalogo;
            }
            catch (Exception ex)
            {
                return ListadoCatalogo;
            }
        }
    }
}