import 'package:freezed_annotation/freezed_annotation.dart';

part 'user_model.freezed.dart';
part 'user_model.g.dart';

/// Authenticated user identity. Mirrors backend `CurrentUserDto`.
@freezed
class UserModel with _$UserModel {
  const factory UserModel({
    required String id,
    required String email,
    required String role,
  }) = _UserModel;

  factory UserModel.fromJson(Map<String, dynamic> json) =>
      _$UserModelFromJson(json);
}

/// App roles in Phase 1. Unknown roles get 403 via backend guards.
class AppRoles {
  const AppRoles._();
  static const String volunteer = 'volunteer';
  static const String orgAdmin = 'org_admin';

  static bool isKnown(String? role) =>
      role == volunteer || role == orgAdmin;
}
