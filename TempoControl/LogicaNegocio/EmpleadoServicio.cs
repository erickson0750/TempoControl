using TempoControl.Dominio;
using TempoControl.Repositorio.Interfaces;

namespace TempoControl.LogicaNegocio
{
    /// <summary>
    /// Servicio de logica de negocio para la gestion de empleados.
    /// Capa: Logica de negocio - valida regla antes de delegar al repositorio.
    /// No conoce la implementacion concreta(depende solo de la interfaz).
    /// </summary>
    public class EmpleadoServicio
    {
        private readonly IEmpleadoRepositorio _repositorio;

        public EmpleadoServicio(IEmpleadoRepositorio repositorio)
        {
            _repositorio = repositorio
                ?? throw new ArgumentNullException(nameof(repositorio));
        }

        /// <summary>
        /// Crea un nuevo empleado tras validar que los datos sean correctos.
        /// </summary>
        public Empleado CrearEmpleado(string nombre, string departamento, string posicion)
        {
            ValidarTexto(nombre, "Nombre completo");
            ValidarTexto(departamento, "Departamento");
            ValidarTexto(posicion, "Posición");

            var empleado = new Empleado
            {
                NombreCompleto = nombre.Trim(),
                Departamento = departamento.Trim(),
                Posicion = posicion.Trim(),
                Activo = true,
                FechaRegistro = DateTime.Now
            };

            _repositorio.Crear(empleado);
            return empleado;
        }

        /// <summary> Obtiene todos los empleados del sistema.</summary>
        public IEnumerable<Empleado> ObtenerTodos()
            => _repositorio.ObtenerTodos();

        /// <summary> Obtiene solo los empleados activos.</summary>
        public IEnumerable<Empleado> ObtenerActivos()
            => _repositorio.ObtenerActivos();

        /// <summary>
        /// Busca un empleado por ID.
        /// Lanza excepcion si no existe
        /// </summary>
        public Empleado ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID del empleado debe ser mayor a cero.");

            return _repositorio.ObtenerPorId(id)
                ?? throw new KeyNotFoundException(
                    $"No se encontro ningun empleado con ID {id}.");
        }

        /// <summary>
        /// Actualiza los datos de un empleado existente.
        /// </summary>
        public void ActualizarEmpleado(int id, string nombre, string departamento, string posicion)
        {
            ValidarTexto(nombre, "Nombre completo");
            ValidarTexto(departamento, "Departamento");
            ValidarTexto(posicion, "Posición");

            var empleado = ObtenerPorId(id); //Verificar que exista
            empleado.NombreCompleto = nombre.Trim();
            empleado.Departamento = departamento.Trim();
            empleado.Posicion = posicion.Trim();

            if (!_repositorio.Actualizar(empleado))
                throw new InvalidOperationException(
                    $"No se pudo actualizar el empleado con ID {id}.");
        }

        /// <summary>
        /// Desactivar un empleado (baja logica, nunca se elimina).
        /// </summary>
        public void DesactivarEmpleado(int id)
        {
            var empleado = ObtenerPorId(id); //Verificar que exista

            if (!empleado.Activo)
                throw new InvalidOperationException(
                    $"El empleado '{empleado.NombreCompleto}' ya esta inactivo.");

            if (!_repositorio.Desactivar(id))
                throw new InvalidOperationException(
                    $"No se pudo desactivar el empleado con ID {id}.");
        }

        // validaciones privadas
        private static void ValidarTexto(string valor, string campo)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException(
                    $"El campo '{campo}' no puede estar vacio.");

            if (valor.Trim().Length > 100)
                throw new ArgumentException(
                    $"El campo '{campo}' no puede superar los 100 caracteres.");
        }
    }
}