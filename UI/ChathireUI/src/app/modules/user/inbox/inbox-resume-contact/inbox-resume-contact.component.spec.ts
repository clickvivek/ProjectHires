import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InboxResumeContactComponent } from './inbox-resume-contact.component';

describe('InboxResumeContactComponent', () => {
  let component: InboxResumeContactComponent;
  let fixture: ComponentFixture<InboxResumeContactComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [InboxResumeContactComponent]
    });
    fixture = TestBed.createComponent(InboxResumeContactComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
