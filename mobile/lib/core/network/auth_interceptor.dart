import 'dart:async';

import 'package:dio/dio.dart';

typedef TokenProvider = Future<String?> Function();
typedef RefreshTokenHandler = Future<bool> Function();

/// Attaches the access JWT and silently refreshes once on 401.
///
/// Flow: request adds `Authorization: Bearer <access>` when present. On a
/// 401 (not from `/api/auth/*` itself) it runs [refreshHandler] once, then
/// retries the original request a single time. Concurrent 401s share one
/// refresh via an internal completer.
class AuthInterceptor extends Interceptor {
  AuthInterceptor({
    required Dio dio,
    required TokenProvider tokenProvider,
    required RefreshTokenHandler refreshHandler,
  })  : _dio = dio,
        _tokenProvider = tokenProvider,
        _refreshHandler = refreshHandler;

  final Dio _dio;
  final TokenProvider _tokenProvider;
  final RefreshTokenHandler _refreshHandler;

  Completer<bool>? _refreshCompleter;

  static bool _isAuthPath(String path) => path.contains('/api/auth/');

  @override
  Future<void> onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    final token = await _tokenProvider();
    if (token != null && token.isNotEmpty) {
      options.headers['Authorization'] = 'Bearer $token';
    }
    handler.next(options);
  }

  @override
  Future<void> onError(
    DioException err,
    ErrorInterceptorHandler handler,
  ) async {
    final status = err.response?.statusCode;
    final request = err.requestOptions;

    final alreadyRetried = request.extra['__retried'] == true;
    if (status != 401 || alreadyRetried || _isAuthPath(request.path)) {
      handler.next(err);
      return;
    }

    try {
      final refreshed = await _runSingleRefresh();
      if (!refreshed) {
        handler.next(err);
        return;
      }

      final token = await _tokenProvider();
      request.extra['__retried'] = true;
      if (token != null && token.isNotEmpty) {
        request.headers['Authorization'] = 'Bearer $token';
      }

      final response = await _dio.fetch<dynamic>(request);
      handler.resolve(response);
    } catch (_) {
      handler.next(err);
    }
  }

  Future<bool> _runSingleRefresh() {
    final existing = _refreshCompleter;
    if (existing != null) {
      return existing.future;
    }
    final completer = Completer<bool>();
    _refreshCompleter = completer;
    _refreshHandler().then((ok) {
      _refreshCompleter = null;
      completer.complete(ok);
    }).catchError((Object e) {
      _refreshCompleter = null;
      completer.complete(false);
    });
    return completer.future;
  }
}
