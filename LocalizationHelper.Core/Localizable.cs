using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LocalizationHelper.Core.IElements;
using LocalizationHelper.Core.IElements.Code;
using LocalizationHelper.Core.IElements.Text;

namespace LocalizationHelper.Core {
	public class Localizable {
		public static Localizable Parse(List<string> region) {
			Localizable ret = new();

			string[] split = region[^1].Split(';', StringSplitOptions.RemoveEmptyEntries);
			ret.Name = split[0];
			ret.Shortcut = split[1];
			ret.ClassFile = ClassFile.Parse(region[^2]);

			for (int i = 0; i < region.Count - 2; i++) {
				ret.LangFiles.Add(region[i], LangFile.Parse(region[i]));
			}
			return ret;
		}

		public string Name { get; private set; }
		public string Shortcut { get; private set; }
		public ClassFile ClassFile { get; private set; }
		public Dictionary<string, LangFile> LangFiles { get; } = new();

		public void AddSubClass(string trim) {
			InnerClass parent = (InnerClass)ClassFile.Internals.First(f => f.GetType() == typeof(InnerClass));
			InnerClass ic = new(ClassFile, trim);

			parent.Internals.Add(new StdLine(""));
			parent.Internals.Add(ic);

			foreach (LangFile item in LangFiles.Values) {
				item.Sections.Add(new LangSection(item, "# " + trim));
			}
		}
		
		public IEnumerable<(IDLineLocalization, IDLineDefinition)> FindContaining(string query) {
			List<IDLineLocalization> localizationsMatchingQuery = LangFiles.Values.SelectMany(s => s.FindAll(query)).ToList();

			List<(IDLineLocalization, IDLineDefinition)> ret = new();
			IEnumerable<InnerClass> inner = ClassFile.Internals.OfType<InnerClass>().ToList();

			foreach (IDLineLocalization id in localizationsMatchingQuery) {
				IDLineDefinition clsId = inner.Select(s => s.FindAllDefinitions().SingleOrDefault(w => w.ID == id.ID)).Single();
				if (clsId == default) {
					/*TODO inform the user*/
					continue;
				}
				ret.Add((id, clsId));
			}
			return ret;
		}

		public void Save() {
			File.WriteAllText(ClassFile.FilePath, ClassFile.GetStr(), Encoding.UTF8);

			foreach ((string key, LangFile value) in LangFiles) {
				File.WriteAllText(key, value.GetStr(), Encoding.UTF8);
			}
		}
	}
}
