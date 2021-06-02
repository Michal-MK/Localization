using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ConsoleTables;
using LocalizationHelper.Core.IElements;
using TextCopy;

namespace LocalizationHelper.Core {
	public class Main {
		private static Localizable activeLocalizable;
		private static InnerClass activeInnerClass;

		private readonly Random r = new(Environment.TickCount);
		private readonly List<Localizable> ls;

		public Main(List<Localizable> localizables) {
			ls = localizables;
		}

		public string Handle(string line) {
			if (line is null) return null;

			switch (line) {
				case "save":
					Save(ls);
					return "Saved!";
				case "exit":
					Save(ls);
					return null;
				case "!exit":
					return null;
				case "help":
					return PrintHelp();
			}

			if (line.StartsWith("sl ")) {
				string trimmed = line.Remove(0, 3);
				try {
					activeLocalizable = ls.Single(w => w.Shortcut == trimmed);
					return "Selected Active localizable: " + activeLocalizable.Name;
				}
				catch (InvalidOperationException) {
					return "Could not select " + trimmed;
				}
			}

			if (line == "lsl") {
				return string.Join(Environment.NewLine, ls.Select(s => $"{s.Name} -- {s.Shortcut}"));
			}

			if (line.StartsWith("f ")) {
				string query = line.Remove(0, 2);
				IEnumerable<(IDLineDef, IDLineDef)> found = FindAll(ls, query);
				Console.WriteLine("Found:");
				ConsoleTable ct = new("Class File:", "Lang File:");

				found.Select(s => new object[] {
					s.Item2.FileName + " -> " + s.Item2.Parent + " -> " + s.Item2.GetStr().TrimStart(),
					s.Item1.GetStr()
				}).ForEach(f => ct.AddRow(f));

				ct.Write();
			}

			if (activeLocalizable is null) return null;

			if (line == "lsc") {
				List<IElement> interns = ((InnerClass)activeLocalizable.ClassFile.Internals[0]).Internals;
				return string.Join(Environment.NewLine, interns
																   .Where(w => w.GetType() == typeof(InnerClass))
																   .Select(s => ((InnerClass)s).Name));
			}

			if (line.StartsWith("ac ")) {
				string trimmed = line.Remove(0, 3);
				activeLocalizable.AddSubClass(trimmed);
				return "Added subclass: " + trimmed + " to " + activeLocalizable.Name;
			}

			if (line.StartsWith("sc ")) {
				string trimmed = line.Remove(0, 3);
				try {
					activeInnerClass = ((InnerClass)activeLocalizable.ClassFile.Internals[0])
						.Internals.Where(w => w.GetType() == typeof(InnerClass))
						.Cast<InnerClass>()
						.First(w => w.Name.ToLower().Contains(trimmed.ToLower()));

					foreach ((string _, LangFile value) in activeLocalizable.LangFiles) {
						List<LangSection> sections = value.Sections
														  .Where(w => w.GetType() == typeof(LangSection))
														  .Select(s => (LangSection)s).ToList();
						try {
							LangSection _ = sections.Single(w => w.Comment.Remove(0, 2) == activeInnerClass.Name);
						}
						catch (Exception) {
							return "Could not find LangSection for subclass: " + trimmed + Environment.NewLine +
								   "Found only: " + string.Join(", ", sections.Select(s => s.Comment));
						}
					}
				}
				catch (InvalidOperationException) {
					return "Could not select " + trimmed + ", either does not exist or the name is not unique!";
				}
				return "Selected Active Inner class: " + activeInnerClass.Name + " of " + activeLocalizable.Name;
			}

			if (activeInnerClass is null) return null;

			if (line == "lso") {
				return string.Join(", ", activeLocalizable.LangFiles.Keys.Select(Path.GetFileNameWithoutExtension));
			}

			if (line == "lsd") {
				return string.Join(Environment.NewLine, activeInnerClass.Internals
					   .Where(w => w.GetType() == typeof(IDLineDef)).Cast<IDLineDef>()
					   .Select(s => s.GetStr().TrimStart()));
			}

			if (line.Contains("|")) {
				string[] split = line.Split('|');
				string name = ToConstName(split[0]);
				int id = r.Next(1000, int.MaxValue);

				Span<string> languages = split.AsSpan(1);
				activeInnerClass.Internals.Add(new IDLineDef(activeLocalizable.ClassFile.FilePath, activeInnerClass.Name, name, id));
				int index = 0;
				foreach (LangFile val in activeLocalizable.LangFiles.Values) {
					val.Sections
					   .Where(w => w.GetType() == typeof(LangSection))
					   .Select(s => (LangSection)s)
					   .Single(w => w.Comment.Remove(0, 2) == activeInnerClass.Name)
					   .Definitions.Add(new StdLine(id, languages[index]));
					index++;
				}

				string classPath = ReconstructClassPath();
				if (ContainsSmartFormatTags(line)) {
					new Clipboard().SetText($"LocaleProvider.Instance.SmartFormat({classPath}.{name}, );");
				}
				else {
					new Clipboard().SetText($"LocaleProvider.Instance.Get({classPath}.{name});");
				}
				Save(ls);

				return "Added localization!";
			}
			return null;
		}

		private string PrintHelp() {
			StringBuilder sb = new();
			sb.AppendLine("lsl - Lists all localizable");
			sb.AppendLine("lsc - Lists all classes in a selected localizable");
			sb.AppendLine("sl - Selects a localizable");
			sb.AppendLine("sc - Selects a class to work with");
			sb.AppendLine("ac - Adds a class into a selected localizable");
			sb.AppendLine(".+|(.*)+ - Defines new localizable");
			sb.AppendLine("save - Saves a localizable");
			sb.AppendLine("help - Prints this message");
			sb.AppendLine("cls - Clears the console");
			sb.AppendLine("exit - Quits the program saving all changes");
			sb.AppendLine("exit - Quits the program WITHOUT saving");
			return sb.ToString();
		}

		private void Save(IEnumerable<Localizable> localizables) {
			foreach (Localizable item in localizables) {
				item.Save();
			}
		}

		private string ToConstName(string s) {
			StringBuilder sb = new();
			sb.Append(char.ToUpper(s[0]));

			for (int i = 1; i < s.Length; i++) {
				char c = s[i];
				if (char.IsUpper(c)) {
					sb.Append("_" + c);
					continue;
				}
				sb.Append(char.ToUpper(c));
			}
			return sb.ToString();
		}

		private string ReconstructClassPath() {
			string completeClassName = ((InnerClass)activeLocalizable.ClassFile.Internals
																	 .First(f => f.GetType() == typeof(InnerClass))).Name;
			int colonIndex = completeClassName.IndexOf(':');
			if (colonIndex != -1) {
				completeClassName = completeClassName.Remove(colonIndex - 1);
			}
			return completeClassName + "." + activeInnerClass.Name;
		}

		private bool ContainsSmartFormatTags(string line) {
			return line.Any(c => c == '\n') || line.Contains("{0}");
		}

		private IEnumerable<(IDLineDef, IDLineDef)> FindAll(List<Localizable> ls, string query) {
			IEnumerable<(IDLineDef, IDLineDef)> lines = new List<(IDLineDef, IDLineDef)>();
			lines = ls.Aggregate(lines, (current, l) => l.FindContaining(query).Concat(current));
			return lines;
		}
	}
}
