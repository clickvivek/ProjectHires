import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProfileJobPostingHistoryItemComponent } from './profile-job-posting-history-item.component';

describe('ProfileJobPostingHistoryItemComponent', () => {
  let component: ProfileJobPostingHistoryItemComponent;
  let fixture: ComponentFixture<ProfileJobPostingHistoryItemComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ProfileJobPostingHistoryItemComponent]
    });
    fixture = TestBed.createComponent(ProfileJobPostingHistoryItemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
