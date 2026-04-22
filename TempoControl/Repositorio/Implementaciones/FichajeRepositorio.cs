using Microsoft.Data.Sqlite;
using TempoControl.BaseDatos;
using TempoControl.Dominio;
using TempoControl.Repositorio.Interfaces;

namespace TempoControl.Repositorio.Implementaciones
{
    /// <summary>
    /// Implementacion concreta del repositorio de Fichaje usando SQLite
    /// capa: Acceso a datos - usa transacciones explicitas para garantizar la integridad del ciclo entrada/salida.
    /// </summary>
    public class FichajeRepositorio : IFichajeRepositorio
    {
        private readonly InicializadorBaseDatos _db;

        public FichajeRepositorio(InicializadorBaseDatos db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        ///<inheritdoc/>
        public void RegistrarEntrada(int empleadoId, DateTime horaEntrada)
        {
            try
            {
                using var conexion = _db.CrearConexion();
                using var transaccion = conexion.BeginTransaction();
                try
                {
                    //Verificar que no haya ya un fichaje abierto
                    using var checkCmd = conexion.CreateCommand();
                    checkCmd.Transaction = transaccion;
                    checkCmd.CommandText = @"
                        SELECT COUNT(*) FROM RegistrosFichaje
                        WHERE EmpleadoId = $empId
                          AND HoraSalida IS NULL;";
                    checkCmd.Parameters.AddWithValue("$empId", empleadoId);

                    var cantidad = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (cantidad > 0)
                        throw new InvalidOperationException(
                            "El empleado ya tiene un fichaje de entrada abierto.");

                    //Insertar el nuevo registro de entrada
                    using var insertCmd = conexion.CreateCommand();
                    insertCmd.Transaction = transaccion;
                    insertCmd.CommandText = @"
                        INSERT INTO RegistrosFichaje (EmpleadoId, HoraEntrada)
                        VALUES ($empId, $entrada);";
                    insertCmd.Parameters.AddWithValue("$empId", empleadoId);
                    insertCmd.Parameters.AddWithValue("$entrada", horaEntrada.ToString("o"));
                    insertCmd.ExecuteNonQuery();

                    transaccion.Commit();
                }
                catch
                {
                    transaccion.Rollback();
                    throw;
                }
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException(
                    $"Error de base de datos al registrar entrada: {ex.Message}", ex);
            }
        }

        ///<inheritdoc/>
        public bool RegistrarSalida(int empleadoId, DateTime horaSalida)
        {
            try
            {
                using var conexion = _db.CrearConexion();
                using var transaccion = conexion.BeginTransaction();
                try
                {
                    //Buscar el fichaje abierto del empleado
                    using var selectCmd = conexion.CreateCommand();
                    selectCmd.Transaction = transaccion;
                    selectCmd.CommandText = @"
                        SELECT Id, HoraEntrada
                        FROM RegistrosFichaje
                        WHERE EmpleadoId = $empId
                          AND HoraSalida IS NULL
                        ORDER BY HoraEntrada DESC
                        LIMIT 1;";
                    selectCmd.Parameters.AddWithValue("$empId", empleadoId);

                    int fichajeId;
                    DateTime horaEntrada;

                    using (var reader = selectCmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            transaccion.Rollback();
                            return false;
                        }
                        fichajeId = reader.GetInt32(0);
                        horaEntrada = DateTime.Parse(reader.GetString(1));
                    }

                    //Validar que la salida sea posterior a la entrada
                    if (horaSalida <= horaEntrada)
                        throw new InvalidOperationException(
                            "La hora de salida no puede ser anterior o igual a la hora de entrada.");

                    // Registrar la salida
                    using var updateCmd = conexion.CreateCommand();
                    updateCmd.Transaction = transaccion;
                    updateCmd.CommandText = @"
                        UPDATE RegistrosFichaje
                        SET HoraSalida = $salida
                        WHERE Id = $id;";
                    updateCmd.Parameters.AddWithValue("$salida", horaSalida.ToString("o"));
                    updateCmd.Parameters.AddWithValue("$id", fichajeId);
                    updateCmd.ExecuteNonQuery();

                    transaccion.Commit();
                    return true;
                }
                catch
                {
                    transaccion.Rollback();
                    throw;
                }
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException(
                    $"Error de base de datos al registrar salida: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public RegistroFichaje? ObtenerFichajeAbierto(int empleadoId)
        {
            try
            {
                using var conexion = _db.CrearConexion();
                using var cmd = conexion.CreateCommand();
                cmd.CommandText = @"
                    SELECT rf.Id, rf.EmpleadoId, rf.HoraEntrada, rf.HoraSalida,
                           e.NombreCompleto
                    FROM RegistrosFichaje rf
                    INNER JOIN Empleados e ON e.Id = rf.EmpleadoId
                    WHERE rf.EmpleadoId = $empId
                      AND rf.HoraSalida IS NULL
                    ORDER BY rf.HoraEntrada DESC
                    LIMIT 1;";
                cmd.Parameters.AddWithValue("$empId", empleadoId);

                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapearFichaje(reader) : null;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException(
                    $"Error al verificar fichaje abierto: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<RegistroFichaje> ObtenerPorMes(int mes, int anio)
        {
            var lista = new List<RegistroFichaje>();
            try
            {
                using var conexion = _db.CrearConexion();
                using var cmd = conexion.CreateCommand();

                // Filtrar por rango exacto de fechas del mes
                var inicio = new DateTime(anio, mes, 1).ToString("o");
                var fin = new DateTime(anio, mes, 1).AddMonths(1).ToString("o");

                cmd.CommandText = @"
                    SELECT rf.Id, rf.EmpleadoId, rf.HoraEntrada, rf.HoraSalida,
                           e.NombreCompleto
                    FROM RegistrosFichaje rf
                    INNER JOIN Empleados e ON e.Id = rf.EmpleadoId
                    WHERE rf.HoraEntrada >= $inicio
                      AND rf.HoraEntrada <  $fin
                      AND rf.HoraSalida IS NOT NULL
                    ORDER BY e.NombreCompleto, rf.HoraEntrada;";

                cmd.Parameters.AddWithValue("$inicio", inicio);
                cmd.Parameters.AddWithValue("$fin", fin);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(MapearFichaje(reader));
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException(
                    $"Error al obtener fichajes del mes: {ex.Message}", ex);
            }
            return lista;
        }

        /// <inheritdoc/>
        public IEnumerable<RegistroFichaje> ObtenerUltimos(int empleadoId, int cantidad = 10)
        {
            var lista = new List<RegistroFichaje>();
            try
            {
                using var conexion = _db.CrearConexion();
                using var cmd = conexion.CreateCommand();
                cmd.CommandText = @"
                    SELECT rf.Id, rf.EmpleadoId, rf.HoraEntrada, rf.HoraSalida,
                           e.NombreCompleto
                    FROM RegistrosFichaje rf
                    INNER JOIN Empleados e ON e.Id = rf.EmpleadoId
                    WHERE rf.EmpleadoId = $empId
                    ORDER BY rf.HoraEntrada DESC
                    LIMIT $cantidad;";
                cmd.Parameters.AddWithValue("$empId", empleadoId);
                cmd.Parameters.AddWithValue("$cantidad", cantidad);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(MapearFichaje(reader));
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException(
                    $"Error al obtener ultimos fichajes: {ex.Message}", ex);
            }
            return lista;
        }

        // Mapeo privado
        private static RegistroFichaje MapearFichaje(SqliteDataReader reader) => new()
        {
            Id = reader.GetInt32(0),
            EmpleadoId = reader.GetInt32(1),
            HoraEntrada = DateTime.Parse(reader.GetString(2)),
            HoraSalida = reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3)),
            NombreEmpleado = reader.GetString(4)
        };
    }
}