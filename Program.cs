﻿using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace rename
{
    [JsonSerializable(typeof(List<Selected>))]
    internal partial class SelectedSerializerContext : JsonSerializerContext
    {
    }

    public class Selected
    {
        [JsonPropertyName("id")] public required string Id { get; set; }
        [JsonPropertyName("path")] public required string Path { get; set; }
        [JsonPropertyName("dateCreated")] public required DateTime DateCreated { get; set; }
        [JsonPropertyName("dateModified")] public required DateTime DateModified { get; set; }
        [JsonPropertyName("fileSize")] public required long FileSize { get; set; }

        public override string ToString()
        {
            return $"{Path} - {DateCreated} - {DateModified} - {(FileSize == -1 ? "Is a directory" : FileSize)}";
        }
    }

    internal static class Program
    {
        private static List<Selected> _selected = new();

        static void Cd(string cmd)
        {
            try
            {
                List<string> parsed = cmd.Split(" ").ToList();
                parsed.RemoveAll(i => i == "");
                Directory.SetCurrentDirectory(parsed[1]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [UnconditionalSuppressMessage("Trimming",
            "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
            Justification = "TypeInfo false positive. Suppression is OK here.")]
        [UnconditionalSuppressMessage("AOT",
            "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.",
            Justification = "TypeInfo false positive. Suppression is OK here.")]
        static void SelectWhere(string cmd)
        {
            var args = cmd.Substring(cmd.IndexOf('(') + 1, cmd.IndexOf(')') - (cmd.IndexOf('(') + 1));
            Console.WriteLine(args);
            var argList = args.Split(',');
            for (int i = 0; i < argList.Length; i++)
            {
                argList[i] = argList[i].Trim();
                Console.WriteLine(argList[i]);
            }

            if (argList.Count() != 4)
            {
                Console.WriteLine("Syntax error. ");
                return;
            }

            var cList = Directory.EnumerateFileSystemEntries(Directory.GetCurrentDirectory()).ToList();
            var fileNameRule = argList[0];
            var dCRule = argList[1];
            var dMRule = argList[2];
            var fileSizeRule = argList[3];
            if (argList.All(i => i == "*"))
            {
                foreach (var str in cList)
                {
                    var tempObj = new Selected
                    {
                        Id = Guid.NewGuid().ToString(),
                        DateCreated = Directory.GetCreationTime(str),
                        DateModified = Directory.GetLastWriteTime(str),
                        FileSize = File.GetAttributes(str).HasFlag(FileAttributes.Directory)
                            ? -1
                            : new FileInfo(str).Length,
                        Path = str
                    };
                    _selected.Add(tempObj);
                }
            }
            else
            {
                var forExtraction = new List<Selected>();
                foreach (string dir in cList)
                {
                    forExtraction.Add(new Selected
                    {
                        Id = Guid.NewGuid().ToString(),
                        DateCreated = Directory.GetCreationTime(dir),
                        DateModified = Directory.GetLastWriteTime(dir),
                        FileSize = File.GetAttributes(dir).HasFlag(FileAttributes.Directory)
                            ? -1
                            : new FileInfo(dir).Length,
                        Path = dir
                    });
                }

                forExtraction = forExtraction.OrderByDescending(i => i.DateCreated).ToList();
                // Name 
                {
                    forExtraction = forExtraction.Where(i => i.Path.EndsWith(fileNameRule) || fileNameRule == "*")
                        .ToList();
                }
                // Date created 
                {
                    if (dCRule == "*")
                    {
                        goto skipped;
                    }

                    var dCRules = new List<string>();
                    foreach (var str in dCRule.Split("||"))
                    {
                        dCRules.Add(str.Trim());
                    }

                    foreach (var str in dCRules)
                    {
                        if (str.Contains(">") && str.Contains("<"))
                        {
                            return;
                        }

                        if (str.Contains("<="))
                        {
                            forExtraction = forExtraction.Where(i =>
                                i.DateCreated.Trim(TimeSpan.TicksPerSecond).CompareTo(DateTime
                                    .Parse(str.Substring(str.IndexOf("=", StringComparison.Ordinal) + 1))
                                    .Trim(TimeSpan.TicksPerSecond)) <=
                                0).ToList();
                        }
                        else if (str.Contains("<"))
                        {
                            forExtraction = forExtraction.Where(i =>
                                    i.DateCreated.Trim(TimeSpan.TicksPerSecond).CompareTo(DateTime
                                        .Parse(str.Substring(str.IndexOf("<", StringComparison.Ordinal) + 1))
                                        .Trim(TimeSpan.TicksPerSecond)) < 0)
                                .ToList();
                        }
                        else if (str.Contains(">="))
                        {
                            forExtraction = forExtraction.Where(i =>
                                i.DateCreated.Trim(TimeSpan.TicksPerSecond).CompareTo(DateTime
                                    .Parse(str.Substring(str.IndexOf("=", StringComparison.Ordinal) + 1))
                                    .Trim(TimeSpan.TicksPerSecond)) is > 0 or >= 0).ToList();
                        }
                        else if (str.Contains(">"))
                        {
                            forExtraction = forExtraction.Where(i =>
                                    i.DateCreated.Trim(TimeSpan.TicksPerSecond).CompareTo(DateTime
                                        .Parse(str.Substring(str.IndexOf(">", StringComparison.Ordinal) + 1))
                                        .Trim(TimeSpan.TicksPerSecond)) is > 0)
                                .ToList();
                        }
                    }

                    skipped: ;
                }
                // Date modified
                {
                    if (dMRule == "*")
                    {
                        goto skipped;
                    }

                    var dMRules = new List<string>();
                    foreach (var str in dMRule.Split("||"))
                    {
                        dMRules.Add(str.Trim());
                    }

                    foreach (var str in dMRules)
                    {
                        if (str.Contains(">") && str.Contains("<"))
                        {
                            return;
                        }

                        if (str.Contains("<="))
                        {
                            forExtraction = forExtraction.Where(i =>
                                i.DateModified.Trim(TimeSpan.TicksPerSecond).CompareTo(DateTime
                                    .Parse(str.Substring(str.IndexOf("=", StringComparison.Ordinal) + 1))
                                    .Trim(TimeSpan.TicksPerSecond)) is < 0 or <= 0).ToList();
                        }
                        else if (str.Contains("<"))
                        {
                            forExtraction = forExtraction.Where(i =>
                                    i.DateModified.Trim(TimeSpan.TicksPerSecond).CompareTo(DateTime
                                        .Parse(str.Substring(str.IndexOf("<", StringComparison.Ordinal) + 1))
                                        .Trim(TimeSpan.TicksPerSecond)) < 0)
                                .ToList();
                        }
                        else if (str.Contains(">="))
                        {
                            forExtraction = forExtraction.Where(i =>
                                i.DateModified.Trim(TimeSpan.TicksPerSecond).CompareTo(DateTime
                                    .Parse(str.Substring(str.IndexOf("=", StringComparison.Ordinal) + 1))
                                    .Trim(TimeSpan.TicksPerSecond)) >= 0).ToList();
                        }
                        else if (str.Contains(">"))
                        {
                            forExtraction = forExtraction.Where(i =>
                                    i.DateModified.Trim(TimeSpan.TicksPerSecond).CompareTo(DateTime
                                        .Parse(str.Substring(str.IndexOf(">", StringComparison.Ordinal) + 1))
                                        .Trim(TimeSpan.TicksPerSecond)) is > 0)
                                .ToList();
                        }
                    }

                    skipped: ;
                }
                // Filesize
                {
                    if (fileSizeRule == "*")
                    {
                        goto skipped;
                    }

                    var fileSizeRules = new List<string>();
                    foreach (var str in fileSizeRule.Split("||"))
                    {
                        fileSizeRules.Add(str.Trim());
                    }

                    foreach (var str in fileSizeRules)
                    {
                        if (str.Contains(">") && str.Contains("<"))
                        {
                            Console.WriteLine("Syntax error. ");
                            return;
                        }

                        if (str.Contains("<="))
                        {
                            forExtraction = forExtraction
                                .Where(i => i.FileSize <=
                                            Int32.Parse(str.Substring(str.IndexOf("=", StringComparison.Ordinal) + 1)))
                                .ToList();
                        }
                        else if (str.Contains("<"))
                        {
                            forExtraction = forExtraction
                                .Where(i => i.FileSize <
                                            Int32.Parse(str.Substring(str.IndexOf("<", StringComparison.Ordinal) + 1)))
                                .ToList();
                        }
                        else if (str.Contains(">="))
                        {
                            forExtraction = forExtraction
                                .Where(i => i.FileSize >=
                                            Int32.Parse(str.Substring(str.IndexOf("=", StringComparison.Ordinal) + 1)))
                                .ToList();
                        }
                        else if (str.Contains(">"))
                        {
                            forExtraction = forExtraction
                                .Where(i => i.FileSize >
                                            Int32.Parse(str.Substring(str.IndexOf(">", StringComparison.Ordinal) + 1)))
                                .ToList();
                        }
                    }

                    skipped: ;
                }
                _selected.AddRange(forExtraction);
            }

            var se = new JsonSerializerOptions { TypeInfoResolver = SelectedSerializerContext.Default };
            Console.WriteLine(JsonSerializer.Serialize(_selected, se));
        }

        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                foreach (var item in args)
                {
                    Console.WriteLine(item);
                }

                try
                {
                    Directory.SetCurrentDirectory(args[1]);
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Data.Values);
                }
            }


            Console.WriteLine("Hello, World!");
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"[{Directory.GetCurrentDirectory()}]");
                Console.ResetColor();
                Console.Write(">");


                var input = Console.ReadLine() ?? string.Empty;

                Console.WriteLine($"User input: {input}");

                if (input.ToLower() == "help")
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    using Stream? stream = assembly.GetManifestResourceStream("rename.Specification.md");
                    using StreamReader reader = new StreamReader(stream!);
                    string result = reader.ReadToEnd();
                    Console.WriteLine(result);
                }
                else if (input.ToLower().StartsWith("cd"))
                {
                    Cd(input);
                }
                else if (input.ToLower() == "pending")
                {
                    Pending();
                }
                else if (input.ToLower() == "exit")
                {
                    Environment.Exit(0);
                }
                else if (input.ToLower() == "clear")
                {
                    _selected.Clear();
                }
                else if (input.ToLower() == "ls")
                {
                    Ls();
                }
                else if (input.ToLower().StartsWith("select_where"))
                {
                    SelectWhere(input);
                }
                else
                {
                    Console.WriteLine("Syntax error. ");
                }
            }
        }

        private static void Ls()
        {
            Console.WriteLine("{Path} - {DateCreated} - {DateModified} - {FileSize}");

            var @enum = Directory.EnumerateFileSystemEntries(Directory.GetCurrentDirectory());
            @enum = @enum.OrderByDescending(Directory.GetCreationTime);
            Console.WriteLine();
            Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
            Console.WriteLine();
            foreach (var str in @enum)
            {
                Console.WriteLine(
                    $"{str.Replace(Directory.GetCurrentDirectory() + "\\", "")} - {Directory.GetCreationTime(str)} - {Directory.GetLastWriteTime(str)} - {(File.GetAttributes(str).HasFlag(FileAttributes.Directory) ? "Is a Directory" : new FileInfo(str).Length)}");
            }

            Console.WriteLine();
        }

        static void Pending()
        {
            Console.WriteLine("{Path} - {DateCreated} - {DateModified} - {FileSize}");
            _selected = _selected.OrderByDescending(i => i.DateCreated).ToList();
            foreach (Selected selected in _selected)
            {
                Console.WriteLine(selected.ToString());
            }
        }
    }
}