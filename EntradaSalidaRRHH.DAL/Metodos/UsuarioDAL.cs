using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.DAL.Metodos
{
    public class UsuarioDAL
    {
        private static readonly AdministracionEntities db = new AdministracionEntities();

        public static RespuestaTransaccion CrearUsuario(Usuario usuario)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    //validar si existe el registro por el correo
                    var validarMail = db.ConsultarUsuarioExistente(usuario.Mail).ToList();
                    var validarIdentificacion = db.ConsultarUsuarioExistente(usuario.Identificacion).ToList();
                    var validarNombreUsuario = db.ConsultarUsuarioExistente(usuario.Username).ToList();
                    var validarMailCorporativo = db.ConsultarUsuarioExistente(usuario.MailCorporativo).ToList();

                    if (validarMail.Count() > 0)
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeResgistroExistenteIMail };
                    if (validarMailCorporativo.Count() > 0)
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeResgistroExistenteIMailCorporativo };
                    if (validarIdentificacion.Count() > 0)
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeResgistroExistenteIdentificacion };
                    if (validarNombreUsuario.Count() > 0)
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeResgistroExistenteNombreUsuario };

                    usuario.Nombres = (usuario.Nombres ?? string.Empty).ToUpper();
                    usuario.Apellidos = (usuario.Apellidos ?? string.Empty).ToUpper();
                    usuario.ResetClave = true;
                    usuario.ValidacionCorreo = false;
                    usuario.IdEmpresa = usuario.IdEmpresa;

                    //generar la clave random de 8
                    Random random = new Random();
                    String numero = "";
                    for (int i = 0; i < 9; i++)
                    {
                        numero += Convert.ToString(random.Next(0, 9));
                    }
                    //encriptar la clave 
                    var clave = Cryptography.Encrypt(numero);
                    usuario.Clave = clave;
                    usuario.Estado = true;
                    usuario.Activo = true;

                    db.Usuario.Add(usuario);
                    db.SaveChanges();

                    transaction.Commit();

                    return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa, EntidadID = usuario.IdUsuario };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
                }
            }
        }
        public static RespuestaTransaccion ActualizarUsuario(Usuario usuario)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    bool validarMail = db.ListarUsuarios(null).Any(s => s.Mail == usuario.Mail && s.IdUsuario != usuario.IdUsuario);
                    bool validarIdentificacion = db.ListarUsuarios(null).Any(s => s.Identificacion == usuario.Identificacion && s.IdUsuario != usuario.IdUsuario);
                    bool validarNombreUsuario = db.ListarUsuarios(null).Any(s => s.Username == usuario.Username && s.IdUsuario != usuario.IdUsuario);
                    bool validarMailCorporativo = db.ListarUsuarios(null).Any(s => s.MailCorporativo == usuario.MailCorporativo && s.IdUsuario != usuario.IdUsuario);

                    if (validarMail)
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeResgistroExistenteIMail };
                    if (validarMailCorporativo)
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeResgistroExistenteIMailCorporativo };
                    if (validarIdentificacion)
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeResgistroExistenteIdentificacion };
                    if (validarNombreUsuario)
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeResgistroExistenteNombreUsuario };

                    // assume Entity base class have an Id property for all items
                    var entity = db.Usuario.Find(usuario.IdUsuario);
                    if (entity == null)
                    {
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida };
                    }

                    db.Entry(entity).CurrentValues.SetValues(usuario);
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

        public static RespuestaTransaccion ActualizarDatosUsuarioFichaIngreso(Usuario usuario, bool formularioIngreso = false)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // assume Entity base class have an Id property for all items
                    var entidad = db.Usuario.Find(usuario.IdUsuario);

                    //Solo cuando sea para el formulario de Ingreso actualizo estos datos
                    if (formularioIngreso)
                    {
                        entidad.Area = usuario.Area;
                        entidad.Departamento = usuario.Departamento;
                        entidad.Cargo = usuario.Cargo;
                        entidad.MailCorporativo = usuario.MailCorporativo;
                        entidad.IdEmpresa = usuario.IdEmpresa;
                    }
                    else
                    {
                        //Para el formulario de Ficha de Ingreso
                        entidad.Direccion = usuario.Direccion;
                        entidad.Pais = usuario.Pais;
                        entidad.Ciudad = usuario.Ciudad;
                        entidad.Telefono = usuario.Telefono;
                    }

                    db.Entry(entidad).State = EntityState.Modified;
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


        public static RespuestaTransaccion DesactivarUsuario(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var entidad = db.Usuario.Find(id);
                    entidad.Activo = false;
                    db.Entry(entidad).State = EntityState.Modified;
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
        public static RespuestaTransaccion EliminarUsuario(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var usuario = db.Usuario.Find(id);

                    if (usuario.Estado == true)
                    {
                        usuario.Estado = false;
                    }
                    else
                    {
                        usuario.Estado = true;
                    }

                    db.Entry(usuario).State = EntityState.Modified;
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
        public static List<UsuarioInfo> ListarUsuarios()
        {
            try
            {
                return db.ListadoUsuarios(null).ToList();
            }
            catch (Exception e)
            {
                return new List<UsuarioInfo>();
            }
        }
        public static UsuarioInfo ConsultarUsuario(int id)
        {
            try
            {
                UsuarioInfo usuario = db.ListadoUsuarios(id).FirstOrDefault() ?? new UsuarioInfo();
                return usuario;
            }
            catch (Exception ex)
            {
                return new UsuarioInfo();
            }
        }


        public static Usuario ConsultarUsuarioEdit(int id)
        {
            try
            {
                Usuario parametros = (db.ConsultarUsuario(id).FirstOrDefault() ?? new Usuario());
                return parametros;
            }
            catch (Exception ex)
            {
                return new Usuario();
            }
        }

        public static UsuarioInfo ValidarUsuario(string usuario, string clave)
        {
            try
            {
                var UsuarioLogin = db.ConsultarUsuarioClave(usuario, clave).FirstOrDefault() ?? new UsuarioInfo();
                return UsuarioLogin;
            }
            catch (Exception ex)
            {
                return new UsuarioInfo();
            }
        }

        public static RespuestaTransaccion CrearUsuarioGenerico(RegisterViewModel usuario)
        {

            try
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        Usuario usuarioFinal = new Usuario();

                        usuarioFinal.Nombres = usuario.Nombre.ToUpper();
                        usuarioFinal.Apellidos = usuario.Apellidos.ToUpper();
                        usuarioFinal.MailCorporativo = usuario.Email;
                        usuarioFinal.Username = usuario.UserName;
                        usuarioFinal.Identificacion = usuario.Identificacion;

                        //obtener la clave 
                        var clave = Cryptography.Encrypt(usuario.Password);

                        usuarioFinal.Clave = clave;
                        usuarioFinal.Estado = true;
                        usuarioFinal.Activo = true;
                        //usuarioFinal.IdEmpresa = 1;
                        usuarioFinal.ResetClave = false;
                        usuarioFinal.ValidacionCorreo = false;
                        usuarioFinal.Telefono = usuario.telefono_usuario;
                        usuarioFinal.Celular = usuario.celular_usuario;


                        //Obtener el Rol generico
                        Rol rol = db.Rol.Where(r => r.Nombre == "ROL GENÉRICO" || r.Nombre == "ROL GENERICO").First();
                        usuarioFinal.IdRol = rol.IdRol;

                        db.Usuario.Add(usuarioFinal);
                        db.SaveChanges();
                        int id_usuario = usuarioFinal.IdUsuario;

                        //enviar correo
                        //db.EnviarCorreoUsuarioGenérico(id_usuario);
                        //db.SaveChanges();

                        transaction.Commit();
                        return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa, EntidadID = 0 };
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString(), EntidadID = 0 };
                    }
                }
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }

        }

        public static RespuestaTransaccion RecuperarClave(ForgotViewModel recuperar)
        {
            try
            {
                try
                {
                    //validar si existe el registro
                    Usuario usuarioActual = db.Usuario.Where(u => u.MailCorporativo == recuperar.Login).First();

                    //obtener la clave 
                    Random random = new Random();
                    String numero = "";
                    for (int i = 0; i < 9; i++)
                    {
                        numero += Convert.ToString(random.Next(0, 9));
                    }

                    var clave = Cryptography.Encrypt(numero);

                    usuarioActual.Clave = clave;
                    usuarioActual.ResetClave = true;

                    // Por si queda el Attach de la entidad y no deja actualizar
                    var local = db.Usuario.FirstOrDefault(f => f.IdUsuario == usuarioActual.IdUsuario);
                    if (local != null)
                    {
                        db.Entry(local).State = EntityState.Detached;
                    }

                    db.Entry(usuarioActual).State = EntityState.Modified;
                    db.SaveChanges();

                    ////enviar correo
                    //db.EnviarCorreoUsuarioResetClave(usuarioActual.IdUsuario, numero);
                    //db.SaveChanges();

                    return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeRecuperacionClave, EntidadID = long.Parse(numero)};

                }
                catch (Exception ex)
                {
                    ex.ToString();
                    return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeUsuarioNoExiste };
                }
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }

        public static RespuestaTransaccion CambiarClave(CambiarClave cambiarClave)
        {
            try
            {
                //validar las contraseña nuevas coincidan
                if (cambiarClave.ContraseniaNueva != cambiarClave.ConfirmarContrasenia)
                {
                    return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeValidacionContrasenias };
                }
                else
                {
                    if (cambiarClave.ContraseniaNueva.Length < 8)
                    {
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeLongitudContrasenia };
                    }
                    else
                    {
                        Usuario usuario = db.ConsultarUsuario(Convert.ToInt32(cambiarClave.idUsuario)).FirstOrDefault() ?? new Usuario();

                        //hacer md5 a la clave actual
                        var claveActual = Cryptography.Encrypt(cambiarClave.ContraseniaActual);
                        var claveNueva = Cryptography.Encrypt(cambiarClave.ContraseniaNueva);

                        //Validar que la calve nueva sea diferente a la anterior
                        if (cambiarClave.ContraseniaActual == cambiarClave.ContraseniaNueva)
                        {
                            return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeCambioContaseña };
                        }
                        else
                        {

                            //Validar las claves si coinciden
                            if (claveActual == usuario.Clave)
                            {
                                usuario.Clave = claveNueva;

                                // Por si queda el Attach de la entidad y no deja actualizar
                                var local = db.Usuario.FirstOrDefault(f => f.IdUsuario == usuario.IdUsuario);
                                if (local != null)
                                {
                                    db.Entry(local).State = EntityState.Detached;
                                }

                                db.Entry(usuario).State = EntityState.Modified;
                                db.SaveChanges();

                                //db.EnviarCorreoUsuarioCambioClave(usuario.IdUsuario);
                                db.SaveChanges();

                                return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
                            }
                            else
                            {
                                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeValidacionContraseniaActual };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " " + ex.ToString() };
            }
        }

        public static List<OpcionMenuUsuarioInfo> OpcionesMenuUsuario(string correo_username)
        {
            try
            {
                return db.ListarOpcionMenuUsuario(correo_username).ToList();
            }
            catch (Exception ex)
            {
                return new List<OpcionMenuUsuarioInfo>();
            }
        }

        public static bool VerificarCorreoUsuarioExistente(string correo)
        {
            try
            {
                bool existe = db.ListadoUsuarios(null).Any(s => s.Mail == correo && s.EstadoUsuario.Value);
                return existe;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static RespuestaTransaccion CambiarClaveReset(CambiarClave usuario)
        {
            try
            {
                if (usuario.ContraseniaNueva != usuario.ConfirmarContrasenia)
                    return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeValidacionContrasenias };
                else
                {
                    if (usuario.ContraseniaNueva.Length < 8)
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeLongitudContrasenia };
                    else
                    {
                        var claveNueva = Cryptography.Encrypt(usuario.ContraseniaNueva);

                        //Datos actuales del usuario  
                        Usuario usuarioActual = new Usuario();
                        usuarioActual = db.Usuario.Find(usuario.UsuaCodi);
                        usuarioActual.ResetClave = false;
                        usuarioActual.Clave = claveNueva;

                        db.Entry(usuarioActual).State = EntityState.Modified;
                        db.SaveChanges();

                        //db.EnviarCorreoUsuarioCambioClave(usuarioActual.IdUsuario);
                       //db.SaveChanges();

                        return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };

                    }
                }
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " " + ex.ToString() };
                throw;
            }
        }

        public static UsuarioInfo ConsultarInformacionPrincipalUsuario(int id)
        {
            UsuarioInfo Usuario = new UsuarioInfo();
            try
            {
                Usuario = db.ListadoUsuarios(id).FirstOrDefault() ?? new UsuarioInfo();
                return Usuario;
            }
            catch (Exception ex)
            {
                return Usuario;
            }
        }

        public static bool LoginCorrecto(string login, string password)
        {
            //Verificar si el usuario existe.
            bool existeUsuario = db.ListarUsuarios(null).Any(s => s.MailCorporativo == login || s.Username == login);

            if (existeUsuario)
            {
                password = Cryptography.Encrypt(password);
                var loginCorrecto = db.ListarUsuarios(null).Any(s => (s.MailCorporativo == login || s.Username == login) && s.Clave == password);

                if (loginCorrecto)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        public static UsuarioInfo ConsultarLogin(string login, string password)
        {
            try
            {
                bool existeUsuario = db.ListarUsuarios(null).Any(s => s.MailCorporativo == login || s.Username == login);

                if (existeUsuario)
                {
                    password = Cryptography.Encrypt(password);
                    //Verificar credenciales de usuario
                    var usuario = db.ListarUsuarios(null).FirstOrDefault(s => (s.MailCorporativo == login || s.Username == login) && s.Clave == password);

                    if (usuario != null)
                        return usuario;
                    else
                        return null;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static UsuarioInfo ConsultarUsuarioLogin(string login)
        {
            try
            {
                //Verificar si el usuario existe.
                bool existeUsuario = db.ListarUsuarios(null).Any(s => s.MailCorporativo == login || s.Username == login);

                if (existeUsuario)
                {
                    var usuario = db.ListarUsuarios(null).FirstOrDefault(s => (s.MailCorporativo == login || s.Username == login));

                    if (usuario != null)
                        return usuario;
                    else
                        return null;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static int ConsultaUsuarioId(string correo)
        {
            try
            {
                var usuario = db.ConsultarUsuarioId(correo).FirstOrDefault() ?? new UsuarioInfo();
                int UsuarioId = usuario.IdUsuario;

                return UsuarioId;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        //username - Mail - MailCorporativo
        public static UsuarioInfo ConsultaUsuarioByEmail(string correo)
        {
            try
            {
                var usuario = db.ConsultarUsuarioId(correo).FirstOrDefault() ?? new UsuarioInfo();
                return usuario;
            }
            catch (Exception ex)
            {
                return new UsuarioInfo();
            }
        }

        public static bool CorreoExistente(string correo)
        {
            try
            {
                //Verificar si el usuario existe.
                bool existecorreo = db.ListarUsuarios(null).Any(s => s.MailCorporativo == correo);

                if (existecorreo)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static bool IdentificacionExistente(string identificacion)
        {
            try
            {
                //Verificar si el usuario existe.
                bool existecorreo = db.ListarUsuarios(null).Any(s => s.Identificacion == identificacion);

                if (existecorreo)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static bool NombreUsuarioExistente(string nombreusuario)
        {
            try
            {
                //Verificar si el usuario existe.
                bool existecorreo = db.ListarUsuarios(null).Any(s => s.Username == nombreusuario);

                if (existecorreo)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool ExisteCorreoNormalCorporativo(string correo)
        {
            try
            {
                //Verificar si el usuario existe.
                bool existecorreo = db.ListarUsuarios(null).Any(s => s.Mail == correo || s.MailCorporativo == correo);

                if (existecorreo)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static IEnumerable<SelectListItem> ObtenerListadoFichasIngreso(string seleccionado = null)
        {
            List<SelectListItem> ListadoCatalogo = new List<SelectListItem> { new SelectListItem { Text = Etiquetas.TituloComboVacio, Value = string.Empty } };
            try
            {
                ListadoCatalogo.AddRange(db.ListadoUsuarios(null).Select(c => new SelectListItem
                {
                    Text = c.NombresApellidos.ToString() + " - " + c.Identificacion,
                    Value = c.IdUsuario.ToString()
                }).ToList());

                if (!string.IsNullOrEmpty(seleccionado))
                {
                    if (ListadoCatalogo.FirstOrDefault(s => s.Value == seleccionado.ToString()) != null)
                        ListadoCatalogo.FirstOrDefault(s => s.Value == seleccionado.ToString()).Selected = true;
                }

                return ListadoCatalogo;
            }
            catch (Exception ex)
            {
                return ListadoCatalogo;
            }
        }

        public static List<UsuariosReporteBasico> ListadoReporteBasico()
        {
            List<UsuariosReporteBasico> listado = new List<UsuariosReporteBasico>();
            try
            {
                listado = db.ListadoUsuarios(null).Select(u => new UsuariosReporteBasico
                {
                    NombresApellidos = u.NombresApellidos,
                    Identificacion = u.Identificacion,
                    NombreUsuario = u.Username,
                    Departamento = u.TextoCatalogoDepartamento,
                    Area = u.TextoCatalogoArea,
                    Cargo = u.TextoCatalogoCargo,
                    Pais = u.TextoCatalogoPais,
                    Ciudad = u.TextoCatalogoCiudad,
                    Direccion = u.Direccion,
                    Mail = u.Mail,
                    MailCorporativo = u.MailCorporativo,
                    Telefono = u.Telefono,
                    Celular = u.Celular,
                    Rol = u.NombreRol,
                    Estado = u.TextoEstadoUsuario

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