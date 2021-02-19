using System.Web.Optimization;
using WebHelpers.Mvc5;

namespace EntradaSalidaRRHH.UI.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = true;

            bundles.Add(new StyleBundle("~/Bundles/css")
                 .Include("~/Content/css/bootstrap.min.css", new CssRewriteUrlTransformAbsolute())
                 .Include("~/Content/css/bootstrap-select.css")
                 .Include("~/Content/css/bootstrap-datepicker3.min.css")
                 .Include("~/Content/css/font-awesome.min.css", new CssRewriteUrlTransformAbsolute())
                 .Include("~/Content/css/pace/pace.min.css", new CssRewriteUrlTransformAbsolute()) //Animación cargas ajax
                 .Include("~/Content/css/AdminLTE.css", new CssRewriteUrlTransformAbsolute())
                 //wysihtml5
                 .Include("~/Content/js/plugins/bootstrap-wysihtml5/bootstrap3-wysihtml5.min.css", new CssRewriteUrlTransformAbsolute())
                 //.Include("~/Content/css/skins/skin-blue.css")
                 .Include("~/Content/css/skins/skin-asertec.css")
                 //search list
                 .Include("~/Content/bootstrap-chosen.css")
                 );

            bundles.Add(new ScriptBundle("~/Bundles/js")
                .Include("~/Content/js/plugins/jquery/jquery-3.3.1.min.js")
                //.Include("~/Scripts/jquery.easyui.min.js") // easyUI
                .Include("~/Content/js/plugins/bootstrap/bootstrap.js")
                .Include("~/Content/js/plugins/fastclick/fastclick.js")
                .Include("~/Content/js/plugins/slimscroll/jquery.slimscroll.js")
                .Include("~/Content/js/plugins/bootstrap-select/bootstrap-select.js")
                .Include("~/Content/js/plugins/moment/moment.js")
                .Include("~/Content/js/plugins/datepicker/bootstrap-datepicker.js")
                .Include("~/Content/js/plugins/pace/pace.min.js") //Animaciones ajax
                //wysihtml5
                .Include("~/Content/js/plugins/bootstrap-wysihtml5/bootstrap3-wysihtml5.all.min.js")
                .Include("~/Content/js/plugins/bootstrap-wysihtml5/locale/bootstrap-wysihtml5.es-ES.js") 
                .Include("~/Content/js/plugins/validator/validator.js")
                .Include("~/Content/js/plugins/inputmask/jquery.inputmask.bundle.js")
                .Include("~/Content/js/adminlte.js")
                .Include("~/Content/js/init.js")
                //search list
                .Include("~/Scripts/chosen.jquery.js")
                );
            

            bundles.Add(new ScriptBundle("~/Bundles/js_login")
    .Include("~/Content/js/plugins/jquery/jquery-3.3.1.js")
    //.Include("~/Scripts/jquery.easyui.min.js") // easyUI
    .Include("~/Content/js/plugins/pace/pace.min.js") //Animaciones ajax
    .Include("~/Content/js/plugins/bootstrap/bootstrap.js")
    .Include("~/Content/js/plugins/fastclick/fastclick.js")
    .Include("~/Content/js/plugins/slimscroll/jquery.slimscroll.js")
    .Include("~/Content/js/plugins/bootstrap-select/bootstrap-select.js")
    .Include("~/Content/js/plugins/moment/moment.js")
    .Include("~/Content/js/plugins/datepicker/bootstrap-datepicker.js")
    //.Include("~/Content/js/plugins/icheck/icheck.js")
    .Include("~/Content/js/plugins/validator/validator.js")
    .Include("~/Content/js/plugins/inputmask/jquery.inputmask.bundle.js")
    //.Include("~/Content/js/adminlte.js")
    //.Include("~/Content/js/init.js")
    //.Include("~/Content/js/init.js")
    );

#if DEBUG
            BundleTable.EnableOptimizations = false;
#else
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}
