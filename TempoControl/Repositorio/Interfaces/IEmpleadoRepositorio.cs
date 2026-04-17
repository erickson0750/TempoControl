using TempoControl.Dominio;

namespace TempoControl.Repositorio.Interfaces
{
    /// <summary>
    /// Contrato del repositorio de empleados.
    /// Define las operaciones de persitencia sin importa la BD usada.
    /// </summary>
    public interface IEmpleadoRepositorio
    {
        /// <summary> Registra un nuevo empleado en el sistema.</summary>
        void Crear(Empleado empleado);

        /// <summary> Obtine todos los empleados (activos o inactivos) </summary>
        IEnumerable<Empleado> ObtenerTodos();

        /// <summary> Obtiene solo los empleados activos </summary>
        IEnumerable<Empleado> ObtenerActivos();

        /// <summary> Busca un empleado por su id. rotorna null si no existe. </summary>
        Empleado? ObtenerPorId(int id);

        /// <summary> Actualiza los datos de un empleado existente </summary>
        bool Actualizar(Empleado empleado);

        /// <summary>
        /// Desactiva un empleado (baja logica)
        /// Nunca se elimina el registro por intregidad historica
        /// </summary>
        bool Desactivar(int id);
    }
}