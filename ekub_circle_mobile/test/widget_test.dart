import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:ekub_circle_app/main.dart';
import 'package:ekub_circle_app/services/api_service.dart';
import 'package:ekub_circle_app/services/auth_service.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  testWidgets('EkubCircle smoke test', (WidgetTester tester) async {
    SharedPreferences.setMockInitialValues({});
    final api = ApiService();
    final auth = AuthService(api);
    await auth.init();

    await tester.pumpWidget(
      MultiProvider(
        providers: [
          Provider<ApiService>.value(value: api),
          ChangeNotifierProvider<AuthService>.value(value: auth),
        ],
        child: const EkubCircleApp(),
      ),
    );

    await tester.pumpAndSettle();

    // Verify login screen renders
    expect(find.text('EkubCircle'), findsWidgets);
    expect(find.text('Sign in to manage your circles, rounds and payouts'), findsOneWidget);
  });
}

