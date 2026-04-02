# Release & Task Management Tool – Development Guide

## Overview

This project is a small internal tool designed to manage releases and tasks across Product Management (PM), Development, and QA teams. The goal is to ensure transparency, accountability, and smooth collaboration.

---

## Roles & Responsibilities

### PM (Product Manager)

* Create, update, and delete Releases
* Create, update, and delete Tasks

### Development

* Update task status
* Add PR link
* Add remarks

### QA

* Update task status
* Add approver name
* Add remarks

---

## Core Data Model

### Release

* Title
* Description
* StartDate
* EndDate
* Status:

  * Upcoming
  * Active
  * Shipped
  * Cancelled

### Task

* Title
* AssignedTo
* PRLink
* ApproverName
* Remarks
* Status Flow:

  * Pending
  * InProgress
  * PRRaised
  * Merged
  * Deployed
  * Done

---

## Technical Stack

* .NET 10 Web API
* Entity Framework Core
* SQL Database
* JWT Authentication

---

## Architecture & Layering

Follow strict separation of concerns:

```
Controller → Service → Repository → Database
```

### Controller Layer

* Thin layer
* Handles HTTP requests/responses only
* No business logic

### Service Layer

* Contains business logic
* Handles role-based rules
* Coordinates repositories

### Repository Layer

* Data access logic only
* No business rules

---

## Project Structure (Suggested)

```
/src
  /Controllers
  /Services
  /Repositories
  /DTOs
  /Entities
  /Configurations (Fluent API)
  /Enums
  /Middleware
  /Validators
  /Auth
```

---

## Entity Framework Configuration

* Use Fluent API (NOT Data Annotations for DB mapping)
* Keep configurations in separate classes
* Apply via `IEntityTypeConfiguration<T>`

Example:

```csharp
public class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    public void Configure(EntityTypeBuilder<Task> builder)
    {
        builder.Property(t => t.Title)
               .IsRequired()
               .HasMaxLength(200);
    }
}
```

---

## DTO Usage

* Always use DTOs
* Never expose entities directly

Examples:

* CreateTaskDto
* UpdateTaskDto
* TaskResponseDto

---

## Dependency Injection

* Use built-in DI container
* Register services and repositories in `Program.cs`
* Avoid manual instantiation (`new` inside logic)

---

## Validation Strategy

Use centralized validation:

Options:

* Data Annotations (simple)
* FluentValidation (preferred for scalability)

Rules:

* Keep validation in one place
* Do not scatter validation across layers

Example:

```csharp
public class CreateTaskValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
    }
}
```

---

## Error Handling

* Use global exception middleware
* Return consistent API responses

Example response:

```json
{
  "success": false,
  "message": "Validation failed",
  "errors": ["Title is required"]
}
```

---

## Authentication & Security

* JWT-based authentication
* Role-based authorization

### Requirements

* Passwords must be hashed (e.g., BCrypt)
* JWT secrets stored in configuration (appsettings / environment variables)
* Do NOT hardcode secrets

### Role Enforcement

* Apply `[Authorize]` at controller level
* ALSO enforce roles inside service layer (critical)

---

## Business Rules

### Task Status Flow (Strict Order)

```
Pending → InProgress → PRRaised → Merged → Deployed → Done
```

* No skipping steps
* Validate transitions inside service layer

---

## Design Patterns (Use Where Appropriate)

Suggested patterns:

1. **Strategy Pattern**

   * For handling task status transitions

2. **Factory Pattern**

   * For creating domain objects cleanly

Avoid overengineering.

---

## Coding Guidelines

* Follow Single Responsibility Principle
* Keep classes small and focused
* Avoid large files handling multiple concerns
* Avoid long if/else chains

  * Prefer mapping or pattern-based approaches
* Write readable and maintainable code

---

## Focus Areas

* Clean architecture
* Proper separation of concerns
* Maintainability
* Readability
* Consistent structure

---

## Deliverable Expectations

* Well-structured project
* Clean layering
* Proper use of DTOs
* Fluent API configurations
* JWT authentication implemented
* Role-based access enforced
* Centralized validation
* Consistent error handling

---

## Notes

This is a POC, but should reflect production-quality thinking in terms of structure and practices.
