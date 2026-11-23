# Agenda de citas — clínica de depilación láser

Módulo mínimo con SQL Server, ASP.NET Core .NET 8 y Angular 17. Cada cita dura 30 minutos y el procedimiento almacenado evita cruces por cubículo, incluso ante solicitudes concurrentes.

## Requisitos

- SQL Server 2019 o superior y SQL Server Management Studio o `sqlcmd`.
- .NET SDK 8.
- Node.js 20 LTS y npm.

## Base de datos

Ejecute en orden los archivos de `/database` con una cuenta que pueda crear la base `ClinicaLaser`:

```powershell
sqlcmd -S localhost -E -i database/001_schema.sql
sqlcmd -S localhost -E -i database/002_citas.sql
```

El modelo se crea solo si no existe. Para probar la aplicación, inserte al menos un paciente y un cubículo:

```sql
USE ClinicaLaser;
INSERT Paciente (Nombres, Dni, Telefono) VALUES (N'Ana Pérez', '12345678', '999111222');
INSERT Cubiculo (Nombre, Sede) VALUES (N'Cubículo 1', N'Miraflores');
```

La consulta solicitada de atendidas y canceladas del mes actual está en `database/003_reporte_mensual.sql`.

## API

Revise `api/Citas.Api/appsettings.json` y ajuste `ConnectionStrings:Clinica` si SQL Server no usa autenticación integrada o no está en `localhost`.

```powershell
cd api
dotnet restore Citas.sln
dotnet run --project Citas.Api
```

Swagger estará disponible en `https://localhost:7047/swagger`. Durante desarrollo, Angular redirige `/api` a esa dirección mediante `web/proxy.conf.json` con validación del certificado local desactivada para el proxy.

Endpoints:

- `GET /api/citas?cubiculoId=1&fecha=2026-11-24`
- `POST /api/citas`
- `PATCH /api/citas/{id}/cancelar`
- `GET /api/pacientes`
- `GET /api/cubiculos`
- `POST /api/pacientes`
- `POST /api/cubiculos`

Ejemplo de alta:

```json
{
  "pacienteId": 1,
  "cubiculoId": 1,
  "fechaHoraInicio": "2026-11-24T09:00:00"
}
```

Las fechas se interpretan como hora local de Lima y se envían sin sufijo UTC.

## Web

Con la API ejecutándose:

```powershell
cd web
npm install
npm start
```

Abra `http://localhost:4200`. La agenda muestra bloques de 08:00 a 18:00 y comunica claramente los conflictos HTTP 409.

## Pruebas

```powershell
dotnet test api/Citas.sln
npm --prefix web run build
```

