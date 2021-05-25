using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LocalizationHelper.Core.IElements;

namespace LocalizationHelper.Core {
	public class ClassFile : IElement {
		
		private ClassFile(string filePath) {
			FilePath = filePath;
		}
		
		public static ClassFile Parse(string path) {
			ClassFile ret = new(path);

			string[] lines = File.ReadAllLines(path);
			bool gotClass = false;

			for (int i = 0; i < lines.Length; i++) {
				if (lines[i].TrimStart().StartsWith("public const int ")) {
					ret.Internals.Add(new IDLineDef(Path.GetFileNameWithoutExtension(path), Path.GetFileNameWithoutExtension(path), lines[i]));
				}
				else if (lines[i].TrimStart().StartsWith("public class ")) {
					gotClass = true;
					ret.Internals.Add(new InnerClass(Path.GetFileNameWithoutExtension(path), ref i, lines));
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
		
		public List<IElement> Internals { get; } = new();
		public string FilePath { get; }

		private readonly List<string> firstLines = new();

		public string GetStr() {
			return string.Join(Environment.NewLine, firstLines) + Environment.NewLine +
				   string.Join(Environment.NewLine, Internals.Select(s => s.GetStr())) + Environment.NewLine;
		}
	}
}
