import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InboxResumeCommentsComponent } from './inbox-resume-comments.component';

describe('InboxResumeCommentsComponent', () => {
  let component: InboxResumeCommentsComponent;
  let fixture: ComponentFixture<InboxResumeCommentsComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [InboxResumeCommentsComponent]
    });
    fixture = TestBed.createComponent(InboxResumeCommentsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
