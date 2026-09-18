import 'package:dio/dio.dart';
import 'package:get_it/get_it.dart';

import 'core/network/dio_client.dart';
import 'features/auth/application/bloc/auth_bloc.dart';
import 'features/auth/data/datasources/auth_remote_data_source.dart';
import 'features/auth/data/datasources/token_storage.dart';
import 'features/auth/data/repositories/auth_repository_impl.dart';
import 'features/auth/domain/repositories/auth_repository.dart';
import 'features/profile/application/cubit/profile_cubit.dart';
import 'features/profile/data/datasources/profile_remote_data_source.dart';
import 'features/profile/data/repositories/profile_repository_impl.dart';
import 'features/profile/domain/repositories/profile_repository.dart';

final sl = GetIt.instance;

/// Registers Dio, storage, repositories, and BLoCs. Call once from `main`.
Future<void> configureDependencies({required TokenStorage storage}) async {
  sl.registerSingleton<TokenStorage>(storage);

  // Note: the authed Dio's callbacks resolve AuthRepository lazily at
  // request time so there is no circular init between Dio and repository.
  sl.registerLazySingleton<Dio>(
    () => createDio(
      tokenProvider: () => sl<AuthRepository>().readAccessToken(),
      refreshHandler: () => sl<AuthRepository>().refreshSession(),
    ),
    instanceName: 'authed',
  );
  sl.registerLazySingleton<Dio>(
    createRefreshDio,
    instanceName: 'refresh',
  );

  sl.registerLazySingleton<AuthRepository>(
    () => AuthRepositoryImpl(
      remote: AuthRemoteDataSource(sl<Dio>(instanceName: 'authed')),
      refreshRemote: AuthRemoteDataSource(sl<Dio>(instanceName: 'refresh')),
      storage: sl<TokenStorage>(),
    ),
  );

  sl.registerLazySingleton<ProfileRepository>(
    () => ProfileRepositoryImpl(
      ProfileRemoteDataSource(sl<Dio>(instanceName: 'authed')),
    ),
  );

  sl.registerFactory<AuthBloc>(() => AuthBloc(sl<AuthRepository>()));
  sl.registerFactory<ProfileCubit>(() => ProfileCubit(sl<ProfileRepository>()));
}
