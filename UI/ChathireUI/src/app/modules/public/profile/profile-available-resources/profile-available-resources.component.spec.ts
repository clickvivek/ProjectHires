import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProfileAvailableResourcesComponent } from './profile-available-resources.component';

describe('ProfileAvailableResourcesComponent', () => {
  let component: ProfileAvailableResourcesComponent;
  let fixture: ComponentFixture<ProfileAvailableResourcesComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ProfileAvailableResourcesComponent]
    });
    fixture = TestBed.createComponent(ProfileAvailableResourcesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
