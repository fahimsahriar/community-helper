import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../../environments/environment';
import { Organization } from '../models/organization.model';
import { OrganizationService } from './organization.service';

const sample: Organization = {
  id: 'org-1',
  ownerUserId: 'u-9',
  name: 'River Cleaners',
  type: 'Nonprofit',
  causeTags: ['environment'],
  location: 'Dhaka',
  description: 'Cleaning rivers every weekend.',
  verifiedAtUtc: null,
  isVerified: false,
};

describe('OrganizationService', () => {
  let service: OrganizationService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(OrganizationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('GETs the current organization', () => {
    let received: Organization | null | undefined;
    service.getMineOrNull().subscribe((result) => (received = result));

    const request = httpMock.expectOne(`${environment.apiUrl}/organizations/me`);
    expect(request.request.method).toBe('GET');

    request.flush(sample);
    expect(received).toEqual(sample);
  });

  it('maps a 404 to null when no organization is registered yet', () => {
    let received: Organization | null | undefined = sample;
    service.getMineOrNull().subscribe((result) => (received = result));

    httpMock
      .expectOne(`${environment.apiUrl}/organizations/me`)
      .flush({ title: 'Organization was not found.' }, { status: 404, statusText: 'Not Found' });

    expect(received).toBeNull();
  });

  it('rethows non-404 failures', () => {
    let failed = false;
    service.getMineOrNull().subscribe({ error: () => (failed = true) });

    httpMock
      .expectOne(`${environment.apiUrl}/organizations/me`)
      .flush(null, { status: 0, statusText: 'Unknown Error' });

    expect(failed).toBe(true);
  });

  it('POSTs organization registration', () => {
    const payload = {
      name: sample.name,
      type: sample.type,
      causeTags: [...sample.causeTags],
      location: sample.location,
      description: sample.description,
    };
    service.register(payload).subscribe();

    const request = httpMock.expectOne(`${environment.apiUrl}/organizations`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);
    request.flush(sample);
  });

  it('PUTs organization edits by id', () => {
    const payload = {
      name: 'River Cleaners 2',
      type: sample.type,
      causeTags: [...sample.causeTags],
      location: sample.location,
      description: sample.description,
    };
    service.update('org-1', payload).subscribe();

    const request = httpMock.expectOne(`${environment.apiUrl}/organizations/org-1`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(payload);
    request.flush({ ...sample, name: payload.name });
  });
});
