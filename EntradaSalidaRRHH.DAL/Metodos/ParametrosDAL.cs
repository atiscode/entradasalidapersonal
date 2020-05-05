using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace EntradaSalidaRRHH.DAL.Metodos
{
    public class ParametrosDAL
    {
        private static readonly AdministracionEntities db = new AdministracionEntities();

        public static RespuestaTransaccion CrearParametrosSistema(ParametrosSistema parametros)
        {
            try
            {
                parametros.Nombre = parametros.Nombre.ToUpper();
                parametros.Estado = true;
                db.ParametrosSistema.Add(parametros);
                db.SaveChanges();

                return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }

        public static RespuestaTransaccion ActualizarParametrosSistema(ParametrosSistema parametros)
        {
            try
            {
                // Por si queda el Attach de la entidad y no deja actualizar
                var local = db.ParametrosSistema.FirstOrDefault(f => f.IdParametro == parametros.IdParametro);
                if (local != null)
                {
                    db.Entry(local).State = EntityState.Detached;
                }

                parametros.Nombre = parametros.Nombre.ToUpper();
                db.Entry(parametros).State = EntityState.Modified;
                db.SaveChanges();

                return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }

        //Eliminación Lógica
        public static RespuestaTransaccion EliminarParametrosSistema(int id)
        {
            try
            {
                var parametros = db.ParametrosSistema.Find(id);

                if (parametros.Estado == true)
                {
                    parametros.Estado = false;
                }
                else
                {
                    parametros.Estado = true;
                }

                db.Entry(parametros).State = EntityState.Modified;
                db.SaveChanges();

                return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }

        public static List<ParametrosSistemasInfo> ListarParametros()
        {
            try
            {
                return db.ListadoParametrosSistemas().ToList();
            }
            catch (Exception ex)
            {
                return new List<ParametrosSistemasInfo>();
            }
        }

        public static ParametrosSistema ConsultarParametros(int id)
        {
            try
            {
                ParametrosSistema parametros = (db.ConsultarParametrosSistema(id).FirstOrDefault() ?? new ParametrosSistema());
                return parametros;
            }
            catch (Exception ex)
            {
                return new ParametrosSistema();
            }
        }

        public static List<ParametrosSistemasReporteBasico> ListadoReporteBasico()
        {
            List<ParametrosSistemasReporteBasico> listado = new List<ParametrosSistemasReporteBasico>();
            try
            {
                listado = db.ListadoParametrosSistemas().Select(p => new ParametrosSistemasReporteBasico
                {
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Valor = p.Valor,
                    Tipo = p.TextoTipo,
                    Estado = p.TextoEstadoTablaParametros,

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