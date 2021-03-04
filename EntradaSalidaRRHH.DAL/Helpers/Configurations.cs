using System.Configuration;

namespace EntradaSalidaRRHH.DAL.Helpers
{
    public class Configurations
    {
        public static bool EnvioDeCorreosProduccionActivado()
        {
            return ConfigurationManager.AppSettings["EnvioCorreosProduccionActivado"] == "true";
        }
    }
}