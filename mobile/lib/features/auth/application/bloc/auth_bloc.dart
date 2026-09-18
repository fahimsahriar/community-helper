import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../../core/error/api_exception.dart';
import '../../domain/repositories/auth_repository.dart';
import 'auth_event.dart';
import 'auth_state.dart';

/// Coordinates login/register/logout/refresh against [AuthRepository].
class AuthBloc extends Bloc<AuthEvent, AuthState> {
  AuthBloc(this._repository) : super(const AuthInitial()) {
    on<AuthStarted>(_onStarted);
    on<AuthLoginRequested>(_onLogin);
    on<AuthRegisterRequested>(_onRegister);
    on<AuthLogoutRequested>(_onLogout);
    on<AuthTokenRefreshed>(_onTokenRefreshed);
  }

  final AuthRepository _repository;

  Future<void> _onStarted(
    AuthStarted event,
    Emitter<AuthState> emit,
  ) async {
    emit(const AuthLoading());
    final user = await _repository.restoreSession();
    if (isClosed) {
      return;
    }
    if (user == null) {
      // No usable tokens — try one silent refresh before giving up.
      final refreshed = await _repository.refreshSession();
      if (isClosed) {
        return;
      }
      if (refreshed) {
        final retried = await _repository.restoreSession();
        if (isClosed) {
          return;
        }
        if (retried != null) {
          emit(AuthAuthenticated(retried));
          return;
        }
      }
      emit(const AuthUnauthenticated());
      return;
    }
    emit(AuthAuthenticated(user));
  }

  Future<void> _onLogin(
    AuthLoginRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(const AuthLoading());
    try {
      final result = await _repository.login(
        email: event.email,
        password: event.password,
      );
      if (isClosed) {
        return;
      }
      emit(AuthAuthenticated(result.user));
    } on AuthException catch (e) {
      if (isClosed) {
        return;
      }
      emit(AuthFailure(e.failure.message));
    }
  }

  Future<void> _onRegister(
    AuthRegisterRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(const AuthLoading());
    try {
      final result = await _repository.register(
        email: event.email,
        password: event.password,
        role: event.role,
      );
      if (isClosed) {
        return;
      }
      emit(AuthAuthenticated(result.user));
    } on AuthException catch (e) {
      if (isClosed) {
        return;
      }
      emit(AuthFailure(e.failure.message));
    }
  }

  Future<void> _onLogout(
    AuthLogoutRequested event,
    Emitter<AuthState> emit,
  ) async {
    await _repository.logout();
    if (isClosed) {
      return;
    }
    emit(const AuthUnauthenticated());
  }

  Future<void> _onTokenRefreshed(
    AuthTokenRefreshed event,
    Emitter<AuthState> emit,
  ) async {
    final ok = await _repository.refreshSession();
    if (isClosed) {
      return;
    }
    if (!ok) {
      emit(const AuthUnauthenticated());
      return;
    }
    final user = await _repository.restoreSession();
    if (isClosed) {
      return;
    }
    emit(user == null ? const AuthUnauthenticated() : AuthAuthenticated(user));
  }
}
