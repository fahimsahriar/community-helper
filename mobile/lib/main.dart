import 'package:flutter/material.dart';
import 'package:hive_flutter/hive_flutter.dart';

import 'app.dart';
import 'features/auth/data/datasources/token_storage.dart';
import 'injection.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await Hive.initFlutter();
  final box = await Hive.openBox<String>(TokenStorage.boxName);
  await configureDependencies(
    storage: TokenStorage.forTest(box),
  );
  runApp(const CommunityHelperApp());
}
