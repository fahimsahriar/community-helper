import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Observable, of, throwError } from 'rxjs';

import { Opportunity } from '../../models/opportunity.model';
import { OpportunitiesService } from '../../services/opportunities.service';
import { OpportunitiesPageComponent } from './opportunities-page.component';

const opportunity: Opportunity = {
  id: '1',
  title: 'Beach Cleanup',
  description: 'Collect plastic waste.',
  location: "Cox's Bazar",
  isRemote: false,
  startsAtUtc: '2026-06-01T09:30:00Z',
};

/** Records calls so tests can assert the API is only hit on demand. */
class FakeOpportunitiesService {
  callCount = 0;
  response: Observable<readonly Opportunity[]> = of([opportunity]);

  getAll(): Observable<readonly Opportunity[]> {
    this.callCount++;
    return this.response;
  }
}

describe('OpportunitiesPageComponent', () => {
  let fixture: ComponentFixture<OpportunitiesPageComponent>;
  let service: FakeOpportunitiesService;
  let element: HTMLElement;

  beforeEach(async () => {
    service = new FakeOpportunitiesService();

    await TestBed.configureTestingModule({
      imports: [OpportunitiesPageComponent],
      providers: [{ provide: OpportunitiesService, useValue: service }],
    }).compileComponents();

    fixture = TestBed.createComponent(OpportunitiesPageComponent);
    fixture.detectChanges();
    element = fixture.nativeElement as HTMLElement;
  });

  function clickLoad(): void {
    element.querySelector<HTMLButtonElement>('[data-testid="load-button"]')!.click();
    fixture.detectChanges();
  }

  it('does not call the API before the button is pressed', () => {
    expect(service.callCount).toBe(0);
    expect(element.querySelector('[data-testid="idle-message"]')).not.toBeNull();
    expect(element.querySelector('app-opportunity-list')).toBeNull();
  });

  it('calls the API and renders the response when the button is pressed', () => {
    clickLoad();

    expect(service.callCount).toBe(1);
    expect(element.querySelector('app-opportunity-list')).not.toBeNull();
    expect(element.textContent).toContain('Beach Cleanup');
    expect(element.querySelector('[data-testid="idle-message"]')).toBeNull();
  });

  it('shows the empty state when the API returns no opportunities', () => {
    service.response = of([]);

    clickLoad();

    expect(element.querySelector('[data-testid="empty-message"]')).not.toBeNull();
    expect(element.querySelector('app-opportunity-list')).toBeNull();
  });

  it('shows the normalized error message when the request fails', () => {
    service.response = throwError(() => new Error('Could not reach the API.'));

    clickLoad();

    const error = element.querySelector('[data-testid="error-message"]');
    expect(error?.textContent).toContain('Could not reach the API.');
    expect(element.querySelector('app-opportunity-list')).toBeNull();
  });

  it('relabels the button once results have loaded', () => {
    const button = element.querySelector<HTMLButtonElement>('[data-testid="load-button"]')!;
    expect(button.textContent).toContain('Load opportunities');

    clickLoad();

    expect(button.textContent).toContain('Reload opportunities');
  });

  it('recovers after a failed request when pressed again', () => {
    service.response = throwError(() => new Error('boom'));
    clickLoad();
    expect(element.querySelector('[data-testid="error-message"]')).not.toBeNull();

    service.response = of([opportunity]);
    clickLoad();

    expect(service.callCount).toBe(2);
    expect(element.querySelector('[data-testid="error-message"]')).toBeNull();
    expect(element.textContent).toContain('Beach Cleanup');
  });
});
