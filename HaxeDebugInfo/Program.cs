
using BytecodeMapping;
using CommandLine;
using System.Xml.Linq;

Parser.Default.ParseArguments<Options>(args)
    .WithParsed(options =>
    {
        var db = BytecodeMappingData.ReadFrom(File.ReadAllBytes(options.DatabasePath));
        int fid = -1;
        if(options.FunctionIndex != null)
        {
            fid = options.FunctionIndex.Value;
        }
        else
        {
            ArgumentNullException.ThrowIfNull(options.Path, nameof(options.Path));
            var name = Path.GetFileName(options.Path);
            foreach(var v in db.Functions)
            {
                foreach(var j in v.Value.Instructions)
                {
                    if(j.Line != options.Line)
                    {
                        continue;
                    }
                    if(Path.GetFileName(j.Path) == name)
                    {
                        fid = v.Key;
                        break;
                    }
                }
            }
            if(fid == -1)
            {
                throw new InvalidOperationException();
            }
        }

        var fun = db.Functions[fid];

        BytecodeMappingData.FunctionData.Item bestFit = new();
        foreach(var v in fun.Instructions)
        {
            if(options.Path != null)
            {
                if (Path.GetFileName(v.Path) != options.Path)
                {
                    continue;
                }
            }
            if(v.Line >= bestFit.Line &&
                v.Line <= options.Line)
            {
                bestFit = v;
                if(v.Line == options.Line)
                {
                    break;
                }
            }
        }

        Console.WriteLine($"{fun.Name}{{IL Index: {bestFit.ILIndex}}}");
    });

class Options
{
    [Option('i', "function-index",HelpText = "The function index.", Default = null)]
    public int? FunctionIndex { get; set; }
    [Option('p', "path", HelpText = "The path of source file.", Default = null)]
    public string? Path { get; set; }
    [Option('l', "line", HelpText = "The line of source.", Required = true)]
    public int Line { get; set; }
    [Option('d', "database", HelpText = "The path of database. (*.bcm.bin)", Required = true)]
    public string DatabasePath { get; set; } = "";
}