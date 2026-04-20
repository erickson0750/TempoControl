using TempoControl.Dominio;
using TempoControl.Repositorio.Interfaces;

namespace TempoControl.LogicaNegocio
{
    /// <summary>
    /// Servicio de logica de negocio para el fichaje y generacion de reportes.
    /// Capa: Logica de Negocio (aqui vive e calculo de horas y las reglas del ciclo de vida entrada/salida).
    /// </summary>
    public class FichajeServicio
    {
        private readonly IFichajeRepositorio _fichajeRepo;
        private readonly IEmpleadoRepositorio _empleadoRepo;

        public FichajeServicio(
            IFichajeRepositorio fichajeRepo, IEmpleadoRepositorio empleadoRepo)
        {
            _fichajeRepo  = fichajeRepo ?? throw new 
            ArgumentNullException(nameof(fichajeRepo));

            _empleadoRepo = empleadoRepo ?? throw new ArgumentNullException(nameof(empleadoRepo));
        }

        //Fichaje
        public void RegistrarEntrada(int empleadoId)
        {
            var empleado = ObtenerEmpleadoActivo(empleadoId);

            //Verificar si ya tiene un fichaje abierto
            var fichajeAbierto = _fichajeRepo.ObtenerFichajeAbierto(empleadoId);
            if (fichajeAbierto != null)
                throw new InvalidOperationException(
                    $"'{empleado.NombreCompleto}' ya tiene una entrada registrada el " +
                    $"{fichajeAbierto.HoraEntrada:dd/MM/yyyy} a las " +
                    $"{fichajeAbierto.HoraEntrada:HH:mm}. " +
                    $"Debe registrar la salida primero.");

            _fichajeRepo.RegistrarEntrada(empleadoId, DateTime.Now);
        }

        /// <summary>
        /// Registra la salida de un empleado.
        /// Valida que exista un fichaje abierto previamente.
        /// </summary>
        public RegistroFichaje RegistrarSalida(int empleadoId)
        {
            var empleado = ObtenerEmpleadoActivo(empleadoId);

            var fichajeAbierto = _fichajeRepo.ObtenerFichajeAbierto(empleadoId);
            if (fichajeAbierto == null)
                throw new InvalidOperationException(
                    $"'{empleado.NombreCompleto}' no tiene una entrada abierta. " +
                    $"Registre primero la entrada.");

            var ahora = DateTime.Now;

            if (ahora <= fichajeAbierto.HoraEntrada)
                throw new InvalidOperationException(
                    "La hora de salida no puede ser anterior o igual a la hora de entrada.");

            bool exito = _fichajeRepo.RegistrarSalida(empleadoId, ahora);
            if (!exito)
                throw new InvalidOperationException(
                    "No se pudo registrar la salida. Intente de nuevo.");

            // Retornar el registro completado para mostrar el resumen al usuario
            fichajeAbierto.HoraSalida = ahora;
            return fichajeAbierto;
        }

        /// <summary>
        /// Obtiene el fichaje abierto actual de un empleado.
        /// Retorna null si no tiene entrada activa.
        /// </summary>
        public RegistroFichaje? ObtenerFichajeAbierto(int empleadoId)
            => _fichajeRepo.ObtenerFichajeAbierto(empleadoId);

        /// <summary>Obtiene el historial de fichajes de un empleado.</summary>
        public IEnumerable<RegistroFichaje> ObtenerHistorial(int empleadoId, int cantidad = 10)
        {
            ObtenerEmpleadoActivo(empleadoId); // Valida existencia
            return _fichajeRepo.ObtenerUltimos(empleadoId, cantidad);
        }

        //Reporte Mensual

        /// <summary>
        /// Genera el reporte mensual de horas trabajadas por todos los empleados.
        /// Agrupa los fichajes por empleado y calcula dias y horas totales.
        /// Solo incluye empleados con al menos un fichaje completo en el mes.
        /// </summary>
        public IEnumerable<ReporteEmpleado> GenerarReporteMensual(int mes, int anio)
        {
            ValidarMesAnio(mes, anio);

            var fichajes = _fichajeRepo.ObtenerPorMes(mes, anio).ToList();

            if (!fichajes.Any())
                return Enumerable.Empty<ReporteEmpleado>();

            // Agrupar por empleado y calcular totales
            var reporte = fichajes
                .GroupBy(f => f.EmpleadoId)
                .Select(grupo =>
                {
                    var empleado = _empleadoRepo.ObtenerPorId(grupo.Key);

                    // Días únicos trabajados (una entrada por día = un día)
                    int diasTrabajados = grupo
                        .Select(f => f.HoraEntrada.Date)
                        .Distinct()
                        .Count();

                    // Suma de horas de todos los fichajes completos del mes
                    double totalHoras = grupo
                        .Where(f => f.EstaCompleto)
                        .Sum(f => f.HorasTrabajadas ?? 0);

                    return new ReporteEmpleado
                    {
                        EmpleadoId            = grupo.Key,
                        NombreEmpleado        = empleado?.NombreCompleto
                                                ?? grupo.First().NombreEmpleado,
                        Departamento          = empleado?.Departamento ?? "—",
                        TotalDiasTrabajados   = diasTrabajados,
                        TotalHorasTrabajadas  = totalHoras
                    };
                })
                .OrderByDescending(r => r.TotalHorasTrabajadas)
                .ToList();

            return reporte;
        }

        //Utilidades privadas

        private Empleado ObtenerEmpleadoActivo(int empleadoId)
        {
            if (empleadoId <= 0)
                throw new ArgumentException(
                    "El ID del empleado debe ser mayor a cero.");

            var empleado = _empleadoRepo.ObtenerPorId(empleadoId)
                ?? throw new KeyNotFoundException(
                    $"No se encontró ningún empleado con ID {empleadoId}.");

            if (!empleado.Activo)
                throw new InvalidOperationException(
                    $"El empleado '{empleado.NombreCompleto}' está inactivo " +
                    $"y no puede realizar fichajes.");

            return empleado;
        }

        private static void ValidarMesAnio(int mes, int anio)
        {
            if (mes < 1 || mes > 12)
                throw new ArgumentOutOfRangeException(
                    nameof(mes), "El mes debe estar entre 1 y 12.");

            if (anio < 2000 || anio > DateTime.Now.Year)
                throw new ArgumentOutOfRangeException(
                    nameof(anio),
                    $"El año debe estar entre 2000 y {DateTime.Now.Year}.");
        }
    }
}