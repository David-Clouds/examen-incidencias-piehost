# 🚲 Plataforma de Incidencias — Bicicletas Compartidas

Aplicación MVC en .NET 10 para que Operaciones registre, busque y consulte averías en las estaciones de bicicletas compartidas, con actualizaciones en tiempo real.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![Algolia](https://img.shields.io/badge/Búsqueda-Algolia-5468FF?logo=algolia&logoColor=white)
![Redis](https://img.shields.io/badge/Caché-Redis-DC382D?logo=redis&logoColor=white)
![PieSocket](https://img.shields.io/badge/WebSocket-PieSocket-orange)
![Render](https://img.shields.io/badge/Deploy-Render-46E3B7?logo=render&logoColor=white)

## 🔗 Enlaces

| Recurso | URL |
|---|---|
| 📦 Repositorio GitHub | https://github.com/David-Clouds/examen-incidencias-piehost |
| 🌐 Aplicación en Render | https://examen-incidencias-piehost.onrender.com |
| 💻 Local | http://localhost:5064 |

## Commit final desplegado

`324b061` — Merge pull request #3 from feature/websocket-piehost (más el commit de configuración de Render que sigue a este)

## Tecnologías

- **.NET 10** (ASP.NET Core MVC + Identity)
- **EF Core + SQLite**
- **Algolia** — búsqueda de incidencias por estación/descripción
- **Redis Cloud** — caché de 60s del listado general
- **PieSocket** (WebSocket) — notificación en tiempo real al cerrar una incidencia
- **Docker** + **Render** — despliegue

## Funcionalidades

- Login con roles (Supervisor / usuario normal)
- Listado de incidencias abiertas (`/Operaciones/Incidencias`), con caché Redis de 60s
- Búsqueda por estación o descripción vía Algolia (consulta directa, sin caché), filtrando solo abiertas existentes en la base
- Cierre de incidencias (solo Supervisor): persiste el estado, invalida la caché y publica el evento `IncidenciaActualizada` por WebSocket
- Actualización en tiempo real entre sesiones sin recargar la página, con reconexión automática

## Cómo correr localmente

```bash
dotnet restore
dotnet ef database update
dotnet run
```

Abre `http://localhost:5064/Operaciones/Incidencias`. Usuario de prueba: `supervisor@incidencias.com` / `Supervisor123!`.

## Configuración por variables de entorno (Render)

Ninguna clave está en el repositorio. En Render se configuran como variables de entorno (con `__` en vez de `:`):

| Variable | Uso |
|---|---|
| `Algolia__AppId`, `Algolia__SearchApiKey`, `Algolia__WriteApiKey` | Búsqueda de incidencias |
| `Redis__ConnectionString` | Caché del listado general |
| `PieSocket__ClusterId`, `PieSocket__ApiKey`, `PieSocket__Canal` | Canal WebSocket de notificaciones |
| `PORT` | Provista automáticamente por Render |

La clave de administración de Algolia (`WriteApiKey`) nunca se expone en el navegador — todas las consultas a Algolia se hacen desde el servidor.

## Estrategia de ramas y fusiones

Las tres ramas nacieron del mismo commit inicial (proyecto base con login, datos de prueba y `/Operaciones/Incidencias`):