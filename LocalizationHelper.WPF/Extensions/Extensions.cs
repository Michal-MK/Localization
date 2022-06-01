﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LocalizationHelper.WPF.Extensions;

public static class Extensions {
	public static ObservableCollection<T> ToObservable<T>(this IEnumerable<T> source) {
		ObservableCollection<T> ret = new();
		foreach (T item in source) {
			ret.Add(item);
		}
		return ret;
	}

	public static IEnumerable<T> ForEach<T>(this IEnumerable<T> iterable, Action<T> action) {
		foreach (T item in iterable) {
			action(item);
		}
		return iterable;
	}
}
