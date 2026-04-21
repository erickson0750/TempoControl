using TempoControl.LogicaNegocio;

namespace TempoControl.Presentacion
{
    /// <summary>
    /// Menu de presentacion para el fichaje (ponchado) de empleados.
    /// Capa: Presentacion - solo interaccion con el usuario
    /// </summary>
    public class FichajeUI
    {
        private readonly FichajeServicio  _fichajeServicio;
        private readonly EmpleadoServicio _empleadoServicio;

        public FichajeUI(FichajeServicio fichajeServicio, EmpleadoServicio empleadoServicio)
        {
            _fichajeServicio  = fichajeServicio;
            _empleadoServicio = empleadoServicio;
        }

        public void MostrarMenu()
        {
            bool continuar = true;
            while (continuar)
            {
                ConsolaHelper.LimpiarPantalla();
                ConsolaHelper.MostrarTitulo("REGISTRO DE FICHAJE (PONCHADO)");

                Console.WriteLine("  [1] Registrar ENTRADA");
                Console.WriteLine("  [2] Registrar SALIDA");
                Console.WriteLine("  [3] Ver historial de fichajes");
                Console.WriteLine("  [4] Ver estado actual de fichaje");
                Console.WriteLine("  [0] Volver al menu principal");
                ConsolaHelper.MostrarSeparador();

                Console.Write("  Seleccione una opcion: ");
                var opcion = Console.ReadLine()?.Trim();

                switch (opcion)
                {
                    case "1": RegistrarEntrada(); break;
                    case "2": RegistrarSalida();  break;
                    case "3": VerHistorial();     break;
                    case "4": VerEstadoActual();  break;
                    case "0": continuar = false;  break;
                    default:
                        ConsolaHelper.MostrarAdvertencia("Opcion no valida.");
                        ConsolaHelper.EsperarTecla();
                        break;
                }
            }
        }

        // Opciones del menu
        private void RegistrarEntrada()
        {
            ConsolaHelper.LimpiarPantalla();
            ConsolaHelper.MostrarTitulo("REGISTRAR ENTRADA");

            try
            {
                MostrarEmpleadosActivos();
                var id = ConsolaHelper.LeerEntero("ID del empleado", 1);

                ConsolaHelper.MostrarInfo(
                    $"Hora de entrada: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");

                if (!ConsolaHelper.Confirmar("¿Confirma el registro de ENTRADA?"))
                {
                    ConsolaHelper.MostrarAdvertencia("Operación cancelada.");
                    ConsolaHelper.EsperarTecla();
                    return;
                }

                _fichajeServicio.RegistrarEntrada(id);
                ConsolaHelper.MostrarExito(
                    $"Entrada registrada correctamente a las {DateTime.Now:HH:mm:ss}.");
            }
            catch (KeyNotFoundException ex)
            {
                ConsolaHelper.MostrarError(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ConsolaHelper.MostrarError(ex.Message);
            }
            catch (ArgumentException ex)
            {
                ConsolaHelper.MostrarError(ex.Message);
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

        private void RegistrarSalida()
        {
            ConsolaHelper.LimpiarPantalla();
            ConsolaHelper.MostrarTitulo("REGISTRAR SALIDA");

            try
            {
                MostrarEmpleadosActivos();
                var id = ConsolaHelper.LeerEntero("ID del empleado", 1);

                // Mostrar resumen del fichaje abierto antes de confirmar
                var abierto = _fichajeServicio.ObtenerFichajeAbierto(id);
                if (abierto == null)
                {
                    ConsolaHelper.MostrarError(
                        "El empleado no tiene un fichaje de entrada abierto.");
                    ConsolaHelper.EsperarTecla();
                    return;
                }

                var duracion = DateTime.Now - abierto.HoraEntrada;
                ConsolaHelper.MostrarInfo(
                    $"Entrada registrada : {abierto.HoraEntrada:dd/MM/yyyy HH:mm}");
                ConsolaHelper.MostrarInfo(
                    $"Hora de salida     : {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                ConsolaHelper.MostrarInfo(
                    $"Tiempo trabajado   : {(int)duracion.TotalHours}h {duracion.Minutes}m");

                if (!ConsolaHelper.Confirmar("¿Confirma el registro de SALIDA?"))
                {
                    ConsolaHelper.MostrarAdvertencia("Operacion cancelada.");
                    ConsolaHelper.EsperarTecla();
                    return;
                }

                var registro = _fichajeServicio.RegistrarSalida(id);
                ConsolaHelper.MostrarExito(
                    $"Salida registrada. Horas trabajadas: {registro.HorasTrabajadas:F2}h");
            }
            catch (KeyNotFoundException ex)
            {
                ConsolaHelper.MostrarError(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ConsolaHelper.MostrarError(ex.Message);
            }
            catch (ArgumentException ex)
            {
                ConsolaHelper.MostrarError(ex.Message);
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

        private void VerHistorial()
        {
            ConsolaHelper.LimpiarPantalla();
            ConsolaHelper.MostrarTitulo("HISTORIAL DE FICHAJES");

            try
            {
                MostrarEmpleadosActivos();
                var id       = ConsolaHelper.LeerEntero("ID del empleado", 1);
                var cantidad = ConsolaHelper.LeerEntero(
                    "Cantidad de registros a mostrar", 1, 100);

                var emp      = _empleadoServicio.ObtenerPorId(id);
                var fichajes = _fichajeServicio.ObtenerHistorial(id, cantidad).ToList();

                ConsolaHelper.MostrarSubtitulo(
                    $"Ultimos {cantidad} fichajes de {emp.NombreCompleto}");

                if (!fichajes.Any())
                {
                    ConsolaHelper.MostrarAdvertencia(
                        "No se encontraron fichajes para este empleado.");
                    ConsolaHelper.EsperarTecla();
                    return;
                }

                ConsolaHelper.MostrarSeparador();
                int i = 1;
                foreach (var f in fichajes)
                {
                    string estado = f.EstaCompleto ? "Exito" : " EN CURSO";
                    ConsolaHelper.MostrarInfo($"#{i++,3} | {f} | {estado}");
                }
                ConsolaHelper.MostrarSeparador();
            }
            catch (KeyNotFoundException ex)
            {
                ConsolaHelper.MostrarError(ex.Message);
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

        private void VerEstadoActual()
        {
            ConsolaHelper.LimpiarPantalla();
            ConsolaHelper.MostrarTitulo("ESTADO ACTUAL DE FICHAJE");

            try
            {
                MostrarEmpleadosActivos();
                var id  = ConsolaHelper.LeerEntero("ID del empleado", 1);
                var emp = _empleadoServicio.ObtenerPorId(id);

                var abierto = _fichajeServicio.ObtenerFichajeAbierto(id);
                ConsolaHelper.MostrarSeparador();
                ConsolaHelper.MostrarInfo($"Empleado: {emp.NombreCompleto}");

                if (abierto != null)
                {
                    var duracion = DateTime.Now - abierto.HoraEntrada;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n    EN JORNADA");
                    Console.ResetColor();
                    ConsolaHelper.MostrarInfo(
                        $"Entrada  : {abierto.HoraEntrada:dd/MM/yyyy HH:mm:ss}");
                    ConsolaHelper.MostrarInfo(
                        $"Transcurrido: {(int)duracion.TotalHours}h {duracion.Minutes}m {duracion.Seconds}s");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"\n    FUERA DE JORNADA (sin entrada abierta)");
                    Console.ResetColor();
                }
                ConsolaHelper.MostrarSeparador();
            }
            catch (KeyNotFoundException ex)
            {
                ConsolaHelper.MostrarError(ex.Message);
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

        //Helper visual
        private void MostrarEmpleadosActivos()
        {
            var activos = _empleadoServicio.ObtenerActivos().ToList();

            if (!activos.Any())
            {
                ConsolaHelper.MostrarAdvertencia(
                    "No hay empleados activos en el sistema.");
                return;
            }

            ConsolaHelper.MostrarSubtitulo("Empleados activos");
            foreach (var e in activos)
                ConsolaHelper.MostrarInfo($"[{e.Id:D3}] {e.NombreCompleto} — {e.Departamento}");
            ConsolaHelper.MostrarSeparador();
        }
    }
}