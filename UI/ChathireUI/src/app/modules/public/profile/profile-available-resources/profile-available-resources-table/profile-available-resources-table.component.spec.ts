import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProfileAvailableResourcesTableComponent } from './profile-available-resources-table.component';

describe('ProfileAvailableResourcesTableComponent', () => {
  let component: ProfileAvailableResourcesTableComponent;
  let fixture: ComponentFixture<ProfileAvailableResourcesTableComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ProfileAvailableResourcesTableComponent]
    });
    fixture = TestBed.createComponent(ProfileAvailableResourcesTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
