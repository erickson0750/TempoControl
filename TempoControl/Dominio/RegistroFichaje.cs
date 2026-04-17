namespace TempoControl.Dominio
{
    /// <summary>
    /// Entidad que representa un registro de entrada o salida de un empleado.
    /// capa: Dominio - sin dependencias externas.
    /// </summary>
    public class RegistroFichaje
    {
        public int Id {get; set; }
        public int EmpleadoId {get; set; }
        public DateTime HoraEntrada {get; set; }
        public DateTime? HoraSalida {get; set; }

        //Propiedad de navegacion (no se guarda en BD directamente).
        public string NombreEmpleado {get; set; } = string.Empty;

        /// <summary>
        /// Indica si el fichaje tiene entrada y salida registradas.
        /// </summary>
        public bool EstaCompleto => HoraSalida.HasValue;

        /// <summary>
        /// Calcula las horas trabajadas. Retorna null si aun no hay salida.
        /// </summary>
        public double? HorasTrabajadas =>
            HoraSalida.HasValue
                ? (HoraSalida.Value - HoraEntrada).TotalHours
                : null;

        public override string ToString()
        {
            string entrada = HoraEntrada.ToString("dd/MM/yyyy HH:mm");
            string salida = HoraSalida.HasValue
                ? HoraSalida.Value.ToString("dd/MM/yyyy HH:mm")
                : "Pendiente";
            string horas = HorasTrabajadas.HasValue
                ? $"{HorasTrabajadas.Value:F2}h"
                : "En curso";
            return $"Entrada: {entrada} | Salida: {salida} | Horas: {horas}";
        }
    }
}