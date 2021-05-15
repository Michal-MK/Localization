using LocalizationHelper.IElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LocalizationHelper {
	public static class Program {
		private const string CONFIG_PATH = "private/locales.txt";

		private static Random r;

		private static Localizable activeLocalizable;
		private static InnerClass activeInnerClass;

		public static void Main(string[] args) {
			r = new Random(Environment.TickCount);

			List<Localizable> ls = GetLocalizables(CONFIG_PATH);

			while (true) {
				Console.Write("> ");
				string line = Console.ReadLine();
				if (line is null) return;

				switch (line) {
					case "save":
						Save(ls);
						Console.WriteLine("Saved!");
						break;
					case "exit":
						Save(ls);
						return;
					case "!exit":
						return;
				}

				if (line.StartsWith("sel ")) {
					string trimmed = line.Remove(0, 4);
					activeLocalizable = ls.Single(w => w.Shortcut == trimmed);
					Console.WriteLine("Selected Active localizable: " + activeLocalizable.Name);
				}

				if (line == "list locales") {
					Console.WriteLine(string.Join(Environment.NewLine, ls.Select(s => $"{s.Name} -- {s.Shortcut}")));
				}

				if (activeLocalizable is null) continue;

				if (line == "list class") {
					Console.WriteLine(string.Join(Environment.NewLine, ((InnerClass) activeLocalizable.ClassFile.Internals[0]).Internals.Where(w => w.GetType() == typeof(InnerClass)).Select(s => ((InnerClass) s).Name)));
				}

				if (line.StartsWith("add class ")) {
					string trimmed = line.Remove(0, 3);
					activeLocalizable.AddSubClass(trimmed);
					Console.WriteLine("Added subclass: " + trimmed + " to " + activeLocalizable.Name);
				}
				if (line.StartsWith("sel class ")) {
					string trimmed = line.Remove(0, 4);
					activeInnerClass = ((InnerClass) activeLocalizable.ClassFile.Internals[0])
									   .Internals
									   .Where(w => w.GetType() == typeof(InnerClass)).Select(s => (InnerClass) s)
									   .Single(w => w.Name.ToLower() == trimmed.ToLower());
					Console.WriteLine("Selected Active Inner class: " + trimmed + " of " + activeLocalizable.Name);
				}

				if (activeInnerClass is null) continue;

				if (line.Contains("|")) {
					string trimmed = line.Remove(0, 2);
					string[] split = trimmed.Split('|');
					string name = split[0];
					int id = r.Next(1000, int.MaxValue);

					Span<string> languages = split.AsSpan(1);
					activeInnerClass.Internals.Add(new IDLineDef(name, id));
					int index = 0;
					foreach (LangFile val in activeLocalizable.LangFiles.Values) {
						val.Sections
						   .Where(w => w.GetType() == typeof(LangSection))
						   .Select(s => (LangSection) s)
						   .Single(w => w.Comment.Remove(0, 2) == activeInnerClass.Name)
						   .Definitions.Add(new StdLine(id, languages[index]));
						index++;
					}

					Console.WriteLine("Added localization!");
					Save(ls);
				}
			}
		}

		private static List<Localizable> GetLocalizables(string cfgPath) {
			List<Localizable> ret = new();
			List<string> region = new();

			string[] lines = File.ReadAllLines(cfgPath);

			foreach (string line in lines) {
				if (string.IsNullOrEmpty(line)) continue;

				region.Add(line);

				if (!line.StartsWith(";")) continue;

				ret.Add(Localizable.Parse(region));
				region.Clear();
			}

			return ret;
		}

		private static void Save(IEnumerable<Localizable> localizables) {
			foreach (Localizable item in localizables) {
				item.Save();
			}
		}
	}
}