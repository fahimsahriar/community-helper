import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { Observable, of, throwError } from 'rxjs';

import { Organization, OrganizationPayload } from '../../models/organization.model';
import { OrganizationService } from '../../services/organization.service';
import { OrgDashboardComponent } from './org-dashboard.component';

const pending: Organization = {
  id: 'org-1',
  ownerUserId: 'u-9',
  name: 'River Cleaners',
  type: 'Nonprofit',
  causeTags: ['environment'],
  location: 'Dhaka',
  description: 'Cleaning rivers.',
  verifiedAtUtc: null,
  isVerified: false,
};

const verified: Organization = {
  ...pending,
  verifiedAtUtc: '2026-09-01T00:00:00Z',
  isVerified: true,
};

class FakeOrganizationService {
  getMineOrNullResponse: Observable<Organization | null> = of(pending);
  saveResponse: Observable<Organization> = of(pending);
  registerCalls: OrganizationPayload[] = [];
  updateCalls: Array<{ id: string; payload: OrganizationPayload }> = [];

  getMineOrNull(): Observable<Organization | null> {
    return this.getMineOrNullResponse;
  }

  register(payload: OrganizationPayload): Observable<Organization> {
    this.registerCalls.push(payload);
    return this.saveResponse;
  }

  update(id: string, payload: OrganizationPayload): Observable<Organization> {
    this.updateCalls.push({ id, payload });
    return this.saveResponse;
  }
}

describe('OrgDashboardComponent', () => {
  let fixture: ComponentFixture<OrgDashboardComponent>;
  let service: FakeOrganizationService;
  let element: HTMLElement;

  async function setup(configure?: (svc: FakeOrganizationService) => void): Promise<void> {
    service = new FakeOrganizationService();
    configure?.(service);
    await TestBed.configureTestingModule({
      imports: [OrgDashboardComponent],
      providers: [
        provideNoopAnimations(),
        { provide: OrganizationService, useValue: service },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(OrgDashboardComponent);
    fixture.detectChanges();
    element = fixture.nativeElement as HTMLElement;
  }

  function setFormField(testid: string, value: string): void {
    const input = element.querySelector<HTMLInputElement | HTMLTextAreaElement>(
      `[data-testid="${testid}"]`,
    )!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  function submitForm(): void {
    element.querySelector<HTMLButtonElement>('[data-testid="org-save"]')!.click();
    fixture.detectChanges();
  }

  it('shows the org shell title', async () => {
    await setup();

    expect(element.querySelector('[data-testid="org-dashboard-title"]')).not.toBeNull();
  });

  it('shows the pending-verified badge when verified_at is null', async () => {
    await setup();

    expect(element.querySelector('[data-testid="org-pending-badge"]')?.textContent).toContain(
      'Pending verification',
    );
    expect(element.querySelector('[data-testid="org-verified-badge"]')).toBeNull();
  });

  it('shows the verified badge once the organization is verified', async () => {
    await setup((svc) => {
      svc.getMineOrNullResponse = of(verified);
    });

    expect(element.querySelector('[data-testid="org-verified-badge"]')).not.toBeNull();
    expect(element.querySelector('[data-testid="org-pending-badge"]')).toBeNull();
  });

  it('shows the registration flow when no organization exists yet', async () => {
    await setup((svc) => {
      svc.getMineOrNullResponse = of(null);
    });

    expect(element.querySelector('[data-testid="org-setup-heading"]')).not.toBeNull();
    expect(element.querySelector('[data-testid="org-edit-heading"]')).toBeNull();
    expect(element.querySelector('app-organization-form')).not.toBeNull();
  });

  it('registers the organization from the setup form and confirms', async () => {
    await setup((svc) => {
      svc.getMineOrNullResponse = of(null);
      svc.saveResponse = of(pending);
    });

    setFormField('org-name', 'River Cleaners');
    setFormField('org-type', 'Nonprofit');
    setFormField('org-cause-tags', 'environment');
    setFormField('org-location', 'Dhaka');
    setFormField('org-description', 'Cleaning rivers.');
    submitForm();

    expect(service.registerCalls).toHaveLength(1);
    expect(service.updateCalls).toHaveLength(0);
    expect(element.querySelector('[data-testid="org-saved"]')?.textContent).toContain(
      'Organization saved.',
    );
    expect(element.querySelector('[data-testid="org-pending-badge"]')).not.toBeNull();
  });

  it('updates the existing organization from the edit view', async () => {
    await setup();

    setFormField('org-location', 'Chittagong');
    submitForm();

    expect(service.updateCalls).toHaveLength(1);
    expect(service.updateCalls[0]?.id).toBe('org-1');
    expect(service.updateCalls[0]?.payload.location).toBe('Chittagong');
  });

  it('shows the normalized error when loading fails for a real reason', async () => {
    await setup((svc) => {
      svc.getMineOrNullResponse = throwError(() => new Error('Could not reach the API.'));
    });

    expect(element.querySelector('[data-testid="org-error"]')?.textContent).toContain(
      'Could not reach the API.',
    );
    expect(element.querySelector('app-organization-form')).toBeNull();
  });
});
