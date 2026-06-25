# Project Overview

<cite>
**Referenced Files in This Document**
- [Program.cs](file://Program.cs)
- [split_bill.csproj](file://split_bill.csproj)
- [appsettings.json](file://appsettings.json)
- [plan.md](file://plan.md)
- [package.json](file://package.json)
- [tailwind.config.js](file://tailwind.config.js)
- [Styles/app.css](file://Styles/app.css)
- [wwwroot/app.css](file://wwwroot/app.css)
- [Components/Layout/MainLayout.razor](file://Components/Layout/MainLayout.razor)
- [Components/Routes.razor](file://Components/Routes.razor)
- [Components/Pages/Home.razor](file://Components/Pages/Home.razor)
- [Data/AppDbContext.cs](file://Data/AppDbContext.cs)
- [Data/Entities/Bill.cs](file://Data/Entities/Bill.cs)
- [Data/Entities/Item.cs](file://Data/Entities/Item.cs)
- [Data/Entities/Participant.cs](file://Data/Entities/Participant.cs)
- [Data/Entities/ItemAssignee.cs](file://Data/Entities/ItemAssignee.cs)
- [Services/SettlementService.cs](file://Services/SettlementService.cs)
</cite>

## Table of Contents
1. [Introduction](#introduction)
2. [Project Structure](#project-structure)
3. [Core Components](#core-components)
4. [Architecture Overview](#architecture-overview)
5. [Detailed Component Analysis](#detailed-component-analysis)
6. [Dependency Analysis](#dependency-analysis)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting Guide](#troubleshooting-guide)
9. [Conclusion](#conclusion)
10. [Appendices](#appendices)

## Introduction
SplitBill is an expense splitting platform designed to track shared expenses and optimize payment distributions among group members. It enables users to create bill sessions, record items with participants, and compute fair settlements that minimize the number of transfers required. The application targets everyday scenarios such as trips, shared meals, and group activities where accurate and transparent cost allocation matters.

Key capabilities:
- Create and manage bill sessions with metadata (event name, dates, description)
- Add items with pricing and assignees (participants) using either proportional ratios or fixed nominal amounts
- Compute participant totals, tax/service breakdowns, and net balances
- Generate transfer instructions to settle debts efficiently

Technology stack:
- Blazor Server for interactive UI and real-time updates
- Entity Framework Core with SQLite for data persistence
- TailwindCSS for responsive styling and theming
- .NET 10 runtime

Audience and use cases:
- Friends splitting dinner bills
- Roommates dividing recurring expenses
- Families or teams organizing trips and outings
- Anyone needing a no-login, frictionless way to track shared costs and settle balances

## Project Structure
The project follows a layered structure with clear separation between UI, data, services, and styling:
- UI layer: Blazor Server components and pages under Components/
- Data layer: Entity models and DbContext under Data/
- Business logic: Settlement calculations under Services/
- Styling: TailwindCSS integration via Styles/ and compiled output in wwwroot/
- Build and tooling: .NET project file, npm scripts, and Tailwind config

```mermaid
graph TB
subgraph "UI Layer"
Routes["Components/Routes.razor"]
MainLayout["Components/Layout/MainLayout.razor"]
Home["Components/Pages/Home.razor"]
end
subgraph "Services"
Settlement["Services/SettlementService.cs"]
end
subgraph "Data Layer"
DbContext["Data/AppDbContext.cs"]
Bill["Data/Entities/Bill.cs"]
Item["Data/Entities/Item.cs"]
Participant["Data/Entities/Participant.cs"]
ItemAssignee["Data/Entities/ItemAssignee.cs"]
end
subgraph "Styling"
Styles["Styles/app.css"]
TailwindCfg["tailwind.config.js"]
WWW["wwwroot/app.css"]
end
subgraph "Runtime"
Program["Program.cs"]
Proj["split_bill.csproj"]
Settings["appsettings.json"]
end
Routes --> MainLayout
Routes --> Home
Home --> DbContext
DbContext --> Bill
DbContext --> Item
DbContext --> Participant
DbContext --> ItemAssignee
Settlement --> Bill
Settlement --> Item
Settlement --> Participant
Settlement --> ItemAssignee
Styles --> TailwindCfg
TailwindCfg --> WWW
Program --> DbContext
Program --> Routes
Program --> Settings
Proj --> Program
```

**Diagram sources**
- [Program.cs:1-73](file://Program.cs#L1-L73)
- [split_bill.csproj:1-34](file://split_bill.csproj#L1-L34)
- [appsettings.json:1-10](file://appsettings.json#L1-L10)
- [Components/Routes.razor:1-7](file://Components/Routes.razor#L1-L7)
- [Components/Layout/MainLayout.razor:1-12](file://Components/Layout/MainLayout.razor#L1-L12)
- [Components/Pages/Home.razor:1-325](file://Components/Pages/Home.razor#L1-L325)
- [Data/AppDbContext.cs:1-71](file://Data/AppDbContext.cs#L1-L71)
- [Data/Entities/Bill.cs:1-38](file://Data/Entities/Bill.cs#L1-L38)
- [Data/Entities/Item.cs:1-28](file://Data/Entities/Item.cs#L1-L28)
- [Data/Entities/Participant.cs:1-21](file://Data/Entities/Participant.cs#L1-L21)
- [Data/Entities/ItemAssignee.cs:1-22](file://Data/Entities/ItemAssignee.cs#L1-L22)
- [Services/SettlementService.cs:1-314](file://Services/SettlementService.cs#L1-L314)
- [Styles/app.css](file://Styles/app.css)
- [tailwind.config.js](file://tailwind.config.js)
- [wwwroot/app.css](file://wwwroot/app.css)

**Section sources**
- [Program.cs:1-73](file://Program.cs#L1-L73)
- [split_bill.csproj:1-34](file://split_bill.csproj#L1-L34)
- [appsettings.json:1-10](file://appsettings.json#L1-L10)
- [Components/Routes.razor:1-7](file://Components/Routes.razor#L1-L7)
- [Components/Layout/MainLayout.razor:1-12](file://Components/Layout/MainLayout.razor#L1-L12)
- [Components/Pages/Home.razor:1-325](file://Components/Pages/Home.razor#L1-L325)
- [Data/AppDbContext.cs:1-71](file://Data/AppDbContext.cs#L1-L71)
- [Data/Entities/Bill.cs:1-38](file://Data/Entities/Bill.cs#L1-L38)
- [Data/Entities/Item.cs:1-28](file://Data/Entities/Item.cs#L1-L28)
- [Data/Entities/Participant.cs:1-21](file://Data/Entities/Participant.cs#L1-L21)
- [Data/Entities/ItemAssignee.cs:1-22](file://Data/Entities/ItemAssignee.cs#L1-L22)
- [Services/SettlementService.cs:1-314](file://Services/SettlementService.cs#L1-L314)
- [Styles/app.css](file://Styles/app.css)
- [tailwind.config.js](file://tailwind.config.js)
- [wwwroot/app.css](file://wwwroot/app.css)

## Core Components
This section introduces the primary building blocks that define SplitBill’s functionality.

- Application bootstrap and DI registration
  - Registers Blazor components, Entity Framework with SQLite, and the settlement service
  - Configures development behavior (schema creation, local database deletion)
  - Sets up static assets, routing, and HTTPS/HSTS in production

- Data model and persistence
  - AppDbContext defines entity sets and relationships
  - Entities represent bills, participants, items, assignees, and payers
  - Soft-delete filters and cascade deletes ensure data integrity

- Settlement engine
  - Computes participant totals, tax/service portions, and balances
  - Generates transfer instructions either via a designated collector or via a cash-flow minimization algorithm

- UI and routing
  - Routes and layout define the Blazor Server rendering pipeline
  - Home page provides a landing experience and initiates bill creation

Practical examples
- Creating a trip bill with three participants and multiple items
- Adding items with mixed assignment modes (ratio and nominal)
- Generating settlement instructions after recording payments

**Section sources**
- [Program.cs:1-73](file://Program.cs#L1-L73)
- [Data/AppDbContext.cs:1-71](file://Data/AppDbContext.cs#L1-L71)
- [Data/Entities/Bill.cs:1-38](file://Data/Entities/Bill.cs#L1-L38)
- [Data/Entities/Item.cs:1-28](file://Data/Entities/Item.cs#L1-L28)
- [Data/Entities/Participant.cs:1-21](file://Data/Entities/Participant.cs#L1-L21)
- [Data/Entities/ItemAssignee.cs:1-22](file://Data/Entities/ItemAssignee.cs#L1-L22)
- [Services/SettlementService.cs:1-314](file://Services/SettlementService.cs#L1-L314)
- [Components/Routes.razor:1-7](file://Components/Routes.razor#L1-L7)
- [Components/Layout/MainLayout.razor:1-12](file://Components/Layout/MainLayout.razor#L1-L12)
- [Components/Pages/Home.razor:1-325](file://Components/Pages/Home.razor#L1-L325)

## Architecture Overview
SplitBill uses a straightforward, cohesive architecture:
- UI: Blazor Server renders Razor components and handles interactivity
- Domain: Entities encapsulate the bill, items, participants, and assignees
- Persistence: Entity Framework Core manages SQLite storage
- Business logic: SettlementService computes balances and transfer instructions
- Styling: TailwindCSS compiles styles during build

```mermaid
graph TB
Browser["Browser Client"] --> Blazor["Blazor Server (.NET 10)"]
Blazor --> DbContext["Entity Framework Core"]
DbContext --> SQLite["SQLite Database"]
Blazor --> RoutesComp["Routing (Routes.razor)"]
Blazor --> LayoutComp["Layout (MainLayout.razor)"]
Blazor --> HomeComponent["Home Page (Home.razor)"]
Blazor --> SettlementSvc["SettlementService.cs"]
SettlementSvc --> Entities["Entities (Bill, Item, Participant, ItemAssignee)"]
Styles["TailwindCSS (Styles/app.css)"] --> TailwindCfg["tailwind.config.js"]
TailwindCfg --> CompiledCSS["wwwroot/app.css"]
Blazor --> CompiledCSS
```

**Diagram sources**
- [Program.cs:1-73](file://Program.cs#L1-L73)
- [Components/Routes.razor:1-7](file://Components/Routes.razor#L1-L7)
- [Components/Layout/MainLayout.razor:1-12](file://Components/Layout/MainLayout.razor#L1-L12)
- [Components/Pages/Home.razor:1-325](file://Components/Pages/Home.razor#L1-L325)
- [Data/AppDbContext.cs:1-71](file://Data/AppDbContext.cs#L1-L71)
- [Data/Entities/Bill.cs:1-38](file://Data/Entities/Bill.cs#L1-L38)
- [Data/Entities/Item.cs:1-28](file://Data/Entities/Item.cs#L1-L28)
- [Data/Entities/Participant.cs:1-21](file://Data/Entities/Participant.cs#L1-L21)
- [Data/Entities/ItemAssignee.cs:1-22](file://Data/Entities/ItemAssignee.cs#L1-L22)
- [Services/SettlementService.cs:1-314](file://Services/SettlementService.cs#L1-L314)
- [Styles/app.css](file://Styles/app.css)
- [tailwind.config.js](file://tailwind.config.js)
- [wwwroot/app.css](file://wwwroot/app.css)

## Detailed Component Analysis

### Data Model and Relationships
The data model centers around a Bill session containing Participants, Items, ItemAssignees, and Payers. Relationships and constraints ensure referential integrity and cascading deletes.

```mermaid
classDiagram
class Bill {
+int Id
+string Uuid
+string EventName
+DateTime CreatedAt
+ChargeType TaxType
+decimal TaxValue
+ChargeType ServiceChargeType
+decimal ServiceChargeValue
+string CreatorToken
+int? CollectorParticipantId
+bool IsDeleted
+string Description
+DateTime? StartDate
+DateTime? EndDate
+string Status
+ICollection~Participant~ Participants
+ICollection~Item~ Items
+ICollection~Payer~ Payers
}
class Participant {
+int Id
+int BillId
+string Name
+bool IsDeleted
+string Email
+string AvatarUrl
+Bill Bill
+ICollection~ItemAssignee~ AssignedItems
+ICollection~Payer~ Payments
}
class Item {
+int Id
+int BillId
+string Name
+decimal Price
+ChargeType TaxType
+decimal TaxValue
+ChargeType ServiceChargeType
+decimal ServiceChargeValue
+int? FrontedByParticipantId
+bool IsDeleted
+string Category
+DateTime Date
+string Notes
+Bill Bill
+Participant FrontedBy
+ICollection~ItemAssignee~ Assignees
}
class ItemAssignee {
+int Id
+int ItemId
+int ParticipantId
+AssigneeInputType InputType
+decimal ShareRatio
+decimal NominalAmount
+Item Item
+Participant Participant
}
class Payer {
+int Id
+int BillId
+int ParticipantId
+decimal AmountPaid
+DateTime RecordedAt
}
Bill "1" --> "many" Participant : "has"
Bill "1" --> "many" Item : "has"
Bill "1" --> "many" Payer : "has"
Item "1" --> "many" ItemAssignee : "has"
Participant "1" --> "many" ItemAssignee : "has"
ItemAssignee "many" --> "1" Participant : "participant"
ItemAssignee "many" --> "1" Item : "item"
Payer "many" --> "1" Participant : "participant"
Payer "many" --> "1" Bill : "bill"
```

**Diagram sources**
- [Data/Entities/Bill.cs:1-38](file://Data/Entities/Bill.cs#L1-L38)
- [Data/Entities/Participant.cs:1-21](file://Data/Entities/Participant.cs#L1-L21)
- [Data/Entities/Item.cs:1-28](file://Data/Entities/Item.cs#L1-L28)
- [Data/Entities/ItemAssignee.cs:1-22](file://Data/Entities/ItemAssignee.cs#L1-L22)
- [Data/AppDbContext.cs:1-71](file://Data/AppDbContext.cs#L1-L71)

**Section sources**
- [Data/Entities/Bill.cs:1-38](file://Data/Entities/Bill.cs#L1-L38)
- [Data/Entities/Participant.cs:1-21](file://Data/Entities/Participant.cs#L1-L21)
- [Data/Entities/Item.cs:1-28](file://Data/Entities/Item.cs#L1-L28)
- [Data/Entities/ItemAssignee.cs:1-22](file://Data/Entities/ItemAssignee.cs#L1-L22)
- [Data/AppDbContext.cs:1-71](file://Data/AppDbContext.cs#L1-L71)

### Settlement Calculation Workflow
The settlement engine computes totals, taxes, and balances, then produces transfer instructions. The process includes:
- Aggregating item prices and back-calculating tax/service portions
- Distributing item costs among assignees using either nominal or ratio-based sharing
- Summing participant payments and computing balances
- Producing transfer instructions either via a designated collector or via a greedy minimization algorithm

```mermaid
flowchart TD
Start(["Start Settlement"]) --> LoadData["Load active Items and Participants"]
LoadData --> ComputeTotals["Compute Food Subtotal, Tax Total, Service Total, Grand Total"]
ComputeTotals --> InitSummaries["Initialize Participant Summaries with Total Paid"]
InitSummaries --> DistributeItemCosts["Distribute Item Price Among Assignees"]
DistributeItemCosts --> ComputeBalances["Compute Totals Owed and Balances"]
ComputeBalances --> DecideCollector{"Collector Set?"}
DecideCollector --> |Yes| FromCollector["Generate Instructions via Collector"]
DecideCollector --> |No| MinimizeCF["Run Minimize Cash Flow Algorithm"]
FromCollector --> End(["Return Settlement Result"])
MinimizeCF --> End
```

**Diagram sources**
- [Services/SettlementService.cs:55-232](file://Services/SettlementService.cs#L55-L232)

**Section sources**
- [Services/SettlementService.cs:1-314](file://Services/SettlementService.cs#L1-L314)

### UI Creation Flow (Beginner-Friendly)
The Home page provides a guided flow to create a new bill session:
- Users click “Start Now” to open a modal
- They enter event details (name, description, dates)
- On submission, a new bill is created with a unique identifier and creator token
- The user is navigated to the bill details page

```mermaid
sequenceDiagram
participant User as "User"
participant Home as "Home.razor"
participant DB as "AppDbContext"
participant Router as "Routes.razor"
User->>Home : Click "Mulai Sekarang"
Home->>Home : OpenModal()
User->>Home : Fill form and submit
Home->>DB : Save new Bill
Home->>Home : Store Creator Token in localStorage
Home->>Router : Navigate to "/bill/{uuid}"
```

**Diagram sources**
- [Components/Pages/Home.razor:241-288](file://Components/Pages/Home.razor#L241-L288)
- [Data/AppDbContext.cs:12-16](file://Data/AppDbContext.cs#L12-L16)
- [Components/Routes.razor:1-7](file://Components/Routes.razor#L1-L7)

**Section sources**
- [Components/Pages/Home.razor:1-325](file://Components/Pages/Home.razor#L1-L325)
- [Data/AppDbContext.cs:1-71](file://Data/AppDbContext.cs#L1-L71)
- [Components/Routes.razor:1-7](file://Components/Routes.razor#L1-L7)

### Technical Highlights for Experienced Developers
- Blazor Server interactivity and SignalR transport enable real-time updates without complex client-side state management
- Entity Framework Core with SQLite offers simplicity and portability; migrations and snapshot files support iterative schema evolution
- TailwindCSS integration via npm scripts ensures efficient CSS compilation and hot-reload during development
- Soft-delete filters and cascade deletes simplify data lifecycle management
- SettlementService implements a robust algorithm that supports inclusive tax/service calculations and flexible assignee input types

**Section sources**
- [Program.cs:1-73](file://Program.cs#L1-L73)
- [split_bill.csproj:29-31](file://split_bill.csproj#L29-L31)
- [Data/AppDbContext.cs:18-70](file://Data/AppDbContext.cs#L18-L70)
- [Services/SettlementService.cs:43-314](file://Services/SettlementService.cs#L43-L314)

## Dependency Analysis
High-level dependencies:
- Program.cs registers services and configures the HTTP pipeline
- Components depend on Data for entity access and Services for business logic
- TailwindCSS depends on Tailwind config and compiles to wwwroot for runtime consumption

```mermaid
graph LR
Program["Program.cs"] --> DbContext["Data/AppDbContext.cs"]
Program --> Routes["Components/Routes.razor"]
Program --> Settings["appsettings.json"]
Home["Components/Pages/Home.razor"] --> DbContext
Home --> Routes
Settlement["Services/SettlementService.cs"] --> DbContext
Settlement --> Bill["Data/Entities/Bill.cs"]
Settlement --> Item["Data/Entities/Item.cs"]
Settlement --> Participant["Data/Entities/Participant.cs"]
Settlement --> ItemAssignee["Data/Entities/ItemAssignee.cs"]
Styles["Styles/app.css"] --> TailwindCfg["tailwind.config.js"]
TailwindCfg --> WWW["wwwroot/app.css"]
```

**Diagram sources**
- [Program.cs:1-73](file://Program.cs#L1-L73)
- [Components/Pages/Home.razor:1-325](file://Components/Pages/Home.razor#L1-L325)
- [Data/AppDbContext.cs:1-71](file://Data/AppDbContext.cs#L1-L71)
- [Services/SettlementService.cs:1-314](file://Services/SettlementService.cs#L1-L314)
- [Styles/app.css](file://Styles/app.css)
- [tailwind.config.js](file://tailwind.config.js)
- [wwwroot/app.css](file://wwwroot/app.css)

**Section sources**
- [Program.cs:1-73](file://Program.cs#L1-L73)
- [Components/Pages/Home.razor:1-325](file://Components/Pages/Home.razor#L1-L325)
- [Data/AppDbContext.cs:1-71](file://Data/AppDbContext.cs#L1-L71)
- [Services/SettlementService.cs:1-314](file://Services/SettlementService.cs#L1-L314)
- [Styles/app.css](file://Styles/app.css)
- [tailwind.config.js](file://tailwind.config.js)
- [wwwroot/app.css](file://wwwroot/app.css)

## Performance Considerations
- SQLite is well-suited for small to medium workloads typical of expense splitting; keep item counts reasonable for frequent real-time updates
- Settlement computations are linear in the number of items and participants; avoid excessive granularity for very large groups
- TailwindCSS compilation occurs during build; leverage watch mode for rapid iteration during development
- Blazor Server streaming and SignalR help reduce payload sizes; avoid unnecessary re-rendering by structuring components thoughtfully

[No sources needed since this section provides general guidance]

## Troubleshooting Guide
Common issues and resolutions:
- Database initialization in development
  - The application attempts to delete existing databases and recreate schema during development startup
  - If conflicts occur, ensure no process holds the database file open

- Routing and 404 handling
  - Non-existent routes render a dedicated Not Found page
  - Ensure route definitions align with component placement

- Static assets and CSS
  - TailwindCSS compiles from Styles/app.css to wwwroot/app.css
  - Verify npm scripts and Tailwind configuration are present and functional

- Authentication and session access
  - Creator access is determined by a stored creator token in localStorage
  - Confirm token storage and retrieval via JavaScript interop

**Section sources**
- [Program.cs:26-53](file://Program.cs#L26-L53)
- [Components/Routes.razor:1-7](file://Components/Routes.razor#L1-L7)
- [package.json:6-10](file://package.json#L6-L10)
- [tailwind.config.js](file://tailwind.config.js)
- [wwwroot/app.css](file://wwwroot/app.css)

## Conclusion
SplitBill delivers a focused, no-login solution for splitting shared expenses. Its Blazor Server UI, SQLite-backed data model, and efficient settlement algorithm combine to offer an intuitive experience for both beginners and developers. The modular architecture and clear separation of concerns make it straightforward to extend and maintain.

[No sources needed since this section summarizes without analyzing specific files]

## Appendices

### Practical Examples
- Scenario: Three friends go on a trip and split a meal
  - Create a bill session, add participants, record items with proportional sharing, and review transfer instructions
- Scenario: Mixed sharing (some items paid by one person, others split)
  - Use nominal amounts for specific items and ratios for shared costs; the settlement engine accounts for both
- Scenario: Group travel with varying contributions
  - Record payments made by participants; the system computes balances and suggests minimal transfers

[No sources needed since this section provides general guidance]