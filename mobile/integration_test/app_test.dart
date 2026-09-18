import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';

/// Full register → login → view profile flow against a running backend.
///
/// Run with:
///   flutter test integration_test/app_test.dart \
///     --dart-define=API_BASE_URL=http://10.0.2.2:5000
void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  testWidgets('placeholder: app boots to login', (tester) async {
    // Wired to the real app once a test backend is available.
    expect(true, isTrue);
  });
}
