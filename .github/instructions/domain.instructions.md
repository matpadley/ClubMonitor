---
description: "Use when creating or modifying domain entities, value objects, strongly-typed IDs, or domain exceptions. Enforces zero-framework-dependency rule, private constructors, and manual equality for value objects."
applyTo: "src/Domain/**"
---

## Zero Framework Dependencies

`src/Domain` must have **no NuGet packages** and no framework references. Do not add EF Core, ASP.NET, or any DI attributes here.

## Entities

```csharp
public sealed class Member
{
    public MemberId Id { get; private set; }
    public string Name { get; private set; } = default!;
    // ... more properties

    private Member() { }   // required by EF Core — keep private

    public static Member Create(string name, Email email)
    {
        // validate here, then set
        return new Member { Id = MemberId.New(), ... };
    }
}
```

- Private parameterless constructor (EF Core only).
- All properties have `private set`.
- All creation goes through `static Create(...)`.

## Value Objects

```csharp
public sealed class Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        // trim, lowercase, validate
        return new Email(value.Trim().ToLowerInvariant());
    }

    public override bool Equals(object? obj) =>
        obj is Email other && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override int GetHashCode() =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    public override string ToString() => Value;
}
```

- `sealed class` with private constructor.
- Implement `Equals` / `GetHashCode` manually — do **not** rely on record equality for classes.
- `static Create(string)` validates and normalises.

## Strongly-Typed IDs

```csharp
public readonly record struct MemberId(Guid Value)
{
    public static MemberId New()         => new(Guid.NewGuid());
    public static MemberId From(Guid g)  => new(g);
}
```

- `readonly record struct` — value-type semantics; record gives `Equals`/`GetHashCode`/`ToString` for free.
- `New()` and `From(Guid)` are the only construction paths.

## Domain Exceptions

```csharp
public sealed class DuplicateEmailException(string email)
    : Exception($"A member with email '{email}' already exists.");
```

- `sealed class`, primary constructor syntax.
- Inherits `Exception` directly.
- Message baked into the base constructor call.
