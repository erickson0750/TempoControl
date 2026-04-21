using Microsoft.Data.Sqlite;
using TempoControl.BaseDatos;
using TempoControl.LogicaNegocio;
using TempoControl.Presentacion;
using TempoControl.Repositorio.Implementaciones;

namespace TempoControl
{
    /// <summary>
    /// Punto de entrada de la aplicacion TempoControl.
    /// Aqui se realiza la composicion manual de dependencias y se muestra el menu principal.
    /// </summary>
    internal class Program
    {
        //Nombre del archivo de base de datos SQLite
        private const string ARCHIVO_BD = "tempocontrol.db";

        static void Main(string[] args)
        {
            //Configuracion cultura para fechas en espeñol
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("es-DO");

            try
            {
                // 1. Inicializar base de datos
                string cadenaConexion = $"Data Source={ARCHIVO_BD};";
                var inicializador = new InicializadorBaseDatos(cadenaConexion);
                inicializador.inicializar();

                // 2. Instanciar repositorios (Capa Acceso a Datos)
                var empleadoRepo = new EmpleadoRepositorio(inicializador);
                var fichajeRepo = new FichajeRepositorio(inicializador);

                // 3. Instanciar servicios (Capa Logica de negocio)
                var empleadoServicio = new EmpleadoServicio(empleadoRepo);
                var fichajeServicio = new FichajeServicio(fichajeRepo, empleadoRepo);

                // 4. Instanciar UI (Capa de Presentacion)
                var EmpleadoUI = new EmpleadoUI(empleadoServicio);
                var fichajeUI = new FichajeUI(fichajeServicio, empleadoServicio);
                var reporteUI = new ReporteUI(fichajeServicio);

                // 5. Mostrar menu principal
                MostrarMenuPrincipal(EmpleadoUI, fichajeUI, reporteUI);
            }
            catch (SqliteException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n ERROR CRITICO DE BASE DE DATOS:");
                Console.WriteLine($"  {ex.Message}");
                Console.WriteLine("\n  Varifique que la aplicacion tenga permisos de escritura.");
                Console.ResetColor();
                Console.WriteLine("\n Presione cualquier tecla para salir...");
                Console.ReadKey(true);
                Environment.Exit(1);
            }
            catch (InvalidOperationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n  ERROR DE INICIALIZACION:");
                Console.WriteLine($"  {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("\n  Presione cualquier tecla para salir...");
                Console.ReadKey(true);
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n  ERROR INESPERADO:");
                Console.WriteLine($"  {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("\n  Presione cualquier tecla para salir...");
                Console.ReadKey(true);
                Environment.Exit(1);
            }
        }

        //Menu principal.
        private static void MostrarMenuPrincipal(
            EmpleadoUI empleadoUI,
            FichajeUI fichajeUI,
            ReporteUI reporteUI)
        {
            bool ejecutando = true;

            while (ejecutando)
            {
                ConsolaHelper.LimpiarPantalla();
                ConsolaHelper.MostrarTitulo("MENU PRINCIPAL");

                Console.WriteLine("  [1]  Gestion de Empleados");
                Console.WriteLine("  [2]  Registro de Fichaje (Ponchado)");
                Console.WriteLine("  [3]  Reporte Mensual de Horas");
                ConsolaHelper.MostrarSeparador();
                Console.WriteLine("  [0]  Salir del sistema");
                ConsolaHelper.MostrarSeparador();

                Console.Write("  Seleccione una opcion: ");
                var opcion = Console.ReadLine()?.Trim();

                switch (opcion)
                {
                    case "1":
                        empleadoUI.MostrarMenu();
                        break;
                    case "2":
                        fichajeUI.MostrarMenu();
                        break;
                    case "3":
                        reporteUI.MostrarMenu();
                        break;
                    case "0":
                        if (ConsolaHelper.Confirmar("¿Desea salir del sistema?"))
                        {
                            ConsolaHelper.LimpiarPantalla();
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("\n  Gracias por usar TempoControl.");
                            Console.WriteLine("  Innovatech Solutions, S.R.L.\n");
                            Console.ResetColor();
                            ejecutando = false;
                        }
                        break;

                    default:
                        ConsolaHelper.MostrarAdvertencia("Opcion no valida. Intente de nuevo.");
                        ConsolaHelper.EsperarTecla();
                        break;
                }
            }
        }
    }
}
