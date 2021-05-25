using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ConsoleTables;
using LocalizationHelper.IElements;
using TextCopy;

namespace LocalizationHelper {
	public class Main {
		private static Localizable activeLocalizable;
		private static InnerClass activeInnerClass;

		private readonly Random r = new(Environment.TickCount);
		private readonly List<Localizable> ls;

		public Main(List<Localizable> localizables) {
			ls = localizables;
		}

		public void Handle(string line) {
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
				case "help":
					PrintHelp();
					break;
				case "cls":
					Console.Clear();
					break;
			}

			if (line.StartsWith("sl ")) {
				string trimmed = line.Remove(0, 3);
				try {
					activeLocalizable = ls.Single(w => w.Shortcut == trimmed);
					Console.WriteLine("Selected Active localizable: " + activeLocalizable.Name);
				}
				catch (InvalidOperationException) {
					Console.WriteLine("Could not select " + trimmed);
				}
			}

			if (line == "lsl") {
				Console.WriteLine(string.Join(Environment.NewLine, ls.Select(s => $"{s.Name} -- {s.Shortcut}")));
			}

			if (line.StartsWith("f ")) {
				string query = line.Remove(0, 2);
				FindAll(ls, query);
			}

			if (activeLocalizable is null) return;

			if (line == "lsc") {
				List<IElement> interns = ((InnerClass)activeLocalizable.ClassFile.Internals[0]).Internals;
				Console.WriteLine(string.Join(Environment.NewLine, interns
																   .Where(w => w.GetType() == typeof(InnerClass))
																   .Select(s => ((InnerClass)s).Name)));
			}

			if (line.StartsWith("ac ")) {
				string trimmed = line.Remove(0, 3);
				activeLocalizable.AddSubClass(trimmed);
				Console.WriteLine("Added subclass: " + trimmed + " to " + activeLocalizable.Name);
			}

			if (line.StartsWith("sc ")) {
				string trimmed = line.Remove(0, 3);
				try {
					activeInnerClass = ((InnerClass)activeLocalizable.ClassFile.Internals[0])
									   .Internals
									   .Where(w => w.GetType() == typeof(InnerClass)).Select(s => (InnerClass)s)
									   .Single(w => w.Name.ToLower().Contains(trimmed.ToLower()));

					foreach ((string _, LangFile value) in activeLocalizable.LangFiles) {
						List<LangSection> sections = value.Sections
														  .Where(w => w.GetType() == typeof(LangSection))
														  .Select(s => (LangSection)s).ToList();
						try {
							LangSection _ = sections.Single(w => w.Comment.Remove(0, 2) == activeInnerClass.Name);
						}
						catch (Exception) {
							Console.WriteLine("Could not find LangSection for subclass: " + trimmed);
							Console.WriteLine("Found only: " + string.Join(", ", sections.Select(s => s.Comment)));
						}
					}
				}
				catch (InvalidOperationException) {
					Console.WriteLine("Could not select " + trimmed + ", either does not exist or the name is not unique!");
					return;
				}
				Console.WriteLine("Selected Active Inner class: " + activeInnerClass.Name + " of " + activeLocalizable.Name);
			}

			if (activeInnerClass is null) return;

			if (line == "lso") {
				Console.WriteLine(string.Join(", ", activeLocalizable.LangFiles.Keys.Select(Path.GetFileNameWithoutExtension)));
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

				Console.WriteLine("Added localization!");
				string classPath = ReconstructClassPath();
				if (ContainsSmartFormatTags(line)) {
					new Clipboard().SetText($"LocaleProvider.Instance.SmartFormat({classPath}.{name}, );");
				}
				else {
					new Clipboard().SetText($"LocaleProvider.Instance.Get({classPath}.{name});");
				}
				Save(ls);
			}
		}

		private static void PrintHelp() {
			Console.WriteLine("lsl - Lists all localizable");
			Console.WriteLine("lsc - Lists all classes in a selected localizable");
			Console.WriteLine("sl - Selects a localizable");
			Console.WriteLine("sc - Selects a class to work with");
			Console.WriteLine("ac - Adds a class into a selected localizable");
			Console.WriteLine(".+|(.*)+ - Defines new localizable");
			Console.WriteLine("save - Saves a localizable");
			Console.WriteLine("help - Prints this message");
			Console.WriteLine("cls - Clears the console");
			Console.WriteLine("exit - Quits the program saving all changes");
			Console.WriteLine("exit - Quits the program WITHOUT saving");
		}

		private static void Save(IEnumerable<Localizable> localizables) {
			foreach (Localizable item in localizables) {
				item.Save();
			}
		}

		private static string ToConstName(string s) {
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

		private static string ReconstructClassPath() {
			string completeClassName = ((InnerClass)activeLocalizable.ClassFile.Internals
																	 .First(f => f.GetType() == typeof(InnerClass))).Name;
			int colonIndex = completeClassName.IndexOf(':');
			if (colonIndex != -1) {
				completeClassName = completeClassName.Remove(colonIndex - 1);
			}
			return completeClassName + "." + activeInnerClass.Name;
		}

		private static bool ContainsSmartFormatTags(string line) {
			return line.Any(c => c == '\n') || line.Contains("{0}");
		}

		private static void FindAll(List<Localizable> ls, string query) {
			IEnumerable<(IDLineDef, IDLineDef)> lines = new List<(IDLineDef, IDLineDef)>();
			lines = ls.Aggregate(lines, (current, l) => l.FindContaining(query).Concat(current));

			Console.WriteLine("Found:");
			ConsoleTable ct = new("Class File:", "Lang File:");

			lines.Select(s => new object[] {
				s.Item2.FileName + " -> " + s.Item2.Parent + " -> " + s.Item2.GetStr().TrimStart(),
				s.Item1.GetStr()
			}).ForEach(f => ct.AddRow(f));

			ct.Write();
		}
	}
}
