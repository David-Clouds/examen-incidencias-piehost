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

main (commit inicial)
├── feature/busqueda-algolia (PR #1, sin conflicto — primera en fusionar)
├── feature/cache-redis (PR #2 — conflicto 1 al traer main)
└── feature/websocket-piehost (PR #3 — conflicto 2 al traer main)





Orden de fusión: **A → B → C**, incorporando `main` a cada rama mediante `merge` (no rebase, no squash) antes de fusionar su PR.

### Resolución del conflicto 1 (rama B, PR #2)
Ambas ramas modificaban la misma línea de título y el listado de `Incidencias`. Se conservaron **ambas funciones**: la búsqueda con Algolia (consulta directa) y el caché con Redis (60s) para el listado general sin término de búsqueda. Título final: "Incidencias abiertas con búsqueda y consulta rápida".

### Resolución del conflicto 2 (rama C, PR #3)
Se incorporó `main` (ya con Algolia + Redis) a la rama de WebSocket. Se conservaron **las tres funciones**: búsqueda, caché y la conexión en tiempo real vía PieSocket, además del endpoint `/Estado/{id}` para reconciliar el estado al reconectar. Título final: "Incidencias abiertas en tiempo real, con búsqueda y consulta rápida".

Historial completo: `git log --graph --oneline --all`.

## Pruebas realizadas

- ✅ Búsqueda por estación/descripción vía Algolia; una incidencia cerrada no aparece aunque esté indexada
- ✅ Caché Redis: `CACHE MISS` en la primera consulta, `CACHE HIT` en las siguientes, invalidación (`CACHE INVALIDADA`) al cerrar una incidencia
- ✅ Al cerrar una incidencia: se persiste en base → se invalida la caché → se publica el evento en PieSocket, en ese orden
- ✅ Dos sesiones abiertas simultáneamente: al cerrar una incidencia en una, desaparece en la otra sin recargar
- ✅ Reconexión del WebSocket: al recuperar la conexión, se consulta el estado vigente de cada fila visible