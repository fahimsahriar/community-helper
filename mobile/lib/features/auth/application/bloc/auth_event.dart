import 'package:equatable/equatable.dart';

/// Auth events. The four ticket events plus [AuthStarted] for cold-boot
/// session restore from Hive.
sealed class AuthEvent extends Equatable {
  const AuthEvent();

  @override
  List<Object?> get props => [];
}

final class AuthStarted extends AuthEvent {
  const AuthStarted();
}

final class AuthLoginRequested extends AuthEvent {
  const AuthLoginRequested({required this.email, required this.password});

  final String email;
  final String password;

  @override
  List<Object?> get props => [email, password];
}

final class AuthRegisterRequested extends AuthEvent {
  const AuthRegisterRequested({
    required this.email,
    required this.password,
    required this.role,
  });

  final String email;
  final String password;
  final String role;

  @override
  List<Object?> get props => [email, password, role];
}

final class AuthLogoutRequested extends AuthEvent {
  const AuthLogoutRequested();
}

final class AuthTokenRefreshed extends AuthEvent {
  const AuthTokenRefreshed();
}
