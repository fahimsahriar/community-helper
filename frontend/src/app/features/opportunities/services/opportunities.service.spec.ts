import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../../environments/environment';
import { Opportunity } from '../models/opportunity.model';
import { OpportunitiesService } from './opportunities.service';

const sample: Opportunity = {
  id: '1',
  title: 'Beach Cleanup',
  description: 'Collect plastic waste.',
  location: "Cox's Bazar",
  isRemote: false,
  startsAtUtc: '2026-06-01T09:30:00Z',
};

describe('OpportunitiesService', () => {
  let service: OpportunitiesService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(OpportunitiesService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('GETs the opportunities endpoint and returns the payload', () => {
    let received: readonly Opportunity[] | undefined;
    service.getAll().subscribe((result) => (received = result));

    const request = httpMock.expectOne(`${environment.apiUrl}/opportunities`);
    expect(request.request.method).toBe('GET');

    request.flush([sample]);
    expect(received).toEqual([sample]);
  });
});
