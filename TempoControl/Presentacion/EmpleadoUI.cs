using TempoControl.LogicaNegocio;
using TempoControl.Dominio;

namespace TempoControl.Presentacion
{
    /// <summary>
    /// Menu de presentacion para la gestion de Empleados.
    /// Capa: Presentacion — solo interaccion con el usuario,
    /// delega toda la logica al servicio.
    /// </summary>
    public class EmpleadoUI
    {
        private readonly EmpleadoServicio _servicio;

        public EmpleadoUI(EmpleadoServicio servicio)
        {
            _servicio = servicio;
        }

        public void MostrarMenu()
        {
            bool continuar = true;
            while (continuar)
            {
                ConsolaHelper.LimpiarPantalla();
                ConsolaHelper.MostrarTitulo("GESTION DE EMPLEADOS");

                Console.WriteLine("  [1] Crear nuevo empleado");
                Console.WriteLine("  [2] Listar todos los empleados");
                Console.WriteLine("  [3] Buscar empleado por ID");
                Console.WriteLine("  [4] Actualizar datos de empleado");
                Console.WriteLine("  [5] Desactivar empleado");
                Console.WriteLine("  [0] Volver al menu principal");
                ConsolaHelper.MostrarSeparador();

                Console.Write("  Seleccione una opcion: ");
                var opcion = Console.ReadLine()?.Trim();

                switch (opcion)
                {
                    case "1": CrearEmpleado(); break;
                    case "2": ListarEmpleados(); break;
                    case "3": BuscarEmpleado(); break;
                    case "4": ActualizarEmpleado(); break;
                    case "5": DesactivarEmpleado(); break;
                    case "0": continuar = false; break;
                    default:
                        ConsolaHelper.MostrarAdvertencia("Opcion no valida. Intente de nuevo.");
                        ConsolaHelper.EsperarTecla();
                        break;
                }
            }
        }

        // Opciones del menu

        private void CrearEmpleado()
        {
            ConsolaHelper.LimpiarPantalla();
            ConsolaHelper.MostrarTitulo("CREAR NUEVO EMPLEADO");

            try
            {
                var nombre = ConsolaHelper.LeerTexto("Nombre completo");
                var depto = ConsolaHelper.LeerTexto("Departamento");
                var posic = ConsolaHelper.LeerTexto("Posicion / Cargo");

                if (!ConsolaHelper.Confirmar($"¿Confirma crear al empleado '{nombre}'?"))
                {
                    ConsolaHelper.MostrarAdvertencia("Operacion cancelada.");
                    ConsolaHelper.EsperarTecla();
                    return;
                }

                var emp = _servicio.CrearEmpleado(nombre, depto, posic);
                ConsolaHelper.MostrarExito($"Empleado creado correctamente con ID: {emp.Id}");
            }
            catch (ArgumentException ex)
            {
                ConsolaHelper.MostrarError($"Datos invalidos: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                ConsolaHelper.MostrarError($"Error de operacion: {ex.Message}");
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

        private void ListarEmpleados()
        {
            ConsolaHelper.LimpiarPantalla();
            ConsolaHelper.MostrarTitulo("LISTA DE EMPLEADOS");

            try
            {
                var empleados = _servicio.ObtenerTodos().ToList();

                if (!empleados.Any())
                {
                    ConsolaHelper.MostrarAdvertencia("No hay empleados registrados.");
                    ConsolaHelper.EsperarTecla();
                    return;
                }

                ConsolaHelper.MostrarInfo($"Total registrados: {empleados.Count}");
                ConsolaHelper.MostrarSeparador();

                var activos = empleados.Where(e => e.Activo).ToList();
                var inactivos = empleados.Where(e => !e.Activo).ToList();

                if (activos.Any())
                {
                    ConsolaHelper.MostrarSubtitulo("ACTIVOS");
                    foreach (var e in activos)
                        ConsolaHelper.MostrarInfo(e.ToString());
                }

                if (inactivos.Any())
                {
                    ConsolaHelper.MostrarSubtitulo("INACTIVOS");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    foreach (var e in inactivos)
                        Console.WriteLine($"  {e}");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                ConsolaHelper.MostrarError($"No se pudo obtener la lista: {ex.Message}");
            }
            finally
            {
                ConsolaHelper.EsperarTecla();
            }
        }

        private void BuscarEmpleado()
        {
            ConsolaHelper.LimpiarPantalla();
            ConsolaHelper.MostrarTitulo("BUSCAR EMPLEADO POR ID");

            try
            {
                var id = ConsolaHelper.LeerEntero("ID del empleado", 1);
                var emp = _servicio.ObtenerPorId(id);
                MostrarDetalleEmpleado(emp);
            }
            catch (KeyNotFoundException ex)
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

        private void ActualizarEmpleado()
        {
            ConsolaHelper.LimpiarPantalla();
            ConsolaHelper.MostrarTitulo("ACTUALIZAR DATOS DE EMPLEADO");

            try
            {
                var id = ConsolaHelper.LeerEntero("ID del empleado a actualizar", 1);
                var emp = _servicio.ObtenerPorId(id);

                ConsolaHelper.MostrarInfo($"Empleado actual: {emp}");
                ConsolaHelper.MostrarInfo("(Presione Enter para mantener el valor actual)");
                ConsolaHelper.MostrarSeparador();

                Console.Write($"  Nombre completo [{emp.NombreCompleto}]: ");
                var nombre = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(nombre)) nombre = emp.NombreCompleto;

                Console.Write($"  Departamento [{emp.Departamento}]: ");
                var depto = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(depto)) depto = emp.Departamento;

                Console.Write($"  Posicion [{emp.Posicion}]: ");
                var posic = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(posic)) posic = emp.Posicion;

                if (!ConsolaHelper.Confirmar("¿Confirma los cambios?"))
                {
                    ConsolaHelper.MostrarAdvertencia("Actualizacion cancelada.");
                    ConsolaHelper.EsperarTecla();
                    return;
                }

                _servicio.ActualizarEmpleado(id, nombre, depto, posic);
                ConsolaHelper.MostrarExito("Empleado actualizado correctamente.");
            }
            catch (KeyNotFoundException ex)
            {
                ConsolaHelper.MostrarError(ex.Message);
            }
            catch (ArgumentException ex)
            {
                ConsolaHelper.MostrarError($"Datos invalidos: {ex.Message}");
            }
            catch (InvalidOperationException ex)
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

        private void DesactivarEmpleado()
        {
            ConsolaHelper.LimpiarPantalla();
            ConsolaHelper.MostrarTitulo("DESACTIVAR EMPLEADO");

            try
            {
                var id = ConsolaHelper.LeerEntero("ID del empleado a desactivar", 1);
                var emp = _servicio.ObtenerPorId(id);

                ConsolaHelper.MostrarInfo($"Empleado: {emp}");
                ConsolaHelper.MostrarAdvertencia(
                    "Esta accion marcará al empleado como INACTIVO.");
                ConsolaHelper.MostrarInfo(
                    "Sus registros historicos se conservaran.");

                if (!ConsolaHelper.Confirmar($"¿Confirma desactivar a '{emp.NombreCompleto}'?"))
                {
                    ConsolaHelper.MostrarAdvertencia("Operacion cancelada.");
                    ConsolaHelper.EsperarTecla();
                    return;
                }

                _servicio.DesactivarEmpleado(id);
                ConsolaHelper.MostrarExito(
                    $"El empleado '{emp.NombreCompleto}' ha sido desactivado.");
            }
            catch (KeyNotFoundException ex)
            {
                ConsolaHelper.MostrarError(ex.Message);
            }
            catch (InvalidOperationException ex)
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

        // Helper visual
        private static void MostrarDetalleEmpleado(Empleado emp)
        {
            ConsolaHelper.MostrarSeparador();
            ConsolaHelper.MostrarInfo($"ID            : {emp.Id}");
            ConsolaHelper.MostrarInfo($"Nombre        : {emp.NombreCompleto}");
            ConsolaHelper.MostrarInfo($"Departamento  : {emp.Departamento}");
            ConsolaHelper.MostrarInfo($"Posicion      : {emp.Posicion}");
            ConsolaHelper.MostrarInfo($"Estado        : {(emp.Activo ? " Activo" : " Inactivo")}");
            ConsolaHelper.MostrarInfo($"Fecha registro: {emp.FechaRegistro:dd/MM/yyyy HH:mm}");
            ConsolaHelper.MostrarSeparador();
        }
    }
}