using CommandLine;
using Innovator.Client;
using Innovator.Client.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InnovatorAdmin.Cmd
{
  class Program
  {
    static async Task<int> Main(string[] args)
    {
      //var parser = new Parser(with => with.IgnoreUnknownArguments = false);
      var cmdArgs = Parser.Default.ParseArguments<ExportOptions, ImportOptions, PackageDiffOptions, ConvertOptions>(args);
      var task = default(Task<int>);
      cmdArgs
        .WithParsed<ExportOptions>(o => task = o.Execute())
        .WithParsed<PackageDiffOptions>(o => task = Task.FromResult(o.Execute()))
        .WithParsed<ImportOptions>(o => task = o.Execute())
        .WithParsed<ConvertOptions>(o => task = Task.FromResult(o.Execute()))
        .WithNotParsed(err => task = TryParseArasFormat(cmdArgs, args));
      var result = await task;
#if DEBUG
      Console.ReadLine();
#endif
      return result;
    }

    private static Task<int> TryParseArasFormat(ParserResult<object> result, string[] args)
    {
      var opts = default(SharedOptions);
      if (result.TypeInfo.Choices.Any())
      {
        opts = new ExportOptions();
      }
      else if (typeof(SharedOptions).IsAssignableFrom(result.TypeInfo.Current))
      {
        opts = (SharedOptions)Activator.CreateInstance(result.TypeInfo.Current);
      }
      else
      {
        return Task.FromResult(-1);
      }

      foreach (var arg in args)
      {
        var idx = arg.IndexOf('=');
        if (idx > 0)
        {
          switch (arg.Substring(0, idx).ToLowerInvariant())
          {
            case "server":
              opts.Url = arg.Substring(idx + 1);
              break;
            case "database":
              opts.Database = arg.Substring(idx + 1);
              break;
            case "login":
              opts.Username = arg.Substring(idx + 1);
              break;
            case "password":
              opts.Password = arg.Substring(idx + 1);
              break;
            case "mffile":
              opts.InputFile = arg.Substring(idx + 1);
              break;
            case "dir":
              if (opts is ExportOptions e2)
                e2.Output = arg.Substring(idx + 1);
              break;
          }
        }
      }

      if (string.IsNullOrEmpty(opts.Url)
        || string.IsNullOrEmpty(opts.Database))
        return Task.FromResult(-1);

      if (opts is ExportOptions export)
        return export.Execute();
      else if (opts is ImportOptions import)
        return import.Execute();
      return Task.FromResult(-1);
    }
  }
}
