using DioRed.CoIn;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddCoIn(typeof(Program).Assembly);

var app = builder.Build();

var invoker = app.Services.GetRequiredService<IInvoker>();

// sync
invoker.Invoke(new PrintToConsole("Hello CoIn!"));

int ssum = invoker.Invoke(new AddNumbers(2, 3));
Console.WriteLine(ssum);

string suser = invoker.Invoke(new GetUser(42));
Console.WriteLine(suser);

// async
await invoker.InvokeAsync(new PrintToConsole("Hello async CoIn!"));

int asum = await invoker.InvokeAsync(new AddNumbers(2, 3));
Console.WriteLine(asum);

var auser = await invoker.InvokeAsync(new GetUser(42));
Console.WriteLine(auser);
return;



public record PrintToConsole(string Message) : ICommand;
public record AddNumbers(int A, int B) : ICommand<int>;

public class PrintToConsoleHandler : ICommandHandler<PrintToConsole>
{
    public void Handle(PrintToConsole command) =>
        Console.WriteLine(command.Message);
}

public class AddNumbersHandler : ICommandHandler<AddNumbers, int>
{
    public int Handle(AddNumbers command) => command.A + command.B;
}

public record GetUser(int Id) : ICommand<string>;

public class GetUserHandler : IAsyncCommandHandler<GetUser, string>
{
    public async Task<string> HandleAsync(GetUser command, CancellationToken ct = default)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), ct); // имитация работы
        return $"User {command.Id}";
    }
}