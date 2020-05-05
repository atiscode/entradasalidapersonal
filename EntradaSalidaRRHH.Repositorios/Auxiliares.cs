//using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EntradaSalidaRRHH.Repositorios
{
    public static class Auxiliares
    {
        //private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static List<string> GetNombreCamposObjeto(List<object> collection)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            List<string> listNames = collection.FirstOrDefault().GetType().GetFields(bindingFlags).Select(field => field.Name).ToList();

            return GetListaNombreColumnas(listNames).ConvertAll(d => d.ToUpper());
        }

        public static List<string> GetListaNombreColumnas(List<string> listado)
        {
            List<string> columnas = new List<string>();

            foreach (var item in listado)
            {
                int posicionInicial = item.IndexOf("<") + 1;
                int posicionFinal = item.IndexOf(">") - 1;
                string nombre = posicionInicial != -1 && posicionFinal != -1 ? item.Substring(posicionInicial, posicionFinal) : "";
                columnas.Add(nombre);
            }

            return columnas;
        }


        public static string GetEtiquetaEstado(string estado)
        {
            estado = (estado ?? "").ToUpper();

            string etiqueta = "<a href='#' class='badge badge-default'><i class='fa fa-info-circle' aria-hidden='true'></i> SIN ESTADO</a>";

            if(estado.Equals("ABIERTO"))
                etiqueta = "<a href='#' style='background-color:#4A8059;border-color: #4A8059;' class='badge badge-success'><i class='fa fa-unlock' aria-hidden='true'></i> " + estado + "</a>";

            if(estado.Equals("CERRADO"))
                etiqueta = "<a href='#' title='El caso ya no puede cambiar de estado.' style='background-color:#D0753B;border-color: #D0753B;' class='badge badge-danger'><i class='fa fa-lock' aria-hidden='true'></i> " + estado + "</a>";

            if (estado.Equals("ASIGNADO"))
                etiqueta = "<a href='#' style='background-color: #5DA0CC; border -color:  #5DA0CC; ' class='badge badge-info'><i class='fa fa-share' aria-hidden='true'></i> " + estado + "</a>";

            return etiqueta;
        }

        public static List<object> GetValoresCamposObjeto(object collection)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            List<object> listValues = collection.GetType().GetFields(bindingFlags).Select(field => field.GetValue(collection)).Where(value => value != null).ToList();
            return listValues;
        }

        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        //Devuelve un listado solo de los valores del objeto
        public static List<string> GetResponseStringValoresObjeto(object objeto) {
            List<string> response = new List<string>();
            try
            {
                Type t = objeto.GetType(); // Where obj is object whose properties you need.
                PropertyInfo[] pi = t.GetProperties();
                foreach (PropertyInfo p in pi)
                {
                    var valor = GetPropValue(objeto, p.Name);
                    response.Add((string)valor);
                }
                return response;
            }
            catch (Exception ex)
            {
                return response;
            }
        }

        public static string FormatearValorMilesDecimales(decimal? valor)
        {
            try
            {
                string specifier;
                CultureInfo culture;

                specifier = "N6";
                culture = CultureInfo.CreateSpecificCulture("es-ES");

                var conversion = valor.Value.ToString(specifier, culture);
                return conversion;
            }
            catch (Exception ex)
            {
                return "0";
            }
        }

        public static string ConvertToListHtml(string prefacturas, string ids, string enlace)
        {
            string resultado = string.Empty;
            try
            {

                if (!string.IsNullOrEmpty(prefacturas) && !string.IsNullOrEmpty(ids))
                {
                    var itemsIDS = ids.Split(',');//listado.Split(',');
                    var listadoPrefacturas = prefacturas.Split(',');

                    int indice = 0;
                    foreach (string idPrefactura in itemsIDS)
                    {
                        string prefactura = listadoPrefacturas.ElementAt(indice);
                        string enlaceDescarga = enlace + "?listadoIDs=" + idPrefactura + "&descargaDirecta=" + true;
                        resultado += "<ul><li><a title='Descargar prefactura.' href='" + enlaceDescarga + "'> #" + prefactura + "</a></li></ul>";
                        //resultado += "<li> <a href='" + enlace + "'" + @service + "</a></li>";
                        indice++;
                    }

                    //<a href="index.html">valid</a>
                }

                return resultado;
            }
            catch (Exception ex)
            {
                return resultado;
            }
        }

        public static bool Base64Decode(string base64EncodedData, string rutaCompleta)
        {
            try
            {
                var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
                File.WriteAllBytes(rutaCompleta/*"pdf.pdf"*/, base64EncodedBytes);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static string ConvertToLinkDescargaHtml(string id, string nombreArchivo, string enlace)
        {
            string resultado = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(nombreArchivo) && !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(enlace))
                {
                    string enlaceDescarga = enlace + "?id=" + id;
                    resultado += "<ul><li><a title='Descargar Archivo.' href='" + enlaceDescarga + "'>" + nombreArchivo + "</a></li></ul>";
                }
                return resultado;
            }
            catch (Exception ex)
            {
                return resultado;
            }
        }

        public static string LeerParametrizacionWebCofig(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                string result = appSettings[key] ?? string.Empty;
                return result.ToString();
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static bool GetValorBooleano(string valor)
        {
            valor = valor.ToUpper();
            bool valorBooleano;
            switch (valor)
            {
                case "FALSE":
                    valorBooleano = false;
                    break;
                case "NO":
                    valorBooleano = false;
                    break;
                case "TRUE":
                    valorBooleano = true;
                    break;
                case "SI":
                    valorBooleano = true;
                    break;
                default:
                    valorBooleano = false;
                    break;
            }
            return valorBooleano;
        }

        //Para Font Awesome Icons
        public static string GetIconoExtension(string valor)
        {
            string icono;
            switch (valor)
            {
                case ".xls":
                    icono = "fa fa-file-excel-o";
                    break;
                case ".xlsx":
                    icono = "fa fa-file-excel-o";
                    break;
                case ".xlsm":
                    icono = "fa fa-file-excel-o";
                    break;
                case ".pdf":
                    icono = "fa fa-file-pdf-o";
                    break;
                default:
                    icono = "";
                    break;
            }
            return icono;
        }

        public static string CrearCarpetasDirectorio(string carpetaInicial, List<string> carpetas)
        {
            string camino = carpetaInicial;

            foreach (string ele in carpetas)
            {
                if (!Directory.Exists(Path.Combine(camino, ele)))
                {
                    Directory.CreateDirectory(Path.Combine(camino, ele));
                }
                camino = Path.Combine(camino, ele);
            }

            return camino;

        }

        //Para ejecutar varios procesos en la línea de comandos
        public static void EjecutarProcesosCMD(List<string> comandos, List<string> archivos, string rutaExe = "")
        {
            try
            {
                Process cmd = new Process();
                cmd.StartInfo.FileName = rutaExe;// "cmd.exe";
                cmd.StartInfo.Arguments = archivos[0] + " " + archivos[1];
                cmd.StartInfo.RedirectStandardInput = true;

                cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.UseShellExecute = false;
                cmd.Start();

                foreach (var item in comandos)
                {
                    cmd.StandardInput.WriteLine(item);
                }

                cmd.StandardInput.Flush();
                cmd.StandardInput.Close();
                cmd.WaitForExit();
                Console.WriteLine(cmd.StandardOutput.ReadToEnd());
            }
            catch (Exception ex)
            {
                //Log.Error(ex, "Excepcion");
            }
        }

        //Para objetos con propiedades en común
        public static T Casting<T>(this Object myobj)
        {
            Type objectType = myobj.GetType();
            Type target = typeof(T);
            var x = Activator.CreateInstance(target, false);
            var z = from source in objectType.GetMembers().ToList()
                    where source.MemberType == MemberTypes.Property
                    select source;
            var d = from source in target.GetMembers().ToList()
                    where source.MemberType == MemberTypes.Property
                    select source;
            List<MemberInfo> members = d.Where(memberInfo => d.Select(c => c.Name)
               .ToList().Contains(memberInfo.Name)).ToList();
            PropertyInfo propertyInfo;
            object value;
            foreach (var memberInfo in members)
            {
                propertyInfo = typeof(T).GetProperty(memberInfo.Name);
                value = myobj.GetType().GetProperty(memberInfo.Name).GetValue(myobj, null);

                propertyInfo.SetValue(x, value, null);
            }
            return (T)x;
        }

        

    }
}