import 'package:dio/dio.dart';

import '../config/app_config.dart';
import 'auth_interceptor.dart';

/// Creates the shared Dio instance used by all remote data sources.
///
/// JWT attach + 401 silent refresh live in [AuthInterceptor]; UI and BLoCs
/// never touch Dio directly — only repositories do.
Dio createDio({
  required TokenProvider tokenProvider,
  required RefreshTokenHandler refreshHandler,
}) {
  final dio = Dio(
    BaseOptions(
      baseUrl: AppConfig.apiBaseUrl,
      connectTimeout: AppConfig.connectTimeout,
      receiveTimeout: AppConfig.receiveTimeout,
      contentType: Headers.jsonContentType,
      responseType: ResponseType.json,
    ),
  );

  dio.interceptors.add(
    AuthInterceptor(
      dio: dio,
      tokenProvider: tokenProvider,
      refreshHandler: refreshHandler,
    ),
  );

  return dio;
}

/// A plain-Dio instance without interceptors, used for the refresh call
/// itself to avoid recursive 401 handling.
Dio createRefreshDio() {
  return Dio(
    BaseOptions(
      baseUrl: AppConfig.apiBaseUrl,
      connectTimeout: AppConfig.connectTimeout,
      receiveTimeout: AppConfig.receiveTimeout,
      contentType: Headers.jsonContentType,
      responseType: ResponseType.json,
    ),
  );
}
