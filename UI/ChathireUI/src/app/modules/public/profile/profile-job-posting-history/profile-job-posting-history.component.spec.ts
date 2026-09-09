import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProfileJobPostingHistoryComponent } from './profile-job-posting-history.component';

describe('ProfileJobPostingHistoryComponent', () => {
  let component: ProfileJobPostingHistoryComponent;
  let fixture: ComponentFixture<ProfileJobPostingHistoryComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ProfileJobPostingHistoryComponent]
    });
    fixture = TestBed.createComponent(ProfileJobPostingHistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
