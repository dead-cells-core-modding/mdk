using CommandLine;
using CommandLine.Text;
using GameRes.Core.Pak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCCMTool.Commands.Pak
{
    internal class UnpackPakCommand: CommandBase<UnpackPakCommand.Options>
    {
        public override void Execute()
        {
            var output = new DirectoryInfo(Arguments.OutputDir);
            output.Create();

            foreach (var v in Arguments.PakPath)
            {
                using var stream = File.OpenRead(v);
                var pak = PakFile.ReadFrom(stream, false);


                void ExtractToDirectory(PakFile.DirectoryEntry dir, DirectoryInfo output)
                {
                    foreach (var v in dir.Entries)
                    {
                        if (v is PakFile.DirectoryEntry d)
                        {
                            var doi = output.CreateSubdirectory(d.Name);
                            doi.Create();
                            ExtractToDirectory(d, doi);
                            continue;
                        }
                        else if (v is PakFile.FileEntry f)
                        {
                            File.WriteAllBytes(
                                Path.Combine(output.FullName, v.Name),
                                f.Data.Data.Span);
                        }
                    }
                }

                ExtractToDirectory(pak.Root, output);
            }
        }

        [Verb("unpack-pak", false, [
            "extract-pak"
            ], HelpText = "Extract the contents from the pak file")]
        public class Options
        {
            [Option('i', "input", HelpText = "The path to the input pak file.", Required = true)]
            public required IEnumerable<string> PakPath { get; set; }
            [Option('o', "output", HelpText = "The path to the output directory.", Required = true)]
            public required string OutputDir { get; set; }

            [Usage]
            public static Example[] Examples => [
                new("Unpack a pak file", new Options(){
                    PakPath = [ "res.pak" ],
                    OutputDir = "outputDir"
                }),
                new("Unpack pak files", new Options(){
                    PakPath = [ "res1.pak", "res2.pak", "res3.pak" ],
                    OutputDir = "outputDir"
                })
                ];
        }
    }
}
