import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProfileAvailableResourcesItemComponent } from './profile-available-resources-item.component';

describe('ProfileAvailableResourcesItemComponent', () => {
  let component: ProfileAvailableResourcesItemComponent;
  let fixture: ComponentFixture<ProfileAvailableResourcesItemComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ProfileAvailableResourcesItemComponent]
    });
    fixture = TestBed.createComponent(ProfileAvailableResourcesItemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
