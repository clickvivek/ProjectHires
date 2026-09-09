import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InboxJobDetailsComponent } from './inbox-job-details.component';

describe('InboxJobDetailsComponent', () => {
  let component: InboxJobDetailsComponent;
  let fixture: ComponentFixture<InboxJobDetailsComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [InboxJobDetailsComponent]
    });
    fixture = TestBed.createComponent(InboxJobDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
