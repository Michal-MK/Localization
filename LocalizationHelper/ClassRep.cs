using LocalizationHelper.IElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LocalizationHelper {
	public class ClassRep : IElement {
		public List<string> FirstLines { get; set; } = new();

		public int FirstLineIndex { get; set; }

		public int ClassLength { get; set; }

		public List<IElement> Internals { get; set; } = new List<IElement>();

		public string GetStr() {
			return string.Join(Environment.NewLine, FirstLines) + Environment.NewLine + string.Join(Environment.NewLine, Internals.Select(s => s.GetStr())) + Environment.NewLine;
		}

		public static ClassRep Parse(string path) {
			ClassRep ret = new();

			string[] lines = File.ReadAllLines(path);
			bool gotClass = false;

			for (int i = 0; i < lines.Length; i++) {

				if (lines[i].TrimStart().StartsWith("public const int ")) {
					ret.Internals.Add(new IDDef(lines[i]));
				}
				else if (lines[i].TrimStart().StartsWith("public class ")) {
					gotClass = true;
					ret.Internals.Add(new InnerClass(ref i, lines));
				}
				else if (lines[i].Trim() == "}") {
					ret.Internals.Add(new StdLine(lines[i]));
				}
				if (!gotClass) {
					ret.FirstLines.Add(lines[i]);
				}
			}

			return ret;
		}
	}
}
