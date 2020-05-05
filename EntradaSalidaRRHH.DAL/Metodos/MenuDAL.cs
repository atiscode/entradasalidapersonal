using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.DAL.Metodos
{
    public class MenuDAL
    {
        private static readonly AdministracionEntities db = new AdministracionEntities();
        public static RespuestaTransaccion CrearMenu(Menu Menu)
        {
            try
            {
                Menu.EstadoMenu = true;
                Menu.OrdenMenu = 0;
                db.Menu.Add(Menu);
                db.SaveChanges();

                return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }

        public static RespuestaTransaccion ActualizarMenu(Menu Menu)
        {
            try
            {
                // Por si queda el Attach de la entidad y no deja actualizar
                var local = db.Menu.FirstOrDefault(f => f.IdMenu == Menu.IdMenu);
                if (local != null)
                {
                    db.Entry(local).State = EntityState.Detached;
                }

                db.Entry(Menu).State = EntityState.Modified;
                db.SaveChanges();

                return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }

        //Eliminación Lógica
        public static RespuestaTransaccion EliminarMenu(int id)
        {
            try
            {
                var Menu = db.Menu.Find(id);

                Menu.EstadoMenu = false;

                db.Entry(Menu).State = EntityState.Modified;
                db.SaveChanges();

                return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }

        public static List<MenuInfo> ListarMenu()
        {
            try
            {
                return db.ListadoMenu().ToList();
            }
            catch (Exception)
            {
                return new List<MenuInfo>();
            }
        }

        public static List<Menu> ListarMenuHijos()
        {
            try
            {
                return db.ConsultarMenu(null).Where(s => s.IdMenuPadre != null && s.EstadoMenu == true).ToList();
            }
            catch (Exception)
            {
                return new List<Menu>();
            }
        }

        public static Menu ConsultarMenu(int id)
        {
            try
            {
                Menu menu = db.ConsultarMenu(id).FirstOrDefault() ?? new Menu();
                return menu;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static IEnumerable<SelectListItem> ListadoMeruHijos()
        {
            List<SelectListItem> listado = new List<SelectListItem>();
            try
            {
                listado = db.ListaMenuHijos().OrderBy(c => c.NombreMenu).Select(c => new SelectListItem
                {
                    Text = c.NombreMenu.ToUpper(),
                    Value = c.IdMenu.ToString()
                }).ToList();

                return listado;
            }
            catch (Exception ex)
            {
                return listado;
            }
        }

        public static List<MenuReporteBasico> ListadoReporteBasico()
        {
            List<MenuReporteBasico> listado = new List<MenuReporteBasico>();
            try
            {
                listado = db.ListadoMenu().Select(p => new MenuReporteBasico
                {
                    MenuPAdre = p.Padre,
                    OpcionMenu = p.OpcionMenu,
                    RutaAcceso = p.RutaAcceso,
                    Estado = p.TextoEstadoMenu

                }).ToList();

                return listado;
            }
            catch (Exception)
            {
                return listado;
            }
        }

        public static RespuestaTransaccion ActualizarOrdenMenu(string itemIds)
        {
            try
            {
                int count = 1;
                List<int> itemIdList = new List<int>();
                itemIdList = itemIds.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                foreach (var itemId in itemIdList)
                {
                    try
                    {
                        Menu item = db.Menu.Where(x => x.IdMenu == itemId).FirstOrDefault();
                        item.OrdenMenu = count;

                        db.Menu.AddOrUpdate(item);
                        db.SaveChanges();
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    count++;
                }
                return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }

        public static IEnumerable<SelectListItem> ObtenerListadoMenusAplicacion()
        {
            List<SelectListItem> listado = new List<SelectListItem>();
            try
            {
                listado = db.ListadoMenu().Select(c => new SelectListItem
                {
                    Text = c.RutaAcceso,
                    Value = c.IdMenu.ToString()
                }).ToList();

                return listado;
            }
            catch (Exception)
            {
                return listado;
                throw;
            }

        }
        public static List<MenuRutaAccesoInfo> ConsultarRutaMenu(int id)
        {

            try
            {

                List<MenuRutaAccesoInfo> menu = db.ConsultarMenuRutaAcceso(id).ToList();

                return menu;
            }
            catch (Exception ex)
            {
                throw;
            }

        }


    }
}
