<div align="center">
  <img src="wwwroot/android-chrome-512x512.png" width="88" alt="Elara logo" />

  # Elara Beauty Studio

  **A full-featured salon management system** — appointments, billing, inventory, staff and reporting, built with ASP.NET Core MVC.

  [![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
  [![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
  [![MySQL](https://img.shields.io/badge/MySQL-4479A1?logo=mysql&logoColor=white)](https://www.mysql.com/)
  [![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?logo=bootstrap&logoColor=white)](https://getbootstrap.com/)
  [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

  **English** · [Español](README.es.md)
</div>

---

## Overview

Elara is a bilingual (UI in Spanish, docs in both languages) web application built for a real beauty salon's day-to-day operations: booking appointments around each specialist's schedule, checking clients out with an itemized invoice, tracking product stock, and giving the owner a clear picture of revenue and commissions — all behind role-based access for **Admin**, **Front Desk**, and **Specialist** staff.

It's a server-rendered ASP.NET Core MVC app on purpose: no SPA framework, no build step for the frontend — Razor views, a small amount of vanilla JavaScript for AJAX interactions, and a custom design system instead of default Bootstrap styling. The goal was a fast, dependency-light app that a small business can actually run on a modest server.

## Screenshots

<table>
  <tr>
    <td width="50%"><img src="docs/screenshots/login.png" alt="Login screen" /><br/><sub align="center">Login</sub></td>
    <td width="50%"><img src="docs/screenshots/dashboard.png" alt="Dashboard with KPIs" /><br/><sub align="center">Dashboard</sub></td>
  </tr>
  <tr>
    <td width="50%" colspan="2" align="center">
      <img src="docs/screenshots/reportes-1.png" alt="Reports page — KPIs and date filter" /><br/>
      <img src="docs/screenshots/reportes-2.png" alt="Reports page — revenue and top-services charts" /><br/>
      <sub align="center">Reports — KPIs, revenue &amp; top services</sub>
    </td>
  </tr>
  <tr>
    <td width="50%"><img src="docs/screenshots/servicios.png" alt="Service catalog" /><br/><sub align="center">Service catalog</sub></td>
    <td width="50%"><img src="docs/screenshots/clientes.png" alt="Client directory" /><br/><sub align="center">Client directory</sub></td>
  </tr>
  <tr>
    <td width="50%"><img src="docs/screenshots/citas-listado.png" alt="Daily appointment list" /><br/><sub align="center">Appointments — day list</sub></td>
    <td width="50%"><img src="docs/screenshots/citas-nueva-cita.png" alt="New appointment form" /><br/><sub align="center">New appointment — live availability</sub></td>
  </tr>
  <tr>
    <td width="50%" colspan="2" align="center"><img src="docs/screenshots/factura-whatsapp.png" alt="Invoice with WhatsApp share" width="60%" /><br/><sub align="center">Invoice with WhatsApp receipt</sub></td>
  </tr>
</table>

## Features

**Appointments**
- Day-by-day appointment list (date navigator, no external calendar dependency) with create, edit, reschedule and status workflow (`Pending → Confirmed → In progress → Completed`, plus `Cancelled` / `No-show`).
- Real-time availability: picking a service, date and specialist queries only the time slots that are actually free, computed from that specialist's weekly working hours and existing bookings.
- Specialists get their own read-only **Agenda** — their day's appointments and one-tap status updates.

**Billing**
- Checkout flow from a completed appointment, or a standalone **Quick Sale** for walk-in clients/products.
- Line-item invoices (services and/or products), discounts with a required justification, cash/card/transfer payment methods with transfer-proof upload.
- Daily cash register report and per-employee commission report, both with date-range filters.
- One-click, branded **WhatsApp receipt** — builds a `wa.me` deep link with a formatted, itemized message (no paid WhatsApp Business API needed).
- Partial/full returns per line item, automatically reflected back into stock.

**Clients, services & inventory**
- Client directory with visit history.
- Service catalog with categories, duration and price (duration drives appointment-slot length).
- Product inventory with stock levels, movement history and low-stock awareness.

**Staff**
- Employee profiles linked 1:1 to a login account, weekly working-hour schedules, specialties, and a commission percentage used to auto-calculate what each sale/service earns them.
- Temporary password on account creation, with a forced password change enforced on first login.

**Reporting**
- Revenue by month, most-requested services, employee ranking, new clients by month — all as interactive Chart.js graphs with a date-range filter.
- Excel export (via ClosedXML) for revenue data.

**Platform**
- ASP.NET Core Identity with three roles (`Administrador`, `Recepcionista`, `Especialista`) — routes, menus and actions all adapt to the signed-in role.
- Light/dark theme, resolved server-side (no flash of the wrong theme on load) and persisted per user.
- Structured logging with Serilog (console + rolling daily file).
- Configurable business profile (name, logo, hours, currency) from the admin panel.

## Tech stack

| Layer | Choice |
|---|---|
| Framework | ASP.NET Core MVC (.NET 8) |
| Database | MySQL / MariaDB via Entity Framework Core + [Pomelo.EntityFrameworkCore.MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql) |
| Auth | ASP.NET Core Identity, role-based authorization |
| Frontend | Razor views, Bootstrap 5, vanilla JS (`fetch`), Chart.js |
| Logging | Serilog (console + file sinks) |
| Reporting export | ClosedXML |
| Architecture | Controllers → Services (business rules) → Repositories (EF Core) → SQL, with ViewModels at the controller/view boundary |

### A couple of engineering decisions worth mentioning

- **Availability engine**: rather than letting double-bookings happen and cleaning them up, `DisponibilidadService` computes candidate time slots directly from each employee's `HorarioTrabajo` (working hours per weekday) and cross-checks them against existing appointments — the UI can only ever offer slots that are genuinely free.
- **No calendar library**: an earlier version used FullCalendar; it was replaced with a plain server-rendered day list plus small AJAX-driven modals. Less flashy, considerably more robust — no client-side calendar state to get out of sync with the server.
- **WhatsApp without an API contract**: client communication uses `wa.me` "click-to-chat" links with a pre-filled, branded message instead of the paid WhatsApp Business Cloud API — a deliberate trade-off for a small business that doesn't need programmatic delivery, doesn't want a monthly bill for it, and still gets a polished result.

## Roles & permissions

| | Admin | Front desk | Specialist |
|---|:---:|:---:|:---:|
| Dashboard & reports | ✅ | – | – |
| Appointments (create/edit/reschedule) | ✅ | ✅ | – |
| My agenda (own appointments, read-only) | – | – | ✅ |
| Clients & services | ✅ | ✅ | – |
| Billing / invoices | ✅ | ✅ | – |
| Employees, inventory, commissions, settings | ✅ | – | – |

## Getting started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- MySQL or MariaDB server running locally (or reachable)

### Setup

```bash
git clone https://github.com/Luzmy555/Elara.git
cd Elara/Elara

# Configure the database connection (user-secrets keeps it out of source control)
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=127.0.0.1;Port=3306;Database=elara_db;User=root;Password=;"

# Apply migrations (creates the database) and seed the initial admin user + roles
dotnet ef database update

# Run
dotnet watch run
```

The app will be available at `http://localhost:5291`. On first run, `SeedData` creates the `Administrador`, `Recepcionista` and `Especialista` roles and a default admin account — check `Data/SeedData.cs` for the seeded credentials.

## Project structure

```
Elara/
├── Controllers/        # One controller per module (Citas, Facturas, Empleados, Reportes, ...)
├── Services/            # Business rules (availability engine, invoicing, commissions, ...)
├── Repositories/        # EF Core data access, one per aggregate
├── Models/              # EF Core entities
├── ViewModels/          # Shapes tailored to each view/form
├── Views/               # Razor views, grouped by controller
├── wwwroot/             # CSS (custom design system), JS, static assets
└── Data/                # DbContext + seed data
```

## License

Released under the [MIT License](LICENSE).
