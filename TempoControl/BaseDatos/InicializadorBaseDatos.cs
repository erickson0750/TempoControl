using Microsoft.Data.Sqlite;

namespace TempoControl.BaseDatos
{
    /// <summary>
    /// Responsable de inicializar la base de datos SQLite y crear el esquema.
    /// capa: Acceso a Datos - gestiona conexion y la creacion de tablas.
    /// </summary>
    public class InicializadorBaseDatos
    {
        private readonly string _cadenaConexion;

        public InicializadorBaseDatos(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion
                ?? throw new ArgumentException(nameof(cadenaConexion));
        }

        /// <summary>
        /// Crea las tablas si no existen.
        /// Se llama una sola vez al iniciar la apliacion.
        /// </summary>
        public void inicializar()
        {
            try
            {
                using var conexion = new SqliteConnection(_cadenaConexion);
                conexion.Open();

                // Activa integridad referencial en SQLite
                using (var pragma = conexion.CreateCommand())
                {
                    pragma.CommandText = "PRAGMA foreign_keys = ON;";
                    pragma.ExecuteNonQuery();
                }

                // Crear tabla Empleados
                using (var cmd = conexion.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Empleados (
                        Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                        NombreCompleto      TEXT NOT NULL,
                        Departamento        TEXT NOT NULL,
                        Posicion            TEXT NOT NULL,
                        Activo              INTEGER NOT NULL DEFAULT 1,
                        FechaRegistro       TEXT NOT NULL
                        );";
                    cmd.ExecuteNonQuery();
                }

                // Crear tabla registrosFichaje
                using (var cmd = conexion.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS RegistrosFichaje (
                            Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                            EmpleadoId  INTEGER NOT NULL,
                            HoraEntrada TEXT    NOT NULL,
                            HoraSalida  TEXT    NULL,
                            FOREIGN KEY (EmpleadoId) REFERENCES Empleados(Id)
                        );";
                    cmd.ExecuteNonQuery();
                }

                //Crear indices para mejorar redimiento en consultas.
                using (var cmd = conexion.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE INDEX IF NOT EXISTS IX_Fichaje_EmpleadoId
                            ON RegistrosFichaje(Empleado);
                        CREATE INDEX IF NOT EXISTS IX_Fichaje_HoraEntrada
                            ON RegistrosFichaje(HoraEntrada);";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException(
                    $"Error al inicializar la base de datos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Crea y retorna una nueva conexion abierta a SQLite.
        /// Cada repositorio la llama cuando necesita ejecutar una consulta.
        /// </summary>
        public SqliteConnection CrearConexion()
        {
            var conexion = new SqliteConnection(_cadenaConexion);
            conexion.Open();

            // Activar claves foraneas en cada nueva conexion. 
            using var pragma = conexion.CreateCommand();
            pragma.CommandText = "PRAGMA foreing_key = ON;";
            pragma.ExecuteNonQuery();

            return conexion;
        }
    }
}