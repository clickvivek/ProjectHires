import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfirmApplyJobComponent } from './confirm-apply-job.component';

describe('ConfirmApplyJobComponent', () => {
  let component: ConfirmApplyJobComponent;
  let fixture: ComponentFixture<ConfirmApplyJobComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ConfirmApplyJobComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ConfirmApplyJobComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
