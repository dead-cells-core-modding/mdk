using CommandLine;
using GameRes.Core.Pak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCCMTool.Commands.Pak
{
    internal class MergePakCommand : CommandBase<MergePakCommand.Options>
    {
        public override void Execute()
        {
            PakFile pak = new();

            if (!string.IsNullOrEmpty(Arguments.Stamp))
            {
                pak.Stamp = Encoding.ASCII.GetBytes(Arguments.Stamp).AsMemory()[..64];
            }

            foreach (var v in Arguments.Inputs)
            {
                PakFile.DirectoryEntry pakDir = pak.Root;
                string input = v;
                var splitIdx = v.IndexOf('=');
                if (splitIdx != -1)
                {
                    input = v[..splitIdx];
                    pakDir = pak.GetOrCreateDirectory(v[(splitIdx + 1)..]);
                }

                using var rs = File.OpenRead(input);
                var p = PakFile.ReadFrom(rs, false);

                void MergeEntry(PakFile.DirectoryEntry src, PakFile.DirectoryEntry dst)
                {
                    foreach (var v in src.Entries)
                    {
                        if (v is PakFile.DirectoryEntry dir)
                        {
                            var ddir = dst.GetOrCreateDirectory(dir.Name);
                            MergeEntry(dir, ddir);
                        }
                        else if(v is PakFile.FileEntry file)
                        {
                            var fe = (PakFile.FileEntry?) dst.Entries.FirstOrDefault(x => x.Name == file.Name);
                            if(fe == null)
                            {
                                fe = new()
                                {
                                    Name = file.Name,
                                };
                                dst.Entries.Add(fe);
                            }
                            fe.Checksum = null;
                            fe.Data = file.Data;
                        }
                    }
                }

                MergeEntry(p.Root, pakDir);
            }

            using var stream = File.OpenWrite(Arguments.Output);
            pak.Write(new(stream));
        }

        [Verb("merge-paks", HelpText = "Merge multiple PAK files into a single pak file.")]
        public class Options
        {
            [Option('i', "input", Required = true, HelpText = "The path to the input pak files.")]
            public required IEnumerable<string> Inputs { get; set; }
            [Option('o', "output", Required = true, HelpText = "The path to the output pak file.")]
            public required string Output { get; set; }
            [Option('s', "stamp", HelpText = "See https://n3rdl0rd.github.io/ModDocCE/files/pak/#stamps")]
            public string? Stamp { get; set; }
        }
    }
}
