using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalizationHelper.IElements {
	public class InnerClass : IElement {
		public InnerClass(string name) {
			firstLine = "\t\tpublic class " + name + " {";
			Name = name;
			lastLine = "\t\t}";
		}

		public InnerClass(ref int i, string[] lines) {
			firstLine = lines[i];
			Name = firstLine.Trim().Replace("public class ", "").Replace(" {", "");
			i++;

			while (i < lines.Length && lines[i].Trim() != "}") {
				if (lines[i].TrimStart().StartsWith("public const int ")) {
					Internals.Add(new IDLineDef(lines[i]));
				}
				else if (lines[i].TrimStart().StartsWith("public class ")) {
					Internals.Add(new InnerClass(ref i, lines));
				}
				else {
					Internals.Add(new StdLine(lines[i]));
				}
				i++;
			}
			lastLine = lines[i];
		}

		private readonly string firstLine;
		private readonly string lastLine;

		public string Name { get; }

		public List<IElement> Internals { get; } = new();

		public string GetStr() {
			return firstLine + Environment.NewLine +
				   string.Join(Environment.NewLine, Internals.Select(s => s.GetStr())) + Environment.NewLine +
				   lastLine;
		}
	}
}