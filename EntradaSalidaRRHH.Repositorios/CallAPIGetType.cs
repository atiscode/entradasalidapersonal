using System;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EntradaSalidaRRHH.Repositorios
{
    public class CallAPIGetType
    {
        private static readonly string authorization = Auxiliares.LeerParametrizacionWebCofig("ParametroHeaderAutorizacion");

        public static string ConsultarListados(string url, Method metodo = Method.GET,  bool auth = false, string contentType = null)
        {
            try
            {
                var conexion = new RestClient(url);
                var request = new RestRequest(metodo); // Por defecto GET

                if (auth)
                    request.AddHeader("Authorization", authorization);

                if(!string.IsNullOrEmpty(contentType))
                    request.AddHeader("Content-Type", contentType);

                var response = conexion.Execute(request);
                return response.Content;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string ConsultarListados(string url, params Parameter[] parameters)
        {
            try
            {
                var conexion = new RestClient(url);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", authorization);

                foreach (var parameter in parameters)
                    request.AddParameter(parameter);

                var response = conexion.Execute(request);
                return response.Content;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


    }
}
