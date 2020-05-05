using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Metodos
{
    public class ManejoPermisosDAL
    {
        private static readonly AdministracionEntities db = new AdministracionEntities();
        public static RespuestaTransaccion CrearActualizarPermisos(List<RolMenuPermiso> Permisos, int createby, int updateby, DateTime createat, DateTime updateat, int rolID, int perfilID)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (Permisos.Count >= 1)
                    {
                        foreach (var item2 in Permisos)
                        {

                            db.LimpiarRolMenuPermisos(item2.RolID, item2.PerfilID, item2.MenuID);
                        }

                        foreach (var item in Permisos)
                        {

                            db.GuardarRolMenuPermisos(item.RolID, item.PerfilID, item.MenuID, item.AccionID, createby, updateby, createat, updateat, item.Estado);
                        }
                    }
                    else
                    {
                        db.LimpiarRolMenuPermisosCompleto(rolID, perfilID);

                    }

                    transaction.Commit();
                    return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
                }
            }
        }
        public static List<UsuarioRolMenuPermisoInfo> ConsultarRolMenuPermiso(int usuario, string controlador)
        {
            List<UsuarioRolMenuPermisoInfo> listado = new List<UsuarioRolMenuPermisoInfo>();
            try
            {
                listado = db.ConsultarUsuarioRolMenuPermiso(usuario, controlador).ToList();
                return listado;
            }
            catch (Exception ex)
            {
                return listado;
            }
        }
        public static List<string> ListadoAccionesCatalogoUsuario(int usuario, string controlador)
        {
            List<string> listado = new List<string>();
            try
            {
                listado = db.ConsultarUsuarioRolMenuPermiso(usuario, controlador).Select(s => s.CodigoCatalogo).ToList();
                return listado;
            }
            catch (Exception ex)
            {
                return listado;
            }
        }

        public static List<UsuarioRolMenuPermisoInfo> ListadoRolMenuPermiso(int idRol, int idPerfil)
        {
            List<UsuarioRolMenuPermisoInfo> listado = new List<UsuarioRolMenuPermisoInfo>();
            try
            {
                listado = db.ListadoRolPerfilMenuPermisos(idRol, idPerfil).ToList();
                return listado;
            }
            catch (Exception ex)
            {
                return listado;
            }
        }


       


    }
}