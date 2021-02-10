using System.Configuration;

namespace EntradaSalidaRRHH.UI.Configs
{
    public static class Configurations
    {
        public  static string GetBancoDefault()
        {
            return ConfigurationManager.AppSettings["BancoDefault"];
        }
    }
}