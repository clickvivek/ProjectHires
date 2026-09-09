import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InboxResumeTypesComponent } from './inbox-resume-types.component';

describe('InboxResumeTypesComponent', () => {
  let component: InboxResumeTypesComponent;
  let fixture: ComponentFixture<InboxResumeTypesComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [InboxResumeTypesComponent]
    });
    fixture = TestBed.createComponent(InboxResumeTypesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
