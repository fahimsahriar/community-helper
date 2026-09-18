import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { Observable, of, throwError } from 'rxjs';

import { VolunteerProfile, VolunteerProfilePayload } from '../../models/volunteer-profile.model';
import { VolunteerProfileService } from '../../services/volunteer-profile.service';
import { ProfileComponent } from './profile.component';

const profile: VolunteerProfile = {
  userId: 'u-1',
  skills: ['teaching'],
  availability: 'Weekends',
  causes: ['education'],
  location: 'Dhaka',
  bio: 'Teacher.',
};

/** Fake records calls and lets each test choose the response per method. */
class FakeVolunteerProfileService {
  getMineOrNullResponse: Observable<VolunteerProfile | null> = of(profile);
  saveResponse: Observable<VolunteerProfile> = of(profile);
  createCalls: VolunteerProfilePayload[] = [];
  updateCalls: VolunteerProfilePayload[] = [];

  getMineOrNull(): Observable<VolunteerProfile | null> {
    return this.getMineOrNullResponse;
  }

  create(payload: VolunteerProfilePayload): Observable<VolunteerProfile> {
    this.createCalls.push(payload);
    return this.saveResponse;
  }

  update(payload: VolunteerProfilePayload): Observable<VolunteerProfile> {
    this.updateCalls.push(payload);
    return this.saveResponse;
  }
}

describe('ProfileComponent', () => {
  let fixture: ComponentFixture<ProfileComponent>;
  let service: FakeVolunteerProfileService;
  let element: HTMLElement;

  async function setup(configure?: (svc: FakeVolunteerProfileService) => void): Promise<void> {
    service = new FakeVolunteerProfileService();
    configure?.(service);
    await TestBed.configureTestingModule({
      imports: [ProfileComponent],
      providers: [
        provideNoopAnimations(),
        { provide: VolunteerProfileService, useValue: service },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ProfileComponent);
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
    element.querySelector<HTMLButtonElement>('[data-testid="volunteer-save"]')!.click();
    fixture.detectChanges();
  }

  it('shows the volunteer shell with an hours placeholder', async () => {
    await setup();

    expect(element.querySelector('[data-testid="profile-title"]')).not.toBeNull();
    expect(element.querySelector('[data-testid="profile-hours-placeholder"]')).not.toBeNull();
    expect(element.querySelector('[data-testid="profile-hours-total"]')?.textContent).toContain(
      '0 hours verified',
    );
  });

  it('shows the setup flow when no profile exists yet', async () => {
    await setup((svc) => {
      svc.getMineOrNullResponse = of(null);
    });

    expect(element.querySelector('[data-testid="profile-setup-heading"]')).not.toBeNull();
    expect(element.querySelector('[data-testid="profile-edit-heading"]')).toBeNull();
    expect(element.querySelector('[data-testid="profile-error"]')).toBeNull();
    expect(element.querySelector('app-volunteer-profile-form')).not.toBeNull();
  });

  it('shows the edit view when a profile exists', async () => {
    await setup();

    expect(element.querySelector('[data-testid="profile-edit-heading"]')).not.toBeNull();
    expect(element.querySelector('[data-testid="profile-setup-heading"]')).toBeNull();
  });

  it('creates the profile from the setup form and confirms', async () => {
    await setup((svc) => {
      svc.getMineOrNullResponse = of(null);
      svc.saveResponse = of(profile);
    });

    setFormField('volunteer-skills', 'teaching');
    setFormField('volunteer-availability', 'Weekends');
    setFormField('volunteer-causes', 'education');
    setFormField('volunteer-location', 'Dhaka');
    setFormField('volunteer-bio', 'Teacher.');
    submitForm();

    expect(service.createCalls).toHaveLength(1);
    expect(service.updateCalls).toHaveLength(0);
    expect(element.querySelector('[data-testid="profile-saved"]')?.textContent).toContain(
      'Profile saved.',
    );
    expect(element.querySelector('[data-testid="profile-edit-heading"]')).not.toBeNull();
  });

  it('updates the existing profile from the edit view', async () => {
    const updated: VolunteerProfile = { ...profile, location: 'Chittagong' };
    await setup((svc) => {
      svc.saveResponse = of(updated);
    });

    setFormField('volunteer-location', 'Chittagong');
    submitForm();

    expect(service.updateCalls).toHaveLength(1);
    expect(service.updateCalls[0]?.location).toBe('Chittagong');
    expect(element.querySelector('[data-testid="profile-saved"]')).not.toBeNull();
  });

  it('shows the normalized error when loading fails for a real reason', async () => {
    await setup((svc) => {
      svc.getMineOrNullResponse = throwError(() => new Error('Could not reach the API.'));
    });

    expect(element.querySelector('[data-testid="profile-error"]')?.textContent).toContain(
      'Could not reach the API.',
    );
    expect(element.querySelector('app-volunteer-profile-form')).toBeNull();
  });
});
