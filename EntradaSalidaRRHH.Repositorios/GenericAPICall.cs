using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using Newtonsoft.Json;

namespace EntradaSalidaRRHH.Repositorios
{
    public class GenericAPICall
    {
        private string baseURL;
        private IAuthenticator authenticator;

        public GenericAPICall(string baseURL, IAuthenticator authenticator)
        {
            this.baseURL = baseURL;
            this.authenticator = authenticator;
        }

        public GenericAPICall(string baseURL, string username, string password)
        {
            this.baseURL = baseURL;
            authenticator = new HttpBasicAuthenticator(username, password);
        }

        public string Request(Method method, string endPoint, Dictionary<string, object> headers, Dictionary<string, object> parameters, Dictionary<string, object> queryParameters, string body)
        {
            RestClient client = new RestClient(baseURL);
            RestRequest request = new RestRequest(endPoint, method);
            client.Authenticator = authenticator;
            foreach (var key in headers.Keys)
            {
                if (headers[key].GetType().ToString().StartsWith("System.Collections.Generics.List"))
                {
                    request.AddHeader(key, JsonConvert.SerializeObject(headers[key]));
                }
                else
                {
                    request.AddHeader(key, headers[key].ToString());
                }
            }
            foreach (var key in parameters.Keys)
            {
                request.AddParameter(key, parameters[key]);
            }
            foreach (var key in queryParameters.Keys)
            {
                if (headers[key].GetType().ToString().StartsWith("System.Collections.Generics.List"))
                {
                    request.AddQueryParameter(key, JsonConvert.SerializeObject(queryParameters[key]));
                }
                else
                {
                    request.AddQueryParameter(key, queryParameters[key].ToString());
                }
            }
            var response = client.Execute(request);
            return response.Content;
        }
    }
}
