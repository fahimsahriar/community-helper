import 'package:dio/dio.dart';

import '../../../../core/error/api_exception.dart';
import '../../domain/repositories/auth_repository.dart';
import '../datasources/auth_remote_data_source.dart';
import '../datasources/token_storage.dart';
import '../models/auth_response_model.dart';
import '../models/user_model.dart';

/// Persists JWT pairs in Hive; maps Dio errors to [AuthException].
class AuthRepositoryImpl implements AuthRepository {
  AuthRepositoryImpl({
    required AuthRemoteDataSource remote,
    required AuthRemoteDataSource refreshRemote,
    required TokenStorage storage,
  })  : _remote = remote,
        _refreshRemote = refreshRemote,
        _storage = storage;

  final AuthRemoteDataSource _remote;
  final AuthRemoteDataSource _refreshRemote;
  final TokenStorage _storage;

  @override
  Future<String?> readAccessToken() => _storage.readAccessToken();

  @override
  Future<String?> readRefreshToken() => _storage.readRefreshToken();

  @override
  Future<AuthResponseModel> register({
    required String email,
    required String password,
    required String role,
  }) async {
    try {
      final result = await _remote.register(
        email: email,
        password: password,
        role: role,
      );
      await _storage.saveTokens(
        accessToken: result.accessToken,
        refreshToken: result.refreshToken,
      );
      return result;
    } on DioException catch (e) {
      throw AuthException(mapDioErrorToFailure(e));
    }
  }

  @override
  Future<AuthResponseModel> login({
    required String email,
    required String password,
  }) async {
    try {
      final result = await _remote.login(email: email, password: password);
      await _storage.saveTokens(
        accessToken: result.accessToken,
        refreshToken: result.refreshToken,
      );
      return result;
    } on DioException catch (e) {
      throw AuthException(mapDioErrorToFailure(e));
    }
  }

  @override
  Future<bool> refreshSession() async {
    final stored = await _storage.readRefreshToken();
    if (stored == null || stored.isEmpty) {
      return false;
    }
    try {
      final result = await _refreshRemote.refresh(stored);
      await _storage.saveTokens(
        accessToken: result.accessToken,
        refreshToken: result.refreshToken,
      );
      return true;
    } on DioException {
      await _storage.clear();
      return false;
    }
  }

  @override
  Future<void> logout() async {
    final stored = await _storage.readRefreshToken();
    if (stored != null && stored.isNotEmpty) {
      try {
        await _remote.logout(stored);
      } on DioException {
        // Best-effort server revocation; local clear still happens.
      }
    }
    await _storage.clear();
  }

  @override
  Future<UserModel?> restoreSession() async {
    final access = await _storage.readAccessToken();
    final refresh = await _storage.readRefreshToken();
    if ((access == null || access.isEmpty) &&
        (refresh == null || refresh.isEmpty)) {
      return null;
    }
    try {
      return await _remote.me();
    } on DioException {
      return null;
    }
  }
}
