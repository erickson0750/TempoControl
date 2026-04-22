# TempoControl

### Sistema de Gestión de Fichaje de Empleados

**Cliente:** Innovatech Solutions, S.R.L.
**Asignatura:** Teoría de Programación 1 — Unidad 6 Manejo de excepciones y persistencia de datos
**Tecnología:** C# · .NET 10 · SQLite · Patrón Repositorio

---

## Descripción

TempoControl es un sistema de consola desarrollado en C# que permite
gestionar el fichaje (ponchado) de empleados. Reemplaza el proceso
manual de hojas de Excel utilizado por Recursos Humanos, permitiendo
registrar entradas y salidas, y generar reportes mensuales de horas
trabajadas.

---

## Arquitectura — 4 Capas + Patrón Repositorio

```
┌─────────────────────────────────────────────────────┐
│              CAPA DE PRESENTACIÓN                    │
│   ConsolaHelper · EmpleadoUI · FichajeUI · ReporteUI│
└─────────────────────┬───────────────────────────────┘
│ usa
┌─────────────────────▼───────────────────────────────┐
│           CAPA DE LÓGICA DE NEGOCIO                  │
│         EmpleadoServicio · FichajeServicio           │
└──────────┬──────────────────────┬───────────────────┘
│ depende de           │ depende de
┌──────▼──────┐        ┌──────▼──────┐
│IEmpleado    │        │IFichaje     │
│Repositorio  │        │Repositorio  │
│(Interfaz)   │        │(Interfaz)   │
└──────┬──────┘        └──────┬──────┘
│ implementa           │ implementa
┌──────────▼──────────────────────▼───────────────────┐
│              CAPA DE ACCESO A DATOS                  │
│    EmpleadoRepositorio · FichajeRepositorio (SQLite) │
└─────────────────────┬───────────────────────────────┘
│
┌────────▼────────┐
│InicializadorBD  │
│ tempocontrol.db │
└─────────────────┘
    CAPA DE DOMINIO (transversal a todas)
  Empleado · RegistroFichaje · ReporteEmpleado
```

---

## Estructura del Proyecto

```
TempoControl/
│
├── TempoControl.sln
├── TempoControl.csproj
├── Program.cs                        ← Punto de entrada y composición de dependencias
├── README.md
│
├── Dominio/                          ← Entidades puras sin dependencias
│   ├── Empleado.cs
│   ├── RegistroFichaje.cs
│   └── ReporteEmpleado.cs
│
├── Repositorio/
│   ├── Interfaces/                   ← Contratos del Patrón Repositorio
│   │   ├── IEmpleadoRepositorio.cs
│   │   └── IFichajeRepositorio.cs
│   │
│   └── Implementaciones/             ← Acceso a datos con SQLite
│       ├── EmpleadoRepositorio.cs
│       └── FichajeRepositorio.cs
│
├── LogicaNegocio/                    ← Reglas y validaciones del sistema
│   ├── EmpleadoServicio.cs
│   └── FichajeServicio.cs
│
├── Presentacion/                     ← Menús e interacción con el usuario
│   ├── ConsolaHelper.cs
│   ├── EmpleadoUI.cs
│   ├── FichajeUI.cs
│   └── ReporteUI.cs
│
└── BaseDatos/                        ← Inicialización del esquema SQLite
└── InicializadorBaseDatos.cs
```

---

## Modelo de Base de Datos

### Tabla: `Empleados`

| Columna        | Tipo    | Descripción                            |
| -------------- | ------- | -------------------------------------- |
| Id             | INTEGER | Clave primaria autoincremental         |
| NombreCompleto | TEXT    | Nombre completo del empleado           |
| Departamento   | TEXT    | Departamento al que pertenece          |
| Posicion       | TEXT    | Cargo o posición en la empresa         |
| Activo         | INTEGER | 1 = Activo, 0 = Inactivo (baja lógica) |
| FechaRegistro  | TEXT    | Fecha de registro en formato ISO 8601  |

### Tabla: `RegistrosFichaje`

| Columna     | Tipo    | Descripción                              |
| ----------- | ------- | ---------------------------------------- |
| Id          | INTEGER | Clave primaria autoincremental           |
| EmpleadoId  | INTEGER | Clave foránea → Empleados(Id)            |
| HoraEntrada | TEXT    | Fecha y hora de entrada en ISO 8601      |
| HoraSalida  | TEXT    | Fecha y hora de salida (NULL si abierto) |

---

## Diagrama UML de Clases

```
┌─────────────────────────┐ ┌──────────────────────────────┐
│ <<Entidad>> │ │ <<Entidad>> │
│ Empleado │ │ RegistroFichaje │
├─────────────────────────┤ ├──────────────────────────────┤
│ + Id: int │ │ + Id: int │
│ + NombreCompleto: string│ │ + EmpleadoId: int │
│ + Departamento: string │ │ + HoraEntrada: DateTime │
│ + Posicion: string │ 1──\* │ + HoraSalida: DateTime? │
│ + Activo: bool │ │ + NombreEmpleado: string │
│ + FechaRegistro:DateTime│ │ + EstaCompleto: bool │
└─────────────────────────┘ │ + HorasTrabajadas: double? │
└──────────────────────────────┘
┌──────────────────────────────┐
│ <<Interfaz>> │
│ IEmpleadoRepositorio │
├──────────────────────────────┤
│ + Crear(Empleado) │
│ + ObtenerTodos() │
│ + ObtenerActivos() │
│ + ObtenerPorId(int) │
│ + Actualizar(Empleado): bool │
│ + Desactivar(int): bool │
└──────────────┬───────────────┘
△ implementa
┌──────────────┴───────────────┐
│ EmpleadoRepositorio │
│ (SQLite) │
└──────────────────────────────┘
┌──────────────────────────────┐
│ <<Interfaz>> │
│ IFichajeRepositorio │
├──────────────────────────────┤
│ + RegistrarEntrada(...) │
│ + RegistrarSalida(...) │
│ + ObtenerFichajeAbierto(...) │
│ + ObtenerPorMes(...) │
│ + ObtenerUltimos(...) │
└──────────────┬───────────────┘
△ implementa
┌──────────────┴───────────────┐
│ FichajeRepositorio │
│ (SQLite + Transacciones) │
└──────────────────────────────┘
```

---

## Requerimientos Implementados

| Código | Descripción                                             | Estado |
| ------ | ------------------------------------------------------- | ------ |
| RF-01  | CRUD de Empleados (Crear, Leer, Actualizar, Desactivar) | ✅     |
| RF-02  | Registro de entrada y salida con timestamp exacto       | ✅     |
| RF-03  | Reporte mensual con días y horas por empleado           | ✅     |
| RNF-01 | C# · .NET 8 · Aplicación de Consola                     | ✅     |
| RNF-02 | SQLite sin servidor, portable                           | ✅     |
| RNF-03 | Patrón Repositorio con 4 capas separadas                | ✅     |

---

## Instalación y Ejecución

### Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- No requiere servidor de base de datos (SQLite está embebido)

### Pasos

```bash
# 1. Clonar el repositorio
git clone https://github.com/erickson0750/TempoControl.git

# 2. Entrar a la carpeta
cd TempoControl

# 3. Restaurar paquetes
dotnet restore

# 4. Ejecutar
dotnet run
```

La base de datos `tempocontrol.db` se crea automáticamente al primer inicio.

---

## Decisiones de Diseño

- **Baja lógica de empleados:** Los empleados nunca se eliminan con DELETE,
  solo se marcan como Activo = 0, preservando la integridad histórica.
- **Transacciones explícitas:** RegistrarEntrada y RegistrarSalida usan
  BeginTransaction() para garantizar atomicidad.
- **Sin SQL en la capa de negocio:** Todo el SQL está exclusivamente en
  los repositorios. Los servicios solo conocen interfaces.
- **Parámetros SQLite ($param):** Previene inyección SQL. Nunca se
  concatenan valores en los comandos SQL.
- **Índices en BD:** Se crean índices sobre EmpleadoId y HoraEntrada
  para acelerar las consultas de reportes mensuales.

---

## Autor

**Nombre:** Erickson Cayetano Paredes
**Matrícula:** 100690895
**Fecha:** 21 Abril 2026
