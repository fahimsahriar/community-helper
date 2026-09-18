/// Environment constants for the mobile app.
///
/// The API base URL is overridable with `--dart-define=API_BASE_URL=...`
/// so emulators (10.0.2.2) and devices can point at a local backend.
class AppConfig {
  const AppConfig._();

  static const String apiBaseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://10.0.2.2:5000',
  );

  static const Duration connectTimeout = Duration(seconds: 10);
  static const Duration receiveTimeout = Duration(seconds: 10);
}
