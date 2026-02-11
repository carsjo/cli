using SampleCli.Commands;
using SampleCli.Extensions;

namespace SampleCli;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        await using var output = new StringWriter();
        await using var error = new StringWriter();
        await RedMarbleRootCommand.InvokeAsync([
            "-e",
            "Production",
            "greet",
            "John",
            "Doe"
        ], output, error);
        var result = output.ToString();
        Assert.Contains("Hello, John Doe! Production", result);
    }
    
    [Fact]
    public async Task Test2()
    {
        var command = new GreetCommand();
        var result = await command.InvokeAsync([
            "-e",
            "Production",
            "John",
            "Doe"
        ]);
        Assert.Contains("Hello, John Doe! Production", result.Output);
    }
}