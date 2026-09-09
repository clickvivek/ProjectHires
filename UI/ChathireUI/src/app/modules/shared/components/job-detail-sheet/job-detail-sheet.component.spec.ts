import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { JobDetailSheetComponent } from './job-detail-sheet.component';

describe('JobDetailSheetComponent', () => {
  let component: JobDetailSheetComponent;
  let fixture: ComponentFixture<JobDetailSheetComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ JobDetailSheetComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(JobDetailSheetComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
