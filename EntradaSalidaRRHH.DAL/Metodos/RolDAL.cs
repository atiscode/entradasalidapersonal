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
    public class RolDAL
    {
        private static readonly AdministracionEntities db = new AdministracionEntities();

        public static RespuestaTransaccion CrearRol(Rol rol, List<int> idPerfiles)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    rol.Nombre = rol.Nombre.ToUpper();
                    rol.Estado = true;
                    db.Rol.Add(rol);
                    db.SaveChanges();

                    var rolesPerfilesAnteriores = db.RolPerfil.Where(s => s.IdRol == rol.IdRol).ToList();
                    foreach (var item in rolesPerfilesAnteriores)
                    {
                        db.RolPerfil.Remove(item);
                        db.SaveChanges();
                    }

                    List<RolPerfil> ListadoRolesPerfiles = new List<RolPerfil>();

                    foreach (var item in idPerfiles)
                    {
                        db.RolPerfil.Add(new RolPerfil
                        {
                            IdRol = rol.IdRol,
                            IdPerfil = item,
                        });
                        db.SaveChanges();
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

        public static RespuestaTransaccion ActualizarRol(Rol rol, List<int> idPerfiles)
        {
            try
            {
                // Por si queda el Attach de la entidad y no deja actualizar
                var local = db.Rol.FirstOrDefault(f => f.IdRol == rol.IdRol);
                if (local != null)
                {
                    db.Entry(local).State = EntityState.Detached;
                }

                var rolesPerfilesAnteriores = db.RolPerfil.Where(s => s.IdRol == rol.IdRol).ToList();
                foreach (var item in rolesPerfilesAnteriores)
                {
                    db.RolPerfil.Remove(item);
                    db.SaveChanges();
                }

                List<RolPerfil> ListadoRolesPerfiles = new List<RolPerfil>();

                foreach (var item in idPerfiles)
                {
                    db.RolPerfil.Add(new RolPerfil
                    {
                        IdRol = rol.IdRol,
                        IdPerfil = item,
                    });
                    db.SaveChanges();
                }

                rol.Nombre = rol.Nombre.ToUpper();
                db.Entry(rol).State = EntityState.Modified;
                db.SaveChanges();

                return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }

        //Eliminación Lógica
        public static RespuestaTransaccion EliminarRol(int id)
        {
            try
            {
                var rol = db.Rol.Find(id);

                if (rol.Estado == true)
                {
                    rol.Estado = false;
                }
                else
                {
                    rol.Estado = true;
                }

                db.Entry(rol).State = EntityState.Modified;
                db.SaveChanges();

                return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }

        public static List<RolInfo> ListarRol()
        {
            try
            {
                return db.ListadoRol().ToList();
            }
            catch (Exception e)
            {
                return new List<RolInfo>();
            }
        }

        public static IEnumerable<SelectListItem> ObtenerListadoRoles()
        {
            var ListadoRoles = ListarRol().Where(r => r.EstadoRol == true).OrderBy(r => r.Nombre).Select(x => new SelectListItem
            {
                Text = x.Nombre,
                Value = x.IdRol.ToString()
            }).ToList();

            return ListadoRoles;
        }

        public static Rol ConsultarRol(int id)
        {
            try
            {
                Rol rol = db.ConsultarRol(id).FirstOrDefault() ?? new Rol();
                return rol;
            }
            catch (Exception ex)
            {
                return new Rol();
            }
        }

        public static List<int> ListadIdsPerfilesByRol(int idRol)
        {
            try
            {
                var listadoPerfiles = db.RolPerfil.Where(s => s.IdRol == idRol).Select(s => s.IdPerfil).ToList();
                return listadoPerfiles;
            }
            catch (Exception)
            {
                return new List<int>();
            }
        }

        //public static IEnumerable<SelectListItem> ListadIdsPerfilByRol(int idRol)
        //{
        //    try
        //    {
        //        var listadoPerfiles = db.RolPerfil.Where(s => s.IdRol == idRol).OrderBy(c => c.).Select(c => new SelectListItem
        //        {
        //            Text = c.Nombre.ToUpper(),
        //            Value = c.Id.ToString()
        //        }).ToList();
        //        return listadoPerfiles;

        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public static IEnumerable<SelectListItem> ListadoRolPerfil(int idRol)
        {
            List<SelectListItem> listado = new List<SelectListItem>();
            try
            {
                listado = db.ListadoRolPerfil(idRol).OrderBy(c => c.NombrePerfil).Select(c => new SelectListItem
                {
                    Text = c.NombrePerfil.ToUpper(),
                    Value = c.IdPerfil.ToString()
                }).ToList();

                return listado;
            }
            catch (Exception)
            {
                return listado;
            }

        }

        public static List<RolesReporteBasico> ListadoReporteBasico()
        {
            List<RolesReporteBasico> listado = new List<RolesReporteBasico>();
            try
            {
                listado = db.ListadoRol().Select(p => new RolesReporteBasico
                {
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Estado = p.TextoEstado

                }).ToList();

                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }
    }
}