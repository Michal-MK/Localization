using LocalizationHelper.IElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LocalizationHelper {
	public class ClassFile : IElement {
		private readonly List<string> firstLines = new();

		public List<IElement> Internals { get; } = new();

		public string GetStr() {
			return string.Join(Environment.NewLine, firstLines) + Environment.NewLine +
				   string.Join(Environment.NewLine, Internals.Select(s => s.GetStr())) + Environment.NewLine;
		}

		public static ClassFile Parse(string path) {
			ClassFile ret = new();

			string[] lines = File.ReadAllLines(path);
			bool gotClass = false;

			for (int i = 0; i < lines.Length; i++) {
				if (lines[i].TrimStart().StartsWith("public const int ")) {
					ret.Internals.Add(new IDLineDef(lines[i]));
				}
				else if (lines[i].TrimStart().StartsWith("public class ")) {
					gotClass = true;
					ret.Internals.Add(new InnerClass(ref i, lines));
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
	}
}