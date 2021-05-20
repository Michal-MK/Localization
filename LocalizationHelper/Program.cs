﻿using LocalizationHelper.IElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TextCopy;

namespace LocalizationHelper {
	public static class Program {
		private const string CONFIG_PATH = "private/locales.txt";

		private static Random r;

		private static Localizable activeLocalizable;
		private static InnerClass activeInnerClass;

		public static void Main(string[] args) {
			r = new Random(Environment.TickCount);

			Console.OutputEncoding = Encoding.Unicode;
			Console.InputEncoding = Encoding.Unicode;

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
					case "help":
						PrintHelp();
						break;
				}

				if (line.StartsWith("sell ")) {
					string trimmed = line.Remove(0, 5);
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

				if (activeLocalizable is null) continue;

				if (line == "lsc") {
					List<IElement> interns = ((InnerClass)activeLocalizable.ClassFile.Internals[0]).Internals;
					Console.WriteLine(string.Join(Environment.NewLine, interns
																	   .Where(w => w.GetType() == typeof(InnerClass))
																	   .Select(s => ((InnerClass)s).Name)));
				}

				if (line.StartsWith("ac ")) {
					string trimmed = line.Remove(0, 5);
					activeLocalizable.AddSubClass(trimmed);
					Console.WriteLine("Added subclass: " + trimmed + " to " + activeLocalizable.Name);
				}

				if (line.StartsWith("sc ")) {
					string trimmed = line.Remove(0, 5);
					try {
						activeInnerClass = ((InnerClass)activeLocalizable.ClassFile.Internals[0])
										   .Internals
										   .Where(w => w.GetType() == typeof(InnerClass)).Select(s => (InnerClass)s)
										   .Single(w => w.Name.ToLower() == trimmed.ToLower());

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
						Console.WriteLine("Could not select " + trimmed);
						break;
					}
					Console.WriteLine("Selected Active Inner class: " + trimmed + " of " + activeLocalizable.Name);
				}

				if (line.StartsWith("f ")) {
					string query = line.Remove(0, 2);
					FindAll(query);
				}

				if (activeInnerClass is null) continue;

				if (line.Contains("|")) {
					string[] split = line.Split('|');
					string name = ToConstName(split[0]);
					int id = r.Next(1000, int.MaxValue);

					Span<string> languages = split.AsSpan(1);
					activeInnerClass.Internals.Add(new IDLineDef(name, id));
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
		}

		private static void PrintHelp() {
			Console.WriteLine("listl - List all localizable");
			Console.WriteLine("listc - List all classes in a selected localizable");
			Console.WriteLine("sell - Select a localizable");
			Console.WriteLine("selc - Selects a classes to work with");
			Console.WriteLine("addc - Adds a classes into a selected localizable");
			Console.WriteLine(".+|(.*)+ - Defines new localizable");
			Console.WriteLine("save - Saves a localizable");
			Console.WriteLine("exit - Quits the program");
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

		private static void FindAll(string query) {
			IEnumerable<(IDLineDef, IDLineDef)> lines = activeLocalizable.FindContaining(query);
			Console.WriteLine("Found:");
			foreach ((IDLineDef cls, IDLineDef lang) in lines) {
				Console.WriteLine("\t" + cls.GetStr() + " == " + lang.GetStr());
			}
		}
	}
}