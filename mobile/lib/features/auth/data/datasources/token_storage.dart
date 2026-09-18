import 'package:hive_flutter/hive_flutter.dart';

/// Hive-backed token persistence. Survives app restarts.
///
/// Box: `auth_box`, keys: `access_token`, `refresh_token`.
class TokenStorage {
  TokenStorage(this._box);

  final Box<String> _box;

  static const String boxName = 'auth_box';
  static const String accessTokenKey = 'access_token';
  static const String refreshTokenKey = 'refresh_token';

  static Future<TokenStorage> init() async {
    await Hive.initFlutter();
    final box = await Hive.openBox<String>(boxName);
    return TokenStorage(box);
  }

  /// Test seam: wraps an already-open box.
  static TokenStorage forTest(Box<String> box) => TokenStorage(box);

  Future<String?> readAccessToken() async => _box.get(accessTokenKey);
  Future<String?> readRefreshToken() async => _box.get(refreshTokenKey);

  Future<void> saveTokens({
    required String accessToken,
    required String refreshToken,
  }) async {
    await _box.put(accessTokenKey, accessToken);
    await _box.put(refreshTokenKey, refreshToken);
  }

  Future<void> clear() async {
    await _box.delete(accessTokenKey);
    await _box.delete(refreshTokenKey);
  }
}
