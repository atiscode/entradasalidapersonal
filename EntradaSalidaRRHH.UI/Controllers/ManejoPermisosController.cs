using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using EntradaSalidaRRHH.UI.Helper;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.UI.Controllers
{
    [Autenticado]
    public class ManejoPermisosController : BaseController
    {
        // GET: ManejoPermisos
        public ActionResult Index()
        {
            Session["ContadorRecorridoColumnas"] = 0;
            int? numeroColumna = System.Web.HttpContext.Current.Session["numeroColumna"] as int?;
            string nombreColumna = System.Web.HttpContext.Current.Session["nombreColumna"] as string;

            const string sessionVariableName = "num";
            Session[sessionVariableName] = 0;

            ViewBag.ListadoCatalogoHijos = CatalogoDAL.ObtenerListadoCatalogosCodigoPadre("ACCIONES-SIST-01");

            //Listado Rol  
            var roles = RolDAL.ObtenerListadoRoles();
            ViewBag.listadoRoles = roles;
            return View();
        }

        [HttpGet]
        public async Task<PartialViewResult> IndexGrid(String search, int rolID = 0, int perfilID = 0)
        {
            DataTable table = new DataTable();
            DataRow row = table.NewRow();

            Session["Fila"] = row;

            Session["ContadorRecorridoColumnas"] = 0;

            ViewBag.NombreListado = Etiquetas.TituloGridPermisos;
            var nombreControlador = ControllerContext.RouteData.Values["controller"].ToString();
            ViewBag.NombreControlador = nombreControlador;

            List<UsuarioRolMenuPermisoInfo> listado = new List<UsuarioRolMenuPermisoInfo>();
            List<UsuarioRolMenuPermisoR> listadoFinal = new List<UsuarioRolMenuPermisoR>();

            ViewBag.ListadoMenusSistema = MenuDAL.ObtenerListadoMenusAplicacion();

            ViewBag.ListadoCatalogo = CatalogoDAL.ObtenerListadoCatalogos("ACCIONES-SIST-01");

            ViewBag.ListadoCatalogos = CatalogoDAL.ObtenerListadoCatalogosCodigoPadre("ACCIONES-SIST-01");

            ViewBag.PerfilReporte = perfilID;
            ViewBag.RolReporte = rolID;

            //Listado Rol Menú Permiso

            listado = ManejoPermisosDAL.ListadoRolMenuPermiso(rolID, perfilID);

            foreach (var item in listado)
            {
                UsuarioRolMenuPermisoR tmp = new UsuarioRolMenuPermisoR();
                tmp.IDRolMenuPermiso = item.IDRolMenuPermiso;
                tmp.RolID = item.RolID;
                tmp.NombreRol = item.NombreRol;
                tmp.PerfilID = item.PerfilID;
                tmp.NombrePerfil = item.NombrePerfil;
                tmp.MenuID = item.MenuID;
                tmp.NombreMenu = item.NombreMenu;
                tmp.EnlaceMenu = item.EnlaceMenu;
                tmp.MenuPadre = item.MenuPadre;
                tmp.IDCatalogo = item.IDCatalogo;
                tmp.CodigoCatalogo = item.CodigoCatalogo;
                tmp.TextoCatalogoAccion = item.TextoCatalogoAccion;
                tmp.CreadoPorID = item.CreadoPorID;
                tmp.CreadoPor = item.CreadoPor;
                tmp.ActualizadoPorID = item.ActualizadoPorID;
                tmp.ActualizadoPor = item.ActualizadoPor;
                tmp.CreatedAt = item.CreatedAt;
                tmp.UpdatedAt = item.UpdatedAt;
                tmp.Estado = item.Estado;
                tmp.MetodoControlador = item.MetodoControlador;
                tmp.NombreControlador = item.NombreControlador;
                tmp.AccionEnlace = item.AccionEnlace;

                listadoFinal.Add(tmp);
            }

            ViewBag.ListadoAcciones = listadoFinal;

            //Acciones Controlador
            ViewBag.AccionesControlador = GetMetodosControlador(nombreControlador);

            //Búsqueda

            DataTable datos = new DataTable();
            datos = GetDatosgrid(rolID, perfilID);

            search = !string.IsNullOrEmpty(search) ? search.Trim() : "";

            if (!string.IsNullOrEmpty(search))
            {
                string _sqlWhere = "NOMBREMENU like '%{0}%'";

                _sqlWhere = string.Format(_sqlWhere, search);

                string _sqlOrder = "NOMBREMENU DESC";

                DataRow[] dataRow = datos.Select(_sqlWhere, _sqlOrder);
                if (dataRow != null && dataRow.Length > 0)
                {
                    DataTable filadatos = dataRow.CopyToDataTable();
                    filadatos = datos.Select(_sqlWhere, _sqlOrder).CopyToDataTable();
                    datos = filadatos;
                }
                else
                {
                    datos.Clear();
                }
            }

            else
            {
                datos = GetDatosgrid(rolID, perfilID);
            }

            return PartialView("_IndexGrid", await Task.Run(() => datos));
        }

        [HttpPost]
        public ActionResult Guardar(List<RolMenuPermiso> rolmenupermiso, int rolID, int perfilID)
        {

            rolmenupermiso = rolmenupermiso ?? new List<RolMenuPermiso>();
            try
            {

                var usuario = UsuarioLogeadoSession.IdUsuario;
                RespuestaTransaccion resultado = ManejoPermisosDAL.CrearActualizarPermisos(rolmenupermiso, usuario, usuario, DateTime.Now, DateTime.Now, rolID, perfilID);
                return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return View();
            }
        }
        public DataTable GetDatosgrid(int? rolID, int? perfilID)
        {
            DataTable dt = new DataTable();
            string str = ConfigurationManager.ConnectionStrings["ConexionDirectaADO"].ConnectionString;

            using (SqlConnection cn = new SqlConnection(str))
            {

                using (SqlCommand cmd = new SqlCommand("ListarPermisos"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDRol", rolID.ToString());
                    cmd.Parameters.AddWithValue("@IDPerfil", perfilID.ToString());
                    cmd.Connection = cn;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public ActionResult ListarPerfiles(int? rolID)
        {
            List<SelectListItem> listado = new List<SelectListItem>();
            listado.Add(new SelectListItem
            {
                Text = Etiquetas.TituloComboVacio,
                Value = string.Empty,
            });

            try
            {
                if (!rolID.HasValue)
                    return Json(listado, JsonRequestBehavior.AllowGet);

                var perfil = PerfilesDAL.ListarPerfilesPorRol(rolID.Value);

                listado.AddRange(perfil.Select(x => new SelectListItem
                {
                    Text = x.NombrePerfil.ToString(),
                    Value = x.IdPerfil.ToString()
                }).ToList());

                return Json(listado, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(listado, JsonRequestBehavior.AllowGet);
            }
        }


        //Descargar Reportes
        public ActionResult DescargarReporteFormatoExcel(int? rol, int? perfil)
        {
            DataTable datos = new DataTable();
            datos = GetDatosReportes(rol, perfil);

            using (var excel = new ExcelPackage())
            {
                var workSheet = excel.Workbook.Worksheets.Add("ReporteManejoPermisos");
                workSheet.Cells[1, 1].LoadFromDataTable(datos, PrintHeaders: true);
                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();

                return File(excel.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ReporteManejoPermisosExcel.xlsx");
            }

        }

        public ActionResult DescargarReporteFormatoPDF(int? rol, int? perfil)
        {

            DataTable datos = new DataTable();
            datos = GetDatosReportes(rol, perfil);

            var results = datos;
            var list = Reportes.SerializeToJSON(results);

            return Content(list, "application/json");
        }

        public ActionResult DescargarReporteFormatoCSV(int? rol, int? perfil)
        {
            DataTable dt = new DataTable();
            dt = GetDatosReportes(rol, perfil);
            string csv = string.Empty;

            foreach (DataColumn column in dt.Columns)
            {
                csv += column.ColumnName + ',';
            }
            csv += "\r\n";

            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn column in dt.Columns)
                {
                    csv += row[column.ColumnName].ToString().Replace(",", ";") + ',';
                }

                csv += "\r\n";
            }

            var data = Encoding.UTF8.GetBytes(csv);
            var result = Encoding.UTF8.GetPreamble().Concat(data).ToArray();

            return File(result, "text/csv", "ManejoPermisosCSV.csv");

        }

        public DataTable GetDatosReportes(int? rolID, int? perfilID)
        {
            DataTable dt = new DataTable();
            string str = ConfigurationManager.ConnectionStrings["ConexionDirectaADO"].ConnectionString;

            using (SqlConnection cn = new SqlConnection(str))
            {
                using (SqlCommand cmd = new SqlCommand("ListadoMenuPermisos"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDRol", rolID.ToString());
                    cmd.Parameters.AddWithValue("@IDPerfil", perfilID.ToString());
                    cmd.Connection = cn;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }
    }
}

