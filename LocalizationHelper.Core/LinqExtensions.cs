using System;
using System.Collections.Generic;

namespace LocalizationHelper {
	public static class LinqExtensions {
		
		public static IEnumerable<T> ForEach<T>(this IEnumerable<T> col, Action<T> a) {
			foreach (T item in col) {
				a(item);
			}
			return col;
		}
	}
}
