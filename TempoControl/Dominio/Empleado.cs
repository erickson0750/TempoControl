namespace TempoControl.Dominio
{
    /// <summary>
    /// Entidad que representa un Empleado de la empresa
    /// capa: Dominio - sin dependencias externas
    /// </summary>
    public class Empleado
    {
        public int Id {get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Departamento {get; set; } = string.Empty;
        public string Posicion {get; set; } = string.Empty;
        public bool Activo {get; set; } = true;
        public DateTime FechaRegistro {get; set; } = DateTime.Now;

        public override string ToString()
        {
            string estado = Activo ? "Activo" : "Inactivo";
            return $"[{Id:D3}] {NombreCompleto} | {Departamento} | {Posicion} | {estado}";
        }
    }
}