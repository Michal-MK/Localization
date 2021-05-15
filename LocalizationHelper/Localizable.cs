using LocalizationHelper.IElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LocalizationHelper {
	public class Localizable {
		private string csFile;

		public string Name { get; private set; }

		public string Shortcut { get; private set; }

		public ClassFile ClassFile { get; private set; }

		public Dictionary<string, LangFile> LangFiles { get; } = new();

		public static Localizable Parse(List<string> region) {
			Localizable ret = new();

			string[] split = region[^1].Split(';', StringSplitOptions.RemoveEmptyEntries);
			ret.Name = split[0];
			ret.Shortcut = split[1];
			ret.csFile = region[^2];
			ret.ClassFile = ClassFile.Parse(ret.csFile);

			for (int i = 0; i < region.Count - 2; i++) {
				ret.LangFiles.Add(region[i], LangFile.Parse(region[i]));
			}
			return ret;
		}

		public void AddSubClass(string trim) {
			InnerClass ic = new(trim);
			InnerClass parent = (InnerClass) ClassFile.Internals.First(f => f.GetType() == typeof(InnerClass));
			parent.Internals.Add(new StdLine(""));
			parent.Internals.Add(ic);

			foreach (LangFile item in LangFiles.Values) {
				item.Sections.Add(new LangSection("# " + trim));
			}
		}

		public void Save() {
			File.WriteAllText(csFile, ClassFile.GetStr(), Encoding.UTF8);

			foreach ((string key, LangFile value) in LangFiles) {
				File.WriteAllText(key, value.GetStr(), Encoding.UTF8);
			}
		}
	}
}