using LocalizationHelper.IElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LocalizationHelper {
	class Program {

		const string PROP_F = "public const int {0} = {1};";
		const string CFG = "private/locales.txt";

		private static Random r;

		private static Localizable activeL = null;
		private static InnerClass activeIC = null;

		private static bool autosave;

		static void Main(string[] args) {
			r = new Random(Environment.TickCount);

			List<Localizable> ls = GetLocalizables(CFG);

			while (true) {
				Console.Write("> ");
				string line = Console.ReadLine();

				if (line == "!o") {
					Save(ls);
					Console.WriteLine("Saved!");
				}

				if (line == "!q") {
					Save(ls);
					return;
				}

				if (line == "!q!") {
					return;
				}

				if (line == "!autosave") {
					autosave = true; 
					return;
				}

				if (line.StartsWith("!so ")) {
					string trim = line.Remove(0, 4);
					activeL = ls.Where(w => w.Shortcut == trim).Single();
					Console.WriteLine("Selected Active localizable: " + activeL.Name);
				}

				if (line == "!lsc") {
					Console.WriteLine(string.Join(Environment.NewLine, ls.Select(s => $"{s.Name} -- {s.Shortcut}")));
				}

				if (activeL != null) {
					if (line.StartsWith("!a ")) {
						string trim = line.Remove(0, 3);
						activeL.AddSubClass(trim);
						Console.WriteLine("Added subclass: " + trim + " to " + activeL.Name);
					}

					if (line == "!ls") {
						Console.WriteLine(string.Join(Environment.NewLine, (activeL.ClsRep.Internals[0] as InnerClass).Internals.Where(w => w.GetType() == typeof(InnerClass)).Select(s => ((InnerClass)s).Name)));
					}

					if (line.StartsWith("!si ")) {
						string trim = line.Remove(0, 4);
						activeIC = (activeL.ClsRep.Internals[0] as InnerClass).Internals
							.Where(w => w.GetType() == typeof(InnerClass))
							.Select(s => (InnerClass)s).Where(w => w.Name.ToLower() == trim.ToLower())
							.Single();
						Console.WriteLine("Selected Active Inner class: " + trim + " of " + activeL.Name);
					}

					if (activeIC != null) {
						if (line.StartsWith("! ")) {
							string trim = line.Remove(0, 2);
							string[] split = trim.Split('|');
							string name = split[0];
							string l1 = split[1];
							string l2 = split[2];
							string[] arr = new[] { l1, l2 };
							int ID = r.Next(1000, int.MaxValue);
							activeIC.Internals.Add(new IDDef(name, ID));
							int index = 0;
							foreach (var item in activeL.LangFiles) {
								item.Value.Sections.Where(w => w.GetType() == typeof(LangSection))
									.Select(s => (LangSection)s)
									.Where(w => w.Comment.Remove(0, 2) == activeIC.Name)
									.Single().Definitions.Add(new StdLine(ID, arr[index]));
								index++;
							}

							Console.WriteLine("Added localization!");

							if (autosave) {
								Save(ls);
								Console.WriteLine("Autosaved!");
							}
						}
					}
				}
			}
		}

		private static List<Localizable> GetLocalizables(string cfgPath) {
			List<Localizable> ret = new List<Localizable>();

			string[] lines = File.ReadAllLines(cfgPath);

			List<string> region = new List<string>();

			foreach (string line in lines) {
				if (string.IsNullOrEmpty(line)) {
					continue;
				}

				region.Add(line);
				if (line.StartsWith(";")) {
					ret.Add(Localizable.Parse(region));
					region.Clear();
				}
			}

			return ret;
		}

		private static void Save(List<Localizable> locs) {
			foreach (Localizable item in locs) {
				item.Save();
			}
		}
	}
}
