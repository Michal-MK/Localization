class EnumLocalization<TEnum> {
  final Map<TEnum, String> _map;

  EnumLocalization(this._map);

  String get(TEnum? value) {
    if (_map.containsKey(value)) {
      return _map[value]!;
    } else {
      throw Exception("Value $value is not localized!");
    }
  }
}
