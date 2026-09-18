import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../../environments/environment';
import { VolunteerProfile } from '../models/volunteer-profile.model';
import { VolunteerProfileService } from './volunteer-profile.service';

const sample: VolunteerProfile = {
  userId: 'u-1',
  skills: ['teaching', 'first-aid'],
  availability: 'Weekends',
  causes: ['education'],
  location: 'Dhaka',
  bio: 'Volunteer teacher.',
};

describe('VolunteerProfileService', () => {
  let service: VolunteerProfileService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(VolunteerProfileService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('GETs the current volunteer profile', () => {
    let received: VolunteerProfile | null | undefined;
    service.getMineOrNull().subscribe((result) => (received = result));

    const request = httpMock.expectOne(`${environment.apiUrl}/volunteer-profiles/me`);
    expect(request.request.method).toBe('GET');

    request.flush(sample);
    expect(received).toEqual(sample);
  });

  it('maps a 404 to null when no profile exists yet', () => {
    let received: VolunteerProfile | null | undefined = sample;
    service.getMineOrNull().subscribe((result) => (received = result));

    httpMock
      .expectOne(`${environment.apiUrl}/volunteer-profiles/me`)
      .flush({ title: 'Volunteer profile was not found.' }, { status: 404, statusText: 'Not Found' });

    expect(received).toBeNull();
  });

  it('rethows non-404 failures', () => {
    let message: string | undefined;
    service.getMineOrNull().subscribe({ error: (err: Error) => (message = err.message) });

    httpMock
      .expectOne(`${environment.apiUrl}/volunteer-profiles/me`)
      .flush(null, { status: 0, statusText: 'Unknown Error' });

    expect(message).toBeDefined();
  });

  it('POSTs a new volunteer profile', () => {
    const payload = {
      skills: [...sample.skills],
      availability: sample.availability,
      causes: [...sample.causes],
      location: sample.location,
      bio: sample.bio,
    };
    service.create(payload).subscribe();

    const request = httpMock.expectOne(`${environment.apiUrl}/volunteer-profiles/me`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);
    request.flush(sample);
  });

  it('PUTs volunteer profile edits', () => {
    const payload = {
      skills: ['mentoring'],
      availability: 'Evenings',
      causes: ['health'],
      location: 'Chittagong',
      bio: 'Updated bio.',
    };
    service.update(payload).subscribe();

    const request = httpMock.expectOne(`${environment.apiUrl}/volunteer-profiles/me`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(payload);
    request.flush({ ...sample, ...payload });
  });
});
