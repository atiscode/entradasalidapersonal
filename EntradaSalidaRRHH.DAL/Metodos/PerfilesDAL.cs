using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.DAL.Metodos
{
    public class PerfilesDAL
    {
        private static readonly AdministracionEntities db = new AdministracionEntities();

        public static RespuestaTransaccion CrearPerfil(Perfil Perfil)
        {
            try
            {
                Perfil.NombrePerfil = Perfil.NombrePerfil.ToUpper();
                Perfil.EstadoPerfil = true;
                db.Perfil.Add(Perfil);
                db.SaveChanges();

                return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }

        public static RespuestaTransaccion CrearPerfil(Perfil Perfil, List<int> opcionesMenu)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    Perfil.NombrePerfil = Perfil.NombrePerfil.ToUpper();
                    Perfil.EstadoPerfil = true;
                    db.Perfil.Add(Perfil);
                    db.SaveChanges();

                    var opcionesPerfilMenuAnteriores = db.PerfilMenu.Where(s => s.IdPerfil == Perfil.IdPerfil).ToList();
                    foreach (var item in opcionesPerfilMenuAnteriores)
                    {
                        db.PerfilMenu.Remove(item);
                        db.SaveChanges();
                    }

                    //List<RolPerfil> ListadoRolesPerfiles = new List<RolPerfil>();

                    foreach (var item in opcionesMenu)
                    {
                        db.PerfilMenu.Add(new PerfilMenu
                        {
                            IdPerfil = Perfil.IdPerfil,
                            IdMenu = item,
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

        public static RespuestaTransaccion ActualizarPerfil(Perfil Perfil)
        {
            try
            {
                // Por si queda el Attach de la entidad y no deja actualizar
                var local = db.Perfil.FirstOrDefault(f => f.IdPerfil == Perfil.IdPerfil);
                if (local != null)
                {
                    db.Entry(local).State = EntityState.Detached;
                }

                Perfil.NombrePerfil = Perfil.NombrePerfil.ToUpper();
                db.Entry(Perfil).State = EntityState.Modified;
                db.SaveChanges();

                return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }


        public static RespuestaTransaccion ActualizarPerfil(Perfil Perfil, List<int> opcionesMenu)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Por si queda el Attach de la entidad y no deja actualizar
                    var local = db.Perfil.FirstOrDefault(f => f.IdPerfil == Perfil.IdPerfil);
                    if (local != null)
                    {
                        db.Entry(local).State = EntityState.Detached;
                    }

                    var opcionesPerfilMenuAnteriores = db.PerfilMenu.Where(s => s.IdPerfil == Perfil.IdPerfil).ToList();
                    foreach (var item in opcionesPerfilMenuAnteriores)
                    {
                        db.PerfilMenu.Remove(item);
                        db.SaveChanges();
                    }

                    //List<RolPerfil> ListadoRolesPerfiles = new List<RolPerfil>();

                    foreach (var item in opcionesMenu)
                    {
                        db.PerfilMenu.Add(new PerfilMenu
                        {
                            IdPerfil = Perfil.IdPerfil,
                            IdMenu = item,
                        });
                        db.SaveChanges();
                    }

                    Perfil.NombrePerfil = Perfil.NombrePerfil.ToUpper();
                    db.Entry(Perfil).State = EntityState.Modified;
                    db.SaveChanges();

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

        //Eliminación Lógica
        public static RespuestaTransaccion EliminarPerfil(int id)
        {
            try
            {
                var perfil = db.Perfil.Find(id);

                if (perfil.EstadoPerfil == true)
                {
                    perfil.EstadoPerfil = false;
                }
                else
                {
                    perfil.EstadoPerfil = true;
                }

                db.Entry(perfil).State = EntityState.Modified;
                db.SaveChanges();

                return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }

        public static string ConsultarCorreoNotificacion(int id)
        {
            string correos = "";
            try
            {

                correos = db.ConsultarCorreosNotificaciones(id).FirstOrDefault();
                return correos;
            }
            catch (Exception ex)
            {
                return correos;
            }
        }

        public static List<PerfilInfo> ListarPerfil()
        {
            try
            {
                return db.ListadoPerfil().ToList();
            }
            catch (Exception)
            {
                return new List<PerfilInfo>();
            }
        }

        public static Perfil ConsultarPerfil(int id)
        {
            try
            {
                Perfil perfil = db.ConsultarPerfil(id).FirstOrDefault() ?? new Perfil();
                return perfil;
            }
            catch (Exception ex)
            {
                return new Perfil();
            }
        }

        public static List<int> ListadIdsOpcionesMenuByPerfil(int idPerfil)
        {
            try
            {
                var listadoPerfiles = db.PerfilMenu.Where(s => s.IdPerfil == idPerfil).Select(s => s.IdMenu).ToList();
                return listadoPerfiles;
            }
            catch (Exception)
            {
                return new List<int>();
            }
        }


        public static List<PerfilInfo> ListarPerfilesPorRol(int rolID)
        {
            List<PerfilInfo> perfiles = new List<PerfilInfo>();
            try
            {
                var listadoPerfilesRoles = db.RolPerfil.Where(s => s.IdRol == rolID).Select(s => s.IdPerfil).ToList();
                perfiles = db.ListadoPerfil().Where(s => listadoPerfilesRoles.Contains(s.IdPerfil)).ToList();
                return perfiles;
            }
            catch (Exception ex)
            {
                return perfiles;
            }
        }

        public static IEnumerable<SelectListItem> ListadoPerfil()
        {
            var ListadoPerfiles = db.ListadoPerfil().OrderBy(c => c.NombrePerfil).Select(c => new SelectListItem
            {
                Text = c.NombrePerfil.ToUpper(),
                Value = c.IdPerfil.ToString()
            }).ToList();

            return ListadoPerfiles;
        }

        public static IEnumerable<SelectListItem> ListadoPerfilMenu(int idPerfil)
        {
            List<SelectListItem> listado = new List<SelectListItem>();
            try
            {
                listado = db.ListadoPerfilMenu(idPerfil).OrderBy(c => c.OpcionMenu).Select(c => new SelectListItem
                {
                    Text = c.OpcionMenu.ToUpper(),
                    Value = c.IdMenu.ToString()
                }).ToList();

                return listado;
            }
            catch (Exception ex)
            {
                return listado;
            }
        }

        public static List<PerfilesReporteBasico> ListadoReporteBasico()
        {
            List<PerfilesReporteBasico> listado = new List<PerfilesReporteBasico>();
            try
            {
                listado = db.ListadoRol().Select(p => new PerfilesReporteBasico
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