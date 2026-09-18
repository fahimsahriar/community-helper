import 'package:dio/dio.dart';

import '../models/auth_response_model.dart';
import '../models/user_model.dart';

/// Thin Dio wrapper over the Phase 1 auth API.
///
/// Base path is `/api/auth`; the shared Dio already sets the host.
class AuthRemoteDataSource {
  AuthRemoteDataSource(this._dio);

  final Dio _dio;

  Future<AuthResponseModel> register({
    required String email,
    required String password,
    required String role,
  }) async {
    final res = await _dio.post<Map<String, dynamic>>(
      '/api/auth/register',
      data: {'email': email, 'password': password, 'role': role},
    );
    // Guaranteed JSON object on success; comment justifies non-null access.
    return AuthResponseModel.fromJson(res.data ?? <String, dynamic>{});
  }

  Future<AuthResponseModel> login({
    required String email,
    required String password,
  }) async {
    final res = await _dio.post<Map<String, dynamic>>(
      '/api/auth/login',
      data: {'email': email, 'password': password},
    );
    return AuthResponseModel.fromJson(res.data ?? <String, dynamic>{});
  }

  Future<AuthResponseModel> refresh(String refreshToken) async {
    // Uses the plain refresh Dio (no interceptor) when wired via get_it.
    final res = await _dio.post<Map<String, dynamic>>(
      '/api/auth/refresh',
      data: {'refreshToken': refreshToken},
    );
    return AuthResponseModel.fromJson(res.data ?? <String, dynamic>{});
  }

  Future<void> logout(String refreshToken) async {
    await _dio.post<void>(
      '/api/auth/logout',
      data: {'refreshToken': refreshToken},
    );
  }

  Future<UserModel> me() async {
    final res = await _dio.get<Map<String, dynamic>>('/api/auth/me');
    return UserModel.fromJson(res.data ?? <String, dynamic>{});
  }
}
