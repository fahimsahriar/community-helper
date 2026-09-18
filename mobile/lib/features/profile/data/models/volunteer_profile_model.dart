import 'package:freezed_annotation/freezed_annotation.dart';

part 'volunteer_profile_model.freezed.dart';
part 'volunteer_profile_model.g.dart';

/// Mirrors backend `VolunteerProfileDto`.
@freezed
class VolunteerProfileModel with _$VolunteerProfileModel {
  const factory VolunteerProfileModel({
    required String userId,
    required List<String> skills,
    required String availability,
    required List<String> causes,
    required String location,
    required String bio,
  }) = _VolunteerProfileModel;

  factory VolunteerProfileModel.fromJson(Map<String, dynamic> json) =>
      _$VolunteerProfileModelFromJson(json);
}
