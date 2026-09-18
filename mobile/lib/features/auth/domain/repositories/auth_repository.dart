import '../../data/models/auth_response_model.dart';
import '../../data/models/user_model.dart';

/// Repository interface for auth. UI/BLoC depend only on this.
abstract class AuthRepository {
  Future<AuthResponseModel> register({
    required String email,
    required String password,
    required String role,
  });

  Future<AuthResponseModel> login({
    required String email,
    required String password,
  });

  /// Returns true when the stored refresh token was rotated successfully.
  Future<bool> refreshSession();

  Future<void> logout();

  /// Restores a persisted session (tokens exist + `/me` succeeds).
  Future<UserModel?> restoreSession();

  Future<String?> readAccessToken();
  Future<String?> readRefreshToken();
}
