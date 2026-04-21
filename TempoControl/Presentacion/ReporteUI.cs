using TempoControl.LogicaNegocio;
using TempoControl.Dominio;

namespace TempoControl.Presentacion
{
    public class ReporteUI
    {
        private readonly FichajeServicio _servicio;

        public ReporteUI(FichajeServicio servicio)
        {
            _servicio = servicio;
        }

        public void MostrarMenu()
        {
            bool continuar = true;
            while (continuar)
            {
                ConsolaHelper.LimpiarPantalla();
                ConsolaHelper.MostrarTitulo("REPORTE MENSUAL DE HORAS");

                Console.WriteLine("  [1] Generar reporte del mes actual");
                Console.WriteLine("  [2] Generar reporte de un mes especifico");
                Console.WriteLine("  [0] Volver al menu principal");
                ConsolaHelper.MostrarSeparador();

                Console.Write("  Seleccione una opcion: ");
                var opcion = Console.ReadLine()?.Trim();

                switch (opcion)
                {
                    case "1":
                        GenerarReporte(DateTime.Now.Month, DateTime.Now.Year);
                        break;
                    case "2":
                        SolicitarMesYGenerar();
                        break;
                    case "0":
                        continuar = false;
                        break;
                    default:
                        ConsolaHelper.MostrarAdvertencia("Opcion no valida.");
                        ConsolaHelper.EsperarTecla();
                        break;
                }
            }
        }

        // Opciones del menu
        private void SolicitarMesYGenerar()
        {
            ConsolaHelper.LimpiarPantalla();
            ConsolaHelper.MostrarTitulo("SELECCIONAR PERIODO DEL REPORTE");

            try
            {
                int mes = ConsolaHelper.LeerMes();
                int anio = ConsolaHelper.LeerAnio();
                GenerarReporte(mes, anio);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                ConsolaHelper.MostrarError(ex.Message);
                ConsolaHelper.EsperarTecla();
            }
        }

        private void GenerarReporte(int mes, int anio)
        {
            ConsolaHelper.LimpiarPantalla();

            string nombreMes = new DateTime(anio, mes, 1).ToString("MMMM yyyy");
            ConsolaHelper.MostrarTitulo($"REPORTE: {nombreMes.ToUpper()}");

            try
            {
                var reporte = _servicio.GenerarReporteMensual(mes, anio).ToList();

                if (!reporte.Any())
                {
                    ConsolaHelper.MostrarAdvertencia($"No se encontraron fichajes completados para {nombreMes}.");
                    ConsolaHelper.EsperarTecla();
                    return;
                }

                ImprimirEncabezado();
                ImprimirFilas(reporte);
                ImprimirResumen(reporte);

                //Opcion de exportar a archivos de texto
                if (ConsolaHelper.Confirmar("¿Desea exportar el reporte a un archivo .txt?"))
                    ExportarReporte(reporte, mes, anio, nombreMes);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                ConsolaHelper.MostrarError($"Periodo invalido: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                ConsolaHelper.MostrarError($"Error al generar reporte: {ex.Message}");
            }
            catch (Exception ex)
            {
                ConsolaHelper.MostrarError($"Error inesperado: {ex.Message}");
            }
            finally
            {
                ConsolaHelper.EsperarTecla();
            }
        }

        //Renderizado del reporte
        private static void ImprimirEncabezado()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n  {"EMPLEADO",-30} | {"DEPARTAMENTO",-20} | {"DIAS",5} | {"HORAS",8}");
            Console.WriteLine("  " + new string('═', 72));
            Console.ResetColor();
        }

        private static void ImprimirFilas(List<ReporteEmpleado> reporte)
        {
            bool alternar = false;
            foreach (var fila in reporte)
            {
                Console.ForegroundColor = alternar ? ConsoleColor.White : ConsoleColor.Gray;
                Console.WriteLine(
                    $"  {fila.NombreEmpleado,-30} | " +
                    $"{fila.Departamento,-20} | " +
                    $"{fila.TotalDiasTrabajados,5} | " +
                    $"{fila.TotalHorasTrabajadas,7:F2}h");
                alternar = !alternar;
            }
            Console.ResetColor();
        }

        private static void ImprimirResumen(List<ReporteEmpleado> reporte)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  " + new string('═', 72));
            Console.ResetColor();

            double totalHoras = reporte.Sum(r => r.TotalHorasTrabajadas);
            double promedioHoras = totalHoras / reporte.Count;

            ConsolaHelper.MostrarInfo($"Total empleados con registros : {reporte.Count}");
            ConsolaHelper.MostrarInfo($"Total horas trabajadas        : {totalHoras:F2}h");
            ConsolaHelper.MostrarInfo($"Promedio horas por empleado   : {promedioHoras:F2}h");

            var top = reporte.OrderByDescending(r => r.TotalHorasTrabajadas).First();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                $"\n  Mayor cantidad de horas: {top.NombreEmpleado}" + $"({top.TotalHorasTrabajadas:F2}h)");
            Console.ResetColor();

            ConsolaHelper.MostrarSeparador();
        }

        //Exportacion a archivo
        private static void ExportarReporte(List<ReporteEmpleado> reporte, int mes, int anio, string nombreMes)
        {
            try
            {
                string nombreArchivo = $"Reporte_{anio}_{mes:D2}.txt";

                using var writer = new StreamWriter(nombreArchivo, false, System.Text.Encoding.UTF8);

                writer.WriteLine(new string('═', 75));
                writer.WriteLine("    INNOVATECH SOLUTIONS, S.R.L. — TEMPOCONTROL");
                writer.WriteLine($"    REPORTE MENSUAL DE HORAS — {nombreMes.ToUpper()}");
                writer.WriteLine($"    Generado el: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                writer.WriteLine(new string('═', 75));
                writer.WriteLine();
                writer.WriteLine(
                    $"  {"EMPLEADO",-30} | {"DEPARTAMENTO",-20} | {"DIAS",5} | {"HORAS",8}");
                writer.WriteLine("  " + new string('─', 72));

                foreach (var fila in reporte)
                    writer.WriteLine(
                        $"  {fila.NombreEmpleado,-30} | " +
                        $"{fila.Departamento,-20} | " +
                        $"{fila.TotalDiasTrabajados,5} | " +
                        $"{fila.TotalHorasTrabajadas,7:F2}h");

                writer.WriteLine("  " + new string('─', 72));
                writer.WriteLine(
                    $"  Total empleados : {reporte.Count}");
                writer.WriteLine(
                    $"  Total horas     : {reporte.Sum(r => r.TotalHorasTrabajadas):F2}h");
                writer.WriteLine(
                    $"  Promedio        : {reporte.Average(r => r.TotalHorasTrabajadas):F2}h");
                writer.WriteLine();
                writer.WriteLine("  FIN DEL REPORTE");

                ConsolaHelper.MostrarExito(
                    $"Reporte exportado correctamente como: {nombreArchivo}");
            }
            catch (IOException ex)
            {
                ConsolaHelper.MostrarError(
                    $"No se pudo exportar el archivo: {ex.Message}");
            }
        }
    }
}