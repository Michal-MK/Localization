using System;

namespace Igor.Localization {
	internal struct LocalizationRule {
		public string Match { get; set; }
		public string Result { get; set; }

		private bool IsRange => Match.Contains("-");

		private (int min, int max) GetRange() {
			if (!IsRange) {
				throw new InvalidOperationException(Match + " < NOT A RANGE");
			}
			string[] split = Match.Split('-');
			if (split[0] == "") {
				return (int.MinValue, int.Parse(split[1]));
			}
			else if (split[1] == "") {
				return (int.Parse(split[0]), int.MaxValue);
			}
			else {
				return (int.Parse(split[0]), int.Parse(split[1]));
			}
		}

		private int GetValue() {
			if (IsRange) {
				throw new InvalidOperationException(Match + " < IS A RANGE");
			}
			return int.Parse(Match);
		}

		public bool IsMatch(string arg) {
			int argInt = int.Parse(arg);
			if (IsRange) {
				(int min, int max) = GetRange();
				return argInt >= min && argInt < max;
			}
			else {
				return argInt == GetValue();
			}
		}
	}
}
