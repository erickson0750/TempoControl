namespace TempoControl.Dominio
{
    /// <summary>
    /// Modelo de resultado para el reporte mensual de horas.
    /// Capa: Dominio - calculado por la capa de negocio
    /// </summary>
    public class ReporteEmpleado
    {
        public int Empleado {get; set; }
        public string NombreEmpleado {get; set; } = string.Empty;
        public string Departamento {get; set; } = string.Empty;
        public int TotalDiasTrabajados {get; set; }
        public double TotalHorasTrabajadas {get; set; }

        public override string ToString()
        {
            return $"{NombreEmpleado,-30} | {Departamento} |" + $"Dias: {TotalDiasTrabajados,3} | Horas: {TotalHorasTrabajadas,7:F2}";
        }
    }
}