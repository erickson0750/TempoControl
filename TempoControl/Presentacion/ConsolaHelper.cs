using System.Drawing;
using System.Globalization;

namespace TempoControl.Presentacion
{
    public static class ConsolaHelper
    {
        //Mensaje con color.
        public static void MostrarTitulo(string texto)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($" {texto,-44}");
            Console.ResetColor();
        }

        public static void MostrarSubtitulo(string texto)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"\n  ── {texto} ──");
            Console.ResetColor();
        }

        public static void MostrarExito(string mensaje)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n  Exito {mensaje}");
            Console.ResetColor();
        }

        public static void MostrarError(string mensaje)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n  ERROR: {mensaje}");
            Console.ResetColor();
        }

        public static void MostrarAdvertencia(string mensaje)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n  Advertencia {mensaje}");
            Console.ResetColor();
        }

        public static void MostrarInfo(string mensaje)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"  {mensaje}");
            Console.ResetColor();
        }

        public static void MostrarSeparador()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + new string('─', 60));
            Console.ResetColor();
        }

        // Lectura de entrada

        ///<summary> Lee una cadena de texto no vacia del usuario.</summary>
        public static string LeerTexto(string etiqueta, bool requerido = true)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"  {etiqueta}: ");
                Console.ResetColor();

                var valor = Console.ReadLine()?.Trim() ?? string.Empty;

                if (!requerido || !string.IsNullOrWhiteSpace(valor))
                    return valor;

                MostrarAdvertencia("Este campo es obligatorio. Intente de nuevo");
            }
        }

        /// <summary>Lee un número entero dentro de un rango válido.<summary>
        public static int LeerEntero(string etiqueta, int min = 1, int max = int.MaxValue)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"  {etiqueta}: ");
                Console.ResetColor();

                var entrada = Console.ReadLine()?.Trim();

                if (int.TryParse(entrada, out int valor) && valor >= min && valor <= max)
                    return valor;

                MostrarAdvertencia($"Ingrese un numero entero entre {min} y {max}.");
            }
        }

        /// <summary>Lee un número de mes valido (1-12).</summary>
        public static int LeerMes(string etiqueta = "Mes (1-12)")
            => LeerEntero(etiqueta, 1, 12);

        /// <summary>Lee un año valido.</summary>
        public static int LeerAnio(string etiqueta = "Año")
            => LeerEntero(etiqueta, 2000, DateTime.Now.Year);

        /// <summary>Pregunta confirmacion al usuario (S/N).</summary>
        public static bool Confirmar(string pregunta)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"\n  {pregunta} (S/N): ");
            Console.ResetColor();

            var respuesta = Console.ReadLine()?.Trim().ToUpper();
            return respuesta == "S";
        }

        /// <summary>Pausa la ejecucion hasta que el usuario presione una tecla.</summary>
        public static void EsperarTecla(string mensaje = "Presione cualquier tecla para continuar...")
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"\n  {mensaje}");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        /// <summary>Limpia la pantalla y muestra el encabezado de la aplicación.</summary>
        public static void LimpiarPantalla()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
            TEMPO
        CONTROL - Sistema de Fichaje de Empleados");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  Innovatech Solutions, S.R.L.  |  " +
                              $"{DateTime.Now:dd/MM/yyyy  HH:mm:ss}");
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}