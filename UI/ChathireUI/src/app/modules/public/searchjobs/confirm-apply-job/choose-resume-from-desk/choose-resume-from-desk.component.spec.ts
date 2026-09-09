import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChooseResumeFromDeskComponent } from './choose-resume-from-desk.component';

describe('ChooseResumeFromDeskComponent', () => {
  let component: ChooseResumeFromDeskComponent;
  let fixture: ComponentFixture<ChooseResumeFromDeskComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ChooseResumeFromDeskComponent]
    });
    fixture = TestBed.createComponent(ChooseResumeFromDeskComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
