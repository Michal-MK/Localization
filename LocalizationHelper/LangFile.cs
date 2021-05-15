﻿using LocalizationHelper.IElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LocalizationHelper {
	public class LangFile : IElement {
		private string header;

		public List<IElement> Sections { get; } = new();

		public static LangFile Parse(string langFilePath) {
			LangFile ret = new();

			string[] lines = File.ReadAllLines(langFilePath);

			int start = 0;
			if (lines[0].StartsWith("/")) {
				ret.header = lines[0];
				start = 1;
			}

			for (int i = start; i < lines.Length; i++) {
				if (string.IsNullOrWhiteSpace(lines[i])) {
					ret.Sections.Add(new StdLine(lines[i]));
				}
				else if (lines[i].TrimStart().StartsWith("#")) {
					ret.Sections.Add(LangSection.Parse(ref i, lines));
				}
			}

			return ret;
		}

		public string GetStr() {
			return (header != null ? header + Environment.NewLine : "") +
				   string.Join(Environment.NewLine, Sections.Select(s => s.GetStr()));
		}
	}
}