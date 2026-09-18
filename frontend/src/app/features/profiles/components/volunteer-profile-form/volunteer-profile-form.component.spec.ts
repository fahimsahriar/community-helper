import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';

import { VolunteerProfile } from '../../models/volunteer-profile.model';
import { VolunteerProfileFormComponent } from './volunteer-profile-form.component';

const profile: VolunteerProfile = {
  userId: 'u-1',
  skills: ['teaching'],
  availability: 'Weekends',
  causes: ['education'],
  location: 'Dhaka',
  bio: 'Teacher.',
};

describe('VolunteerProfileFormComponent', () => {
  let fixture: ComponentFixture<VolunteerProfileFormComponent>;
  let element: HTMLElement;

  async function setup(inputProfile: VolunteerProfile | null): Promise<void> {
    await TestBed.configureTestingModule({
      imports: [VolunteerProfileFormComponent],
      providers: [provideNoopAnimations()],
    }).compileComponents();

    fixture = TestBed.createComponent(VolunteerProfileFormComponent);
    fixture.componentRef.setInput('profile', inputProfile);
    fixture.detectChanges();
    element = fixture.nativeElement as HTMLElement;
  }

  function setInput(testid: string, value: string): void {
    const input = element.querySelector<HTMLInputElement | HTMLTextAreaElement>(
      `[data-testid="${testid}"]`,
    )!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  it('prefills the form when a profile is provided', async () => {
    await setup(profile);

    expect(
      element.querySelector<HTMLInputElement>('[data-testid="volunteer-skills"]')?.value,
    ).toContain('teaching');
    expect(
      element.querySelector<HTMLInputElement>('[data-testid="volunteer-location"]')?.value,
    ).toBe('Dhaka');
    expect(element.querySelector('[data-testid="volunteer-save"]')?.textContent).toContain(
      'Save changes',
    );
  });

  it('labels the submit button for setup when there is no profile', async () => {
    await setup(null);

    expect(element.querySelector('[data-testid="volunteer-save"]')?.textContent).toContain(
      'Create profile',
    );
  });

  it('emits parsed skills and causes on save', async () => {
    await setup(null);
    const emitted: unknown[] = [];
    fixture.componentInstance.save.subscribe((payload) => emitted.push(payload));

    setInput('volunteer-skills', 'teaching, first-aid');
    setInput('volunteer-availability', 'Weekends');
    setInput('volunteer-causes', 'education, health');
    setInput('volunteer-location', 'Dhaka');
    setInput('volunteer-bio', 'Happy to help.');

    element.querySelector<HTMLButtonElement>('[data-testid="volunteer-save"]')!.click();
    fixture.detectChanges();

    expect(emitted).toEqual([
      {
        skills: ['teaching', 'first-aid'],
        availability: 'Weekends',
        causes: ['education', 'health'],
        location: 'Dhaka',
        bio: 'Happy to help.',
      },
    ]);
  });

  it('does not emit when required fields are missing', async () => {
    await setup(null);
    const emitted: unknown[] = [];
    fixture.componentInstance.save.subscribe((payload) => emitted.push(payload));

    element.querySelector<HTMLButtonElement>('[data-testid="volunteer-save"]')!.click();
    fixture.detectChanges();

    expect(emitted).toHaveLength(0);
    expect(element.querySelector('[data-testid="volunteer-skills-error"]')).not.toBeNull();
  });
});
