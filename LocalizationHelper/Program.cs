using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LocalizationHelper {
	public static class Program {
		private const string CONFIG_PATH = "private/locales.txt";

		public static void Main(string[] args) {
			Console.OutputEncoding = Encoding.Unicode;
			Console.InputEncoding = Encoding.Unicode;

			List<Localizable> ls = GetLocalizables(CONFIG_PATH);

			Main m = new(ls);

			while (true) {
				Console.Write("> ");
				string line = Console.ReadLine();
				m.Handle(line);

				if (line == "exit") {
					return;
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
	}
}
