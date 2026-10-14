# CodeLab

> **Platform**: ASP.NET Web Forms Web Application (.NET Framework 4.7.2)  
> **Development Database**: LocalDB (`(LocalDB)\MSSQLLocalDB`)

CodeLab is an interactive learning and quiz platform built with ASP.NET Web Forms, structured with a 3-tier architecture (Presentation, Business Logic, and Data Access Layers).

---

## Getting Started

### 1. Prerequisites
* **Visual Studio 2022** (with *.NET desktop development* and *ASP.NET and web development* workloads).
* **SQL Server Express LocalDB** (included with Visual Studio).
* `sqlcmd` available on your `PATH`.


---

## Authentication & Cookie Architecture

Security is enforced at two distinct boundaries:
1. **URL & Folder Authorization**: Scoped `Web.config` files in `Modules/` folders inspect `Context.User`.
2. **Business Logic Layer**: `SessionHelper.IsUserInRole(...)` checks before performing database actions.

### How Cookies & Sessions Work Together

* **Forms Authentication Cookie**: The security authority. Holds the encrypted ticket with `UserID` and `Role`. Checked by `Web.config` on every request.
* **Session State**: Fast server-memory cache for user display info (`FullName`, `Username`). If the session expires, it is automatically restored from the cookie ticket.

```text
┌────────────────────────────────────────────────────────┐
│  FormsAuthenticationTicket                             │
├─────────────────┬──────────────────────────────────────┤
│ Name            │ "admin_user"                         │
│ Issue Date      │ 2026-10-14 17:45:00                  │
│ Expiration Date │ 2026-10-14 18:45:00 (60 min timeout) │
│ UserData        │ "1|Admin"  <-- (UserID | Role)       │
│ CookiePath      │ "/"                                  │
└─────────────────┴──────────────────────────────────────┘
                           │
                 [ Encrypted by Server ]
                           ▼
          .ASPXAUTH = 8F3D9A1B4C7E... (HttpOnly, Secure)
```

### 1. Cookie Generation at Login

```mermaid
sequenceDiagram
    autonumber
    actor User as Browser (Client)
    participant Login as Login.aspx.cs
    participant Helper as SessionHelper.cs
    participant DB as SQL Server (Database.mdf)

    User->>Login: Submit Username & Password
    Login->>DB: Verify credentials in dbo.Users
    DB-->>Login: Match found: UserID=1, Role="Admin"

    Login->>Helper: SessionHelper.SignIn(user)
    Note over Helper: 1. Pack ticket data: "1|Admin"<br/>2. Encrypt ticket with server key
    Helper->>Helper: Create HttpCookie (.ASPXAUTH)
    Helper->>User: HTTP 302 Redirect + Set-Cookie: .ASPXAUTH=EncryptedBlob...

    Note over User: Browser securely stores the cookie<br/>(HttpOnly = true)
```

### 2. Request Authorization Flow

```mermaid
sequenceDiagram
    autonumber
    actor User as Browser (Client)
    participant Global as Global.asax.cs
    participant Guard as Modules/Admin/Web.config
    participant Master as Site.Master.cs / SessionHelper
    participant Page as AuditLogs.aspx

    User->>Global: GET /Modules/Admin/AuditLogs<br/>(Sends Cookie: .ASPXAUTH=EncryptedBlob...)
    
    Note over Global: Application_AuthenticateRequest()<br/>1. Decrypts cookie into ticket<br/>2. Extracts "Admin" role<br/>3. Sets Context.User = GenericPrincipal("Admin")
    
    Global->>Guard: Passes request to Web.config guard
    Note over Guard: Checks: Context.User.IsInRole("Admin")?<br/>Matches: <allow roles="Admin" />
    
    Guard->>Master: Permission granted! Start rendering page
    Note over Master: Checks Session<br/>(If session expired, auto-restores it from the cookie ticket)
    
    Master->>Page: Render HTML & Audit Grid
    Page-->>User: HTTP 200 OK (Protected Page Displays)
```

---

## Directory Structure

```text
CodeLab/                          # Repository root
├── README.md                     # Project overview and quickstart
├── CONTRIBUTING.md               # Detailed architectural & coding rules
└── CodeLab/                      # Visual Studio Web Application Project
    ├── App_Data/                 # Database scripts & SetupDatabase.cmd
    ├── App_Start/                # RouteConfig & BundleConfig
    ├── Models/                   # Plain Old CLR Objects (POCOs)
    ├── DAL/                      # Data Access Layer (ADO.NET SQL queries)
    ├── BLL/                      # Business Logic Layer
    ├── Helpers/                  # Cross-cutting static utilities (SessionHelper, etc.)
    ├── Content/                  # CSS design tokens & Bootstrap
    ├── Scripts/                  # Client-side JavaScript
    ├── Modules/                  # Role-based feature pages
    │   ├── Admin/                # Admin only (AuditLogs, Dashboard)
    │   ├── Tutor/                # Tutor & Admin (ConfigureQuiz)
    │   ├── Learner/              # Learner, Tutor, Admin (CodeLab playground)
    │   └── Public/               # Anonymous (Login, Register)
    ├── UserControls/             # Reusable ASCX components
    ├── Site.Master               # Shared application layout
    └── Web.config                # Root IIS & server configuration
```

For full coding guidelines, database rules, and naming conventions, see [CONTRIBUTING.md](file:///d:/ASP%20Workspace/CodeLab/CONTRIBUTING.md).
