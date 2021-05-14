using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenSense.Widget.OpenSmileConfigurationConverter {
    public static class Parser {

        public static void Parse(string filename, string saveDir) {
            var text = File.ReadAllText(filename);
            var baseDir = Path.GetDirectoryName(filename);

            var replaceInstances = new HashSet<string>();
            var includeEval = new MatchEvaluator(m => {
                var i = new Include(m);
                var relativeBaseDir = Path.GetDirectoryName(i.Filename);
                Parse(Path.Combine(baseDir, i.Filename), Path.Combine(saveDir, relativeBaseDir));
                return m.Value;
            });
            var declarationEval = new MatchEvaluator(m => {
                var d = new InstanceDeclaration(m);
                if (Constants.DATA_SOURCE_DERIVATES.Contains(d.Type)) {
                    replaceInstances.Add(d.Name);
                    return d.Replace(Constants.RAW_WAVE_SOURCE_NAME);
                }
                if (Constants.DATA_SINK_DERIVATES.Contains(d.Type)) {
                    replaceInstances.Add(d.Name);
                    return d.Replace(Constants.RAW_DATA_SINK_NAME);
                }
                return m.Value;
            });
            var configurationEval = new MatchEvaluator(m => {
                var c = new InstanceConfiguration(m);
                if (replaceInstances.Contains(c.Name)) {
                    if (Constants.DATA_SOURCE_DERIVATES.Contains(c.Type)) {
                        return c.Replace(Constants.RAW_WAVE_SOURCE_NAME, Constants.RAW_DATA_SOURCE_OPTIONS, Constants.RAW_WAVE_SOURCE_PROPERTY_DEFAULT_TEXT);
                    } else if (Constants.DATA_SINK_DERIVATES.Contains(c.Type)) {
                        return c.Replace(Constants.RAW_DATA_SINK_NAME, Constants.RAW_DATA_SINK_OPTIONS, Constants.RAW_DATA_SINK_PROPERTY_DEFAULT_TEXT);
                    } else {
                        throw new InvalidOperationException();
                    }
                }
                return m.Value;
            });

            var sections = Regex.Split(text, @"(?=^\s*\[)", RegexOptions.Multiline).Where(l => !string.IsNullOrWhiteSpace(l));
            var sb = new StringBuilder();
            foreach (var sec in sections) {
                var modified = sec;
                modified = Include.REGEX.Replace(modified, includeEval);
                modified = InstanceDeclaration.REGEX.Replace(modified, declarationEval);
                modified = InstanceConfiguration.REGEX.Replace(modified, configurationEval);
                sb.Append(modified);
                sb.Append(Constants.NEW_LINE);
                sb.Append(Constants.NEW_LINE);
            }
            Directory.CreateDirectory(saveDir);
            var saveFilename = Path.Combine(saveDir, Path.GetFileName(filename));
            File.WriteAllText(saveFilename, sb.ToString());
        }
    }

    internal class Include {

        public static readonly Regex REGEX = new Regex(@"^\s*\\{([\w\./]+)}.*", RegexOptions.Multiline);

        public Include(Match match) {
            Debug.Assert(match != null && match.Success);
            Match = match;
        }

        public Match Match { get; private set; }

        private Group FilenameGroup => Match.Groups[1];

        public string Filename => FilenameGroup.Value;

        public string Replace(string newFilename) => Match.Value.Substring(0, FilenameGroup.Index - Match.Index) + newFilename + Match.Value.Substring(FilenameGroup.Index + FilenameGroup.Length - Match.Index);
    }

    internal class InstanceDeclaration {

        public static readonly Regex REGEX = new Regex(@"^\s*instance\[(\w+)\]\.type\s*=\s*(\w+).*", RegexOptions.Multiline);

        public InstanceDeclaration(Match match) {
            Debug.Assert(match != null && match.Success);
            Match = match;
        }

        public Match Match { get; private set; }

        public string Name => Match.Groups[1].Value;

        private Group TypeGroup => Match.Groups[2];

        public string Type => TypeGroup.Value;

        public string Replace(string newType) => Match.Value.Substring(0, TypeGroup.Index - Match.Index) + newType + Match.Value.Substring(TypeGroup.Index + TypeGroup.Length - Match.Index);
    }

    internal class InstanceConfiguration {

        public static readonly Regex REGEX = new Regex(@"^\s*\[(\w+):(\w+)\].*\r?\n((.*\r?\n?)*?)(?=(^\s*\[)|\z)", RegexOptions.Multiline);

        public InstanceConfiguration(Match match) {
            Debug.Assert(match != null && match.Success);
            Match = match;
        }

        public Match Match { get; private set; }

        public string Name => Match.Groups[1].Value;

        private Group TypeGroup => Match.Groups[2];

        public string Type => TypeGroup.Value;

        private Group ContentGroup => Match.Groups[3];

        public string Content => ContentGroup.Value;

        private static string DirectPassOptions(string text, string[] options) {
            var lines = Regex.Split(text, "\r\n|\n");
            var filtered = lines.Where(line => {
                var trimed = line.Trim();
                if (trimed == string.Empty || trimed.StartsWith(";") || trimed.StartsWith("//")) {
                    return true;
                }else {
                    var match = Regex.Match(trimed, @"^[\w_]+");
                    if (match.Success) {
                        var option = match.Groups[0].Value;
                        return options.Any(o => option == o);
                    } else {
                        return false; // something like: \{\cm[]}
                    }
                }
            });
            return string.Join(Constants.NEW_LINE, filtered).TrimEnd();
        }

        public string Replace(string newType, string[] supportedOptions, string append) => 
            Match.Value.Substring(0, TypeGroup.Index - Match.Index) 
            + newType 
            + Match.Value.Substring(TypeGroup.Index + TypeGroup.Length - Match.Index, ContentGroup.Index - TypeGroup.Index - TypeGroup.Length) 
            + DirectPassOptions(Content, supportedOptions) 
            + Match.Value.Substring(ContentGroup.Index + ContentGroup.Length - Match.Index)
            + Constants.NEW_LINE
            + append;
    }
}
