using Flurl.Http;
using Microsoft.Extensions.Configuration;

switch (args.FirstOrDefault()?.ToLowerInvariant())
{
    case null:
        await Solver.SolveLast(opt => opt.ClearConsole = false);
        break;
    case "all":
        await Solver.SolveAll(opt =>
        {
            opt.ShowConstructorElapsedTime = true;
            opt.ShowTotalElapsedTimePerDay = true;
        });
        break;
    case "new":
        DateTime currentDate = DateTime.Now;
        await CreateNewDay(uint.TryParse(args.ElementAtOrDefault(1), out uint day) ? new DateTime(currentDate.Year, currentDate.Month, (int)day) : currentDate);

        break;
    default:
        {
            var indexes = args.Select(arg => uint.TryParse(arg, out uint index) ? index : uint.MaxValue);

            await Solver.Solve(indexes.Where(i => i < uint.MaxValue));
            break;
        }
}

public sealed partial class Program
{
    public static readonly IConfigurationRoot Configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

    public static async Task CreateNewDay(DateTime date)
    {
        await GenerateDayClass(date);
        await DownloadDayInput(date);
    }

    public static async Task GenerateDayClass(DateTime date)
    {
        await File.WriteAllTextAsync(
            $"../../../Day{date:dd}.cs",
            $$"""
              namespace AdventOfCode;

              /// <summary>
              ///     Day {{date:dd}}: <see href="https://adventofcode.com/{{date:yyyy}}/day/{{date:%d}}"/>
              /// </summary>
              public sealed class Day{{date:dd}} : BaseDay
              {
                  private readonly string _input;
              
                  public Day{{date:dd}}()
                  {
                      _input = File.ReadAllText(InputFilePath);
                  }
              
                  public override ValueTask<string> Solve_1()
                  {
                      return ValueTask.FromResult($"Solution to day {CalculateIndex()}, part 1");
                  }
              
                  public override ValueTask<string> Solve_2()
                  {
                      return ValueTask.FromResult($"Solution to day {CalculateIndex()}, part 2");
                  }
              }

              """);
    }

    public static async Task DownloadDayInput(DateTime date)
    {
        await using Stream response = await $"https://adventofcode.com/{date:yyyy}/day/{date:%d}/input"
            .WithCookie("session", Configuration["sessionCookie"])
            .GetStreamAsync();

        await using FileStream outputFileStream = new($"../../../Inputs/{date:dd}.txt", FileMode.Create);
        await response.CopyToAsync(outputFileStream);
    }
}