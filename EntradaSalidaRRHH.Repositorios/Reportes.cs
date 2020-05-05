using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace EntradaSalidaRRHH.Repositorios
{
    public static class Reportes
    {
        public static ExcelPackage ExportarExcel(List<object> collection, string nombreHoja)
        {
            var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add(nombreHoja);

            var columnas = Auxiliares.GetNombreCamposObjeto(collection.Cast<object>().ToList());

            var i = 1;
            foreach (var item in columnas)
            {
                worksheet.Column(i).Width = 40;
                worksheet.Cells[1, i].Value = item;
                worksheet.Cells[1, i].Style.Font.Bold = true;
                //worksheet.Cells[1, i].Style.Fill.BackgroundColor. .SetColor(Color.FromArgb(23, 55, 93));
                i++;
            }

            CambiarColorFila(worksheet, 1, columnas.Count, Color.Orange);

            int fila = 2;
            foreach (var item in collection)
            {
                var objeto = Auxiliares.GetValoresCamposObjeto(item);
                int columna = 1;
                foreach (var valor in objeto)
                {
                    worksheet.Cells[fila, columna].Value = valor;
                    columna++;
                }
                fila++;
            }
             
            return package;

        }

        private static void CambiarColorFila(ExcelWorksheet hoja, int fila, int totalColumnas, Color color)
        {
            for (int i = 1; i <= totalColumnas; i++)
            {
                using (ExcelRange rowRange = hoja.Cells[fila, i])
                {
                    rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rowRange.Style.Fill.BackgroundColor.SetColor(color);
                }
            }
        }

        public static string SerializeToXML<T>(this T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }

        public static string SerializeToJSON(object objeto)
        {
            var list = JsonConvert.SerializeObject(objeto,
                Formatting.None,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                });
            return list;
        }


    }
}