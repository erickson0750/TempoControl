using Microsoft.Data.Sqlite;
using TempoControl.BaseDatos;
using TempoControl.Dominio;
using TempoControl.Repositorio.Interfaces;

namespace TempoControl.Repositorio.Implementaciones
{
    /// <summary>
    /// Implementacion concreta del repositorio de Empleados usando SQLite.
    /// Capa: Acceso a Datos - todo el SQL esta aqui, sin logica de negocio.
    /// </summary>
    public class EmpleadoRepositorio
    {
        private readonly InicializadorBaseDatos _db;

        public EmpleadoRepositorio(InicializadorBaseDatos db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public void Crear(Empleado empleado)
        {
            if (empleado == null) throw new ArgumentNullException(nameof(empleado));

            try
            {
                using var conexion = _db.CrearConexion();
                using var cmd = conexion.CreateCommand();

                cmd.CommandText = @"
                    INSERT INTO Empleados (NombreCompleto, Departamento, Posicion, Activo, FechaRegistro)
                    VALUES ($nombre, $depto, $posicion, $activo, $fecha);
                    SELECT last_insert_rowid();";

                cmd.Parameters.AddWithValue("$nombre", empleado.NombreCompleto);
                cmd.Parameters.AddWithValue("$depto", empleado.Departamento);
                cmd.Parameters.AddWithValue("$posicion", empleado.Posicion);
                cmd.Parameters.AddWithValue("$activo", empleado.Activo ? 1 : 0);
                cmd.Parameters.AddWithValue("$fecha", empleado.FechaRegistro.ToString("o"));

                var resultado = cmd.ExecuteScalar();
                empleado.Id = Convert.ToInt32(resultado);
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException(
                    $"Error al crear el empleado '{empleado.NombreCompleto}': {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<Empleado> ObtenerTodos()
        {
            var lista = new List<Empleado>();
            try
            {
                using var conexion = _db.CrearConexion();
                using var cmd = conexion.CreateCommand();
                cmd.CommandText = @"
                    SELECT Id, NombreCompleto, Departamento, Posicion, Activo, FechaRegistro
                    FROM Empleados
                    ORDER BY NombreCompleto;";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(MapearEmpleado(reader));
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException(
                    $"Error al obtener empleados: {ex.Message}", ex);
            }
            return lista;
        }

        ///<inheritdoc/>
        public IEnumerable<Empleado> ObtenerActivos()
        {
            var lista = new List<Empleado>();
            try
            {
                using var conexion = _db.CrearConexion();
                using var cmd = conexion.CreateCommand();
                cmd.CommandText = @"
                    SELECT Id, NombreCompleto, Departamento, Posicion, Activo, FechaRegistro
                    FROM Empleados
                    WHERE Activo = 1
                    ORDER BY NombreCompleto;";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(MapearEmpleado(reader));
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException(
                    $"Error al obtener empleados activos: {ex.Message}", ex);
            }
            return lista;
        }

        /// <inheritdoc/>
        public Empleado? ObtenerPorId(int id)
        {
            try
            {
                using var conexion = _db.CrearConexion();
                using var cmd = conexion.CreateCommand();
                cmd.CommandText = @"
                    SELECT Id, NombreCompleto, Departamento, Posicion, Activo, FechaRegistro
                    FROM Empleados
                    WHERE Id = $id;";
                cmd.Parameters.AddWithValue("$id", id);

                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapearEmpleado(reader) : null;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException(
                    $"Error al buscar empleado con ID {id}: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public bool Actualizar(Empleado empleado)
        {
            if (empleado == null) throw new ArgumentNullException(nameof(empleado));
            try
            {
                using var conexion = _db.CrearConexion();
                using var cmd = conexion.CreateCommand();
                cmd.CommandText = @"
                    UPDATE Empleados
                    SET NombreCompleto = $nombre,
                        Departamento   = $depto,
                        Posicion       = $posicion
                    WHERE Id = $id;";

                cmd.Parameters.AddWithValue("$nombre", empleado.NombreCompleto);
                cmd.Parameters.AddWithValue("$depto", empleado.Departamento);
                cmd.Parameters.AddWithValue("$posicion", empleado.Posicion);
                cmd.Parameters.AddWithValue("$id", empleado.Id);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException(
                    $"Error al actualizar empleado ID {empleado.Id}: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public bool Desactivar(int id)
        {
            try
            {
                using var conexion = _db.CrearConexion();
                using var cmd = conexion.CreateCommand();
                cmd.CommandText = @"
                    UPDATE Empleados
                    SET Activo = 0
                    WHERE Id = $id;";
                cmd.Parameters.AddWithValue("$id", id);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException(
                    $"Error al desactivar empleado ID {id}: {ex.Message}", ex);
            }
        }

        // Mapeo privado 
        private static Empleado MapearEmpleado(SqliteDataReader reader) => new()
        {
            Id = reader.GetInt32(0),
            NombreCompleto = reader.GetString(1),
            Departamento = reader.GetString(2),
            Posicion = reader.GetString(3),
            Activo = reader.GetInt32(4) == 1,
            FechaRegistro = DateTime.Parse(reader.GetString(5))
        };
    }
}