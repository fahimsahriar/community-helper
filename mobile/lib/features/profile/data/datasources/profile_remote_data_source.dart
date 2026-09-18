import 'package:dio/dio.dart';

import '../models/organization_model.dart';
import '../models/volunteer_profile_model.dart';

/// Volunteer + organization profile endpoints (Phase 1).
class ProfileRemoteDataSource {
  ProfileRemoteDataSource(this._dio);

  final Dio _dio;

  // --- Volunteer ---

  Future<VolunteerProfileModel> getMyVolunteerProfile() async {
    final res = await _dio.get<Map<String, dynamic>>(
      '/api/volunteer-profiles/me',
    );
    return VolunteerProfileModel.fromJson(res.data ?? <String, dynamic>{});
  }

  Future<VolunteerProfileModel> createMyVolunteerProfile({
    required List<String> skills,
    required String availability,
    required List<String> causes,
    required String location,
    required String bio,
  }) async {
    final res = await _dio.post<Map<String, dynamic>>(
      '/api/volunteer-profiles/me',
      data: {
        'skills': skills,
        'availability': availability,
        'causes': causes,
        'location': location,
        'bio': bio,
      },
    );
    return VolunteerProfileModel.fromJson(res.data ?? <String, dynamic>{});
  }

  Future<VolunteerProfileModel> updateMyVolunteerProfile({
    required List<String> skills,
    required String availability,
    required List<String> causes,
    required String location,
    required String bio,
  }) async {
    final res = await _dio.put<Map<String, dynamic>>(
      '/api/volunteer-profiles/me',
      data: {
        'skills': skills,
        'availability': availability,
        'causes': causes,
        'location': location,
        'bio': bio,
      },
    );
    return VolunteerProfileModel.fromJson(res.data ?? <String, dynamic>{});
  }

  // --- Organization ---

  Future<OrganizationModel> registerOrganization({
    required String name,
    required String type,
    required List<String> causeTags,
    required String location,
    required String description,
  }) async {
    final res = await _dio.post<Map<String, dynamic>>(
      '/api/organizations',
      data: {
        'name': name,
        'type': type,
        'causeTags': causeTags,
        'location': location,
        'description': description,
      },
    );
    return OrganizationModel.fromJson(res.data ?? <String, dynamic>{});
  }

  Future<OrganizationModel> getMyOrganization() async {
    final res = await _dio.get<Map<String, dynamic>>('/api/organizations/me');
    return OrganizationModel.fromJson(res.data ?? <String, dynamic>{});
  }

  Future<OrganizationModel> updateOrganization({
    required String id,
    required String name,
    required String type,
    required List<String> causeTags,
    required String location,
    required String description,
  }) async {
    final res = await _dio.put<Map<String, dynamic>>(
      '/api/organizations/$id',
      data: {
        'name': name,
        'type': type,
        'causeTags': causeTags,
        'location': location,
        'description': description,
      },
    );
    return OrganizationModel.fromJson(res.data ?? <String, dynamic>{});
  }
}
