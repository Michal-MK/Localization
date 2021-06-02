using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LocalizationHelper.Core.IElements;

namespace LocalizationHelper.Core {
	public class Localizable {
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

		public string Name { get; private set; }
		public string Shortcut { get; private set; }
		public ClassFile ClassFile { get; private set; }
		public Dictionary<string, LangFile> LangFiles { get; } = new();

		private string csFile;

		public void AddSubClass(string trim) {
			InnerClass parent = (InnerClass)ClassFile.Internals.First(f => f.GetType() == typeof(InnerClass));
			InnerClass ic = new(parent.FileName, trim);

			parent.Internals.Add(new StdLine(""));
			parent.Internals.Add(ic);

			foreach (LangFile item in LangFiles.Values) {
				item.Sections.Add(new LangSection(ClassFile.FilePath, "# " + trim));
			}
		}

		public IEnumerable<(IDLineDef, IDLineDef)> FindContaining(string query) {
			List<IDLineDef> definitionsMatchingQuery = LangFiles.Values.SelectMany(s => s.FindAll(query)).ToList();

			List<(IDLineDef, IDLineDef)> ret = new();
			IEnumerable<InnerClass> inner = ClassFile.Internals.Where(w => w.GetType() == typeof(InnerClass)).Cast<InnerClass>().ToList();

			foreach (IDLineDef id in definitionsMatchingQuery) {
				IDLineDef clsId = inner.Select(s => s.FindAllDefinitions().SingleOrDefault(w => w.ID == id.ID)).Single();
				if (clsId == default) {
					/*TODO inform the user*/
					continue;
				}
				ret.Add((id, clsId));
			}
			return ret;
		}

		public void Save() {
			File.WriteAllText(csFile, ClassFile.GetStr(), Encoding.UTF8);

			foreach ((string key, LangFile value) in LangFiles) {
				File.WriteAllText(key, value.GetStr(), Encoding.UTF8);
			}
		}
	}
}
