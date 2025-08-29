<p>
  <img src="assets/coin-wide-logo.png" alt="CoIn Logo"/>
</p>

**CoIn** (Command Invoker) is a lightweight framework for organizing application logic through commands and their handlers.  

# DioRed.CoIn

Lightweight command invocation framework.  
Alternative to MediatR with **synchronous and asynchronous handler support** + fallback.

---

## Install

```bash
dotnet add package DioRed.CoIn
```

## Define commands

With result:
```cs
public record CreateUser(string Name, string Email) : ICommand<Guid>;
```

Without result:
```cs
public record SendEmail(string To, string Subject, string Body) : ICommand;
```

---

## Create handlers

Synchronous:
```cs
public class CreateUserHandler : ICommandHandler<CreateUser, Guid>
{
    public Guid Handle(CreateUser command)
    {
        Console.WriteLine($"Creating user: {command.Name} ({command.Email})");
        return Guid.NewGuid();
    }
}

public class SendEmailHandler : ICommandHandler<SendEmail>
{
    public void Handle(SendEmail command)
    {
        Console.WriteLine($"Sending email to {command.To}: {command.Subject}");
    }
}
```

Asynchronous:
```cs
public class CreateUserAsyncHandler : IAsyncCommandHandler<CreateUser, Guid>
{
    public async Task<Guid> HandleAsync(CreateUser command, CancellationToken ct)
    {
        await Task.Delay(100, ct); // fake async work
        Console.WriteLine($"[Async] Creating user: {command.Name} ({command.Email})");
        return Guid.NewGuid();
    }
}

public class SendEmailAsyncHandler : IAsyncCommandHandler<SendEmail>
{
    public async Task HandleAsync(SendEmail command, CancellationToken ct)
    {
        await Task.Delay(50, ct);
        Console.WriteLine($"[Async] Sending email to {command.To}: {command.Subject}");
    }
}
```

---

## Register handlers

In `Program.cs`:
```cs
using DioRed.CoIn;

builder.Services.AddCoIn(typeof(CreateUser).Assembly);
```

`AddCoIn` will:
- Register **all handlers** from the given assemblies
- Register a singleton `IInvoker` for invoking commands

---

## Use the invoker
```cs
public class UserService
{
    private readonly IInvoker _invoker;

    public UserService(IInvoker invoker) => _invoker = invoker;

    public Guid CreateUserSync(string name, string email)
    {
        var cmd = new CreateUser(name, email);
        return _invoker.Invoke(cmd);
        // Uses sync handler if available
        // Otherwise async handler executed via GetAwaiter().GetResult()
    }

    public async Task<Guid> CreateUserAsync(string name, string email, CancellationToken ct)
    {
        var cmd = new CreateUser(name, email);
        return await _invoker.InvokeAsync(cmd, ct);
        // Prefers async handler
        // Falls back to sync handler wrapped into Task.FromResult
    }

    public void SendEmail(string to, string subject, string body)
    {
        var cmd = new SendEmail(to, subject, body);
        _invoker.Invoke(cmd);
    }

    public Task SendEmailAsync(string to, string subject, string body, CancellationToken ct)
    {
        var cmd = new SendEmail(to, subject, body);
        return _invoker.InvokeAsync(cmd, ct);
    }
}
```

---

## Fallbacks

- Calling `Invoke(...)` with only an **async handler** → framework executes it synchronously
- Calling `InvokeAsync(...)` with only a **sync handler** → framework wraps result into a completed task

So the **caller never needs to care** which handler type is actually registered.

---