import 'package:freezed_annotation/freezed_annotation.dart';

part 'organization_model.freezed.dart';
part 'organization_model.g.dart';

/// Mirrors backend `OrganizationDto`.
/// `verifiedAtUtc == null` means the pending-verification badge.
@freezed
class OrganizationModel with _$OrganizationModel {
  const factory OrganizationModel({
    required String id,
    required String ownerUserId,
    required String name,
    required String type,
    required List<String> causeTags,
    required String location,
    required String description,
    DateTime? verifiedAtUtc,
    required bool isVerified,
  }) = _OrganizationModel;

  factory OrganizationModel.fromJson(Map<String, dynamic> json) =>
      _$OrganizationModelFromJson(json);
}

extension OrganizationModelX on OrganizationModel {
  bool get isPendingVerification => verifiedAtUtc == null && !isVerified;
}
