using TempoControl.Dominio;

namespace TempoControl.Repositorio.Interfaces
{
    /// <summary>
    /// Contrato del repositorio de fichajes.
    /// Definir las operaciones de persistencia sin importar la BD usada.
    /// </summary>
    public interface IFichajeRepositorio
    {
        /// <summary> Registra la hora de entrada de un empleado </summary>
        void RegistrarEntrada(int empleadoId, DateTime horaEntrada);

        /// <summary> Registra la hora de salida del fichaje abierto
        /// Retorna false si no hay fichaje abierto para ese empleado
        /// </summary>
        bool RegistrarSalida(int empleadoId, DateTime horaSalida);

        /// <summary> 
        /// Obtener el fichaje abierto (sin salida) de un empleado
        /// Retorna null si no tiene fichaje activo.
        /// </summary>
        RegistroFichaje? ObtenerFichajeAbierto(int empleadoId);

        /// <summary> Obtine todos los fichaje completados de un mes y anio especificos</summary>
        IEnumerable<RegistroFichaje> ObtenerPorMes(int mes, int anio);

        /// <summary> Obtine los ultimos fichajes de un empleado (Para historial)</summary>
        IEnumerable<RegistroFichaje> ObtenerUltimos(int empleadoId, int cantidad = 10);
    }
}