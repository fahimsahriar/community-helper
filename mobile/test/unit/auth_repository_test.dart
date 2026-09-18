import 'package:community_helper/core/error/api_exception.dart';
import 'package:community_helper/features/auth/data/datasources/auth_remote_data_source.dart';
import 'package:community_helper/features/auth/data/datasources/token_storage.dart';
import 'package:community_helper/features/auth/data/models/auth_response_model.dart';
import 'package:community_helper/features/auth/data/models/user_model.dart';
import 'package:community_helper/features/auth/data/repositories/auth_repository_impl.dart';
import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';

class MockAuthRemoteDataSource extends Mock implements AuthRemoteDataSource {}

class MockTokenStorage extends Mock implements TokenStorage {}

void main() {
  const user = UserModel(
    id: 'u1',
    email: 'vol@example.com',
    role: 'volunteer',
  );
  final authResponse = AuthResponseModel(
    accessToken: 'access',
    refreshToken: 'refresh',
    expiresAtUtc: DateTime.utc(2030),
    user: user,
  );

  late MockAuthRemoteDataSource remote;
  late MockAuthRemoteDataSource refreshRemote;
  late MockTokenStorage storage;
  late AuthRepositoryImpl repository;

  setUpAll(() {
    registerFallbackValue('');
  });

  setUp(() {
    remote = MockAuthRemoteDataSource();
    refreshRemote = MockAuthRemoteDataSource();
    storage = MockTokenStorage();
    repository = AuthRepositoryImpl(
      remote: remote,
      refreshRemote: refreshRemote,
      storage: storage,
    );

    when(() => storage.saveTokens(
          accessToken: any(named: 'accessToken'),
          refreshToken: any(named: 'refreshToken'),
        )).thenAnswer((_) async {});
    when(() => storage.clear()).thenAnswer((_) async {});
  });

  DioException unauthorized() => DioException(
        requestOptions: RequestOptions(path: '/api/auth/login'),
        response: Response(
          requestOptions: RequestOptions(path: '/api/auth/login'),
          statusCode: 401,
        ),
      );

  group('AuthRepository persistence', () {
    test('login persists the rotated token pair', () async {
      when(() => remote.login(email: 'vol@example.com', password: 'pw123456'))
          .thenAnswer((_) async => authResponse);

      final result = await repository.login(
        email: 'vol@example.com',
        password: 'pw123456',
      );

      expect(result.user, user);
      verify(() => storage.saveTokens(
            accessToken: 'access',
            refreshToken: 'refresh',
          )).called(1);
    });

    test('login maps API errors without persisting', () async {
      when(() => remote.login(
            email: any(named: 'email'),
            password: any(named: 'password'),
          )).thenThrow(unauthorized());

      await expectLater(
        repository.login(email: 'vol@example.com', password: 'wrongpass1'),
        throwsA(isA<AuthException>()),
      );
      verifyNever(() => storage.saveTokens(
            accessToken: any(named: 'accessToken'),
            refreshToken: any(named: 'refreshToken'),
          ));
    });

    test('refreshSession rotates tokens and returns true', () async {
      when(() => storage.readRefreshToken()).thenAnswer((_) async => 'old');
      when(() => refreshRemote.refresh('old'))
          .thenAnswer((_) async => authResponse);

      expect(await repository.refreshSession(), isTrue);
      verify(() => storage.saveTokens(
            accessToken: 'access',
            refreshToken: 'refresh',
          )).called(1);
    });

    test('refreshSession clears storage and returns false when revoked', () async {
      when(() => storage.readRefreshToken()).thenAnswer((_) async => 'old');
      when(() => refreshRemote.refresh('old')).thenThrow(unauthorized());

      expect(await repository.refreshSession(), isFalse);
      verify(() => storage.clear()).called(1);
    });

    test('restoreSession returns null with no stored tokens', () async {
      when(() => storage.readAccessToken()).thenAnswer((_) async => null);
      when(() => storage.readRefreshToken()).thenAnswer((_) async => null);

      expect(await repository.restoreSession(), isNull);
    });

    test('logout revokes remotely then clears local tokens', () async {
      when(() => storage.readRefreshToken())
          .thenAnswer((_) async => 'refresh');
      when(() => remote.logout('refresh')).thenAnswer((_) async {});

      await repository.logout();

      verify(() => remote.logout('refresh')).called(1);
      verify(() => storage.clear()).called(1);
    });
  });
}
