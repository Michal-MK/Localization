using System;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Localization {
	public class Compartment {

		//Example: Opravdu chcete smazat {r0:1|tento|2-5|tyto|5-|těchto}{0:1||2-| \0} záznam{r0:1||2-5|y|5-|ů}?

		/*To + Processing afterwards: 
		[
			Opravdu chcete smazat 
			r0:1|tento|2-|tyto
			 
			0:1||2-|\0
			 záznam
			r0:1||2-|y
			?
		]
		*/

		public Compartment(string v) {
			Str = v;
		}

		public string Str { get; }
		public bool IsArgument { get; set; }
		public bool Plain => !IsArgument && !IsBackReference(out _);

		private string[] Split => Str.Split(':', '|');

		private string ProcessedString { get; set; }

		private bool HasID => IsArgument && Split.Length > 0 && int.TryParse(Split[0], out _);

		private bool HasBackRefID => IsArgument && Split.Length > 0 && int.TryParse(Split[0].Substring(1), out _);

		private bool HasRules => IsArgument && (HasID || HasBackRefID) && Split.Length > 1 && Str.Contains(":");

		private int ID => int.Parse(Split[0]);

		private bool IsBackReference(out int refID) {
			refID = -1;
			if (Str.Length <= 1) return false;
			bool numericEnd = int.TryParse(Str[1].ToString(), out refID);

			return IsArgument && Str.Length > 1 && Str[0] == 'r' && numericEnd;
		}

		public override string ToString() {
			return Str;
		}

		public string Process(Compartment[] parts, object[] args) {
			if (ProcessedString != null) return ProcessedString;

			if (IsBackReference(out int refID)) {
				Compartment origin = FindOrigin(refID, parts);
				origin.Process(parts, args);

				object arg = args[refID];
				for (int i = 1; i < Split.Length; i += 2) {
					LocalizationRule rule = new LocalizationRule { Match = Split[i], Result = Split[i + 1] };
					if (rule.IsMatch(arg.ToString())) {
						return ProcessedString = PostProcess(rule.Result, args); ;
					}
				}
				throw new InvalidOperationException($"{Str} [{ arg }] < MATCH NOT EXHAUSTIVE");
			}
			if (IsArgument) {
				if (HasRules) {
					object arg = args[ID];
					for (int i = 1; i < Split.Length; i += 2) {
						LocalizationRule rule = new LocalizationRule { Match = Split[i], Result = Split[i + 1] };
						if (rule.IsMatch(arg.ToString())) {
							return ProcessedString = PostProcess(rule.Result, args);
						}
					}
					throw new InvalidOperationException($"{Str} [{ arg }] < MATCH NOT EXHAUSTIVE");
				}
				if (HasID) {
					return ProcessedString = args[ID].ToString();
				}
				switch (Str) {
					case "OB":
						return ProcessedString = "{";
					case "CB":
						return ProcessedString = "}";
				}
			}
			if (Plain) {
				return ProcessedString = PostProcess(Str, args);
			}

			throw new InvalidOperationException("UNKNOWN ERROR: " + Str);
		}

		private static Compartment FindOrigin(int refID, IEnumerable<Compartment> parts) {
			List<Compartment> cmp = parts.Where(w => w.HasID && w.ID == refID)
										 .ToList();
			if (cmp.Count != 1) {
				throw new InvalidOperationException("REF ID NOT FOUND!");
			}
			return cmp[0];
		}

		private static string PostProcess(string text, object[] args) {
			for (int i = 0; i < args.Length; i++) {
				text = text.Replace("\\" + i, args[i].ToString());
			}
			return text;
		}
	}
}
