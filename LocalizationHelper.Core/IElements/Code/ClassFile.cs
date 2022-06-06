using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LocalizationHelper.Core.IElements.Code {
	public class ClassFile : ICodeElement {

		private ClassFile(string filePath) {
			FilePath = filePath;
		}

		public int IndentLevel { get; private set; }

		public string Indent => new('\t', IndentLevel);

		public static ClassFile Parse(string path) {
			ClassFile ret = new(path);

			string[] lines = File.ReadAllLines(path);
			bool gotClass = false;

			ret.IndentLevel = lines.First(w => w.Contains("namespace")).Trim().EndsWith('{') ? 2 : 1;

			for (int i = 0; i < lines.Length; i++) {
				if (lines[i].TrimStart().StartsWith("public const int ")) {
					ret.Internals.Add(new IDLineDefinition(ret, lines[i], null));
				}
				else if (lines[i].TrimStart().StartsWith("public class ") || lines[i].TrimStart().StartsWith("public static class ")) {
					gotClass = true;
					ret.Internals.Add(new InnerClass(ret, ref i, lines, null));
				}
				else if (lines[i].Trim() == "}") {
					ret.Internals.Add(new StdLine(lines[i]));
				}
				if (!gotClass) {
					ret.firstLines.Add(lines[i]);
				}
			}
			return ret;
		}

		public List<ICodeElement> Internals { get; } = new();
		public string FilePath { get; }

		private readonly List<string> firstLines = new();

		public List<InnerClass> GetInnerClasses() {
			return Internals.OfType<InnerClass>().SelectMany(sm => sm.GetAllInnerClasses()).ToList();
		}

		public List<IDLineDefinition> GetAllDefs() {
			return Internals.SelectMany(s => s.GetAllDefs()).ToList();
		}

		public string GetStr() {
			return string.Join(Environment.NewLine, firstLines) + Environment.NewLine +
				   string.Join(Environment.NewLine, Internals.Select(s => s.GetStr())) + Environment.NewLine;
		}
	}
}