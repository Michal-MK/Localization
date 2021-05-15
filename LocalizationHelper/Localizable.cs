using LocalizationHelper.IElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LocalizationHelper {
	public class Localizable {

		public string Name { get; set; }

		public string Shortcut { get; set; }

		public string CSFile { get; set; }

		public ClassRep ClsRep { get; set; }

		public Dictionary<string, LangFile> LangFiles { get; set; } = new();


		public static Localizable Parse(List<string> region) {
			Localizable ret = new();

			string[] split = region[^1].Split(';', StringSplitOptions.RemoveEmptyEntries);
			ret.Name = split[0];
			ret.Shortcut = split[1];
			ret.CSFile = region[^2];
			ret.ClsRep = ClassRep.Parse(ret.CSFile);



			for (int i = 0; i < region.Count - 2; i++) {
				ret.LangFiles.Add(region[i], LangFile.Parse(region[i]));
			}
			return ret;
		}

		public void AddSubClass(string trim) {
			InnerClass ic = new InnerClass(trim);
			(ClsRep.Internals.First(f => f.GetType() == typeof(InnerClass)) as InnerClass).Internals.Add(new StdLine(""));
			(ClsRep.Internals.First(f => f.GetType() == typeof(InnerClass)) as InnerClass).Internals.Add(ic);
			foreach (var item in LangFiles.Values) {
				item.Sections.Add(new LangSection("# " + trim));
			}
		}

		public void Save() {
			File.WriteAllText(CSFile, ClsRep.GetStr(), Encoding.UTF8);

			foreach (var kv in LangFiles) {
				File.WriteAllText(kv.Key, kv.Value.GetStr(), Encoding.UTF8);
			}
		}
	}
}