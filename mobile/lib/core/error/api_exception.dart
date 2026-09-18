import 'package:dio/dio.dart';

import 'failure.dart';

/// Maps Dio / API errors to typed [Failure]s.
///
/// The backend returns RFC 7807 Problem Details; we surface `detail` or
/// `title` when present, falling back to a generic message. Never includes
/// tokens or PII beyond what the server already returned for display.
Failure mapDioErrorToFailure(DioException e) {
  final status = e.response?.statusCode;
  final data = e.response?.data;

  String? problemMessage;
  if (data is Map<String, dynamic>) {
    problemMessage =
        (data['detail'] ?? data['title'] ?? data['message'])?.toString();
  }

  if (e.type == DioExceptionType.connectionTimeout ||
      e.type == DioExceptionType.sendTimeout ||
      e.type == DioExceptionType.receiveTimeout ||
      e.type == DioExceptionType.connectionError) {
    return NetworkFailure(problemMessage ?? 'No internet connection.');
  }

  return switch (status) {
    400 => ValidationFailure(problemMessage ?? 'Invalid input.'),
    401 => const UnauthorizedFailure('Session expired. Please log in again.'),
    403 => const UnauthorizedFailure(
        'You do not have access to this resource.',
      ),
    404 => NotFoundFailure(problemMessage ?? 'Not found.'),
    _ => ServerFailure(problemMessage ?? 'Something went wrong.'),
  };
}

class AuthException implements Exception {
  const AuthException(this.failure);
  final Failure failure;
}
