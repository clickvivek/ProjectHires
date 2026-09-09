import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CandidateDetailSheetComponent } from './candidate-detail-sheet.component';

describe('CandidateDetailSheetComponent', () => {
  let component: CandidateDetailSheetComponent;
  let fixture: ComponentFixture<CandidateDetailSheetComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CandidateDetailSheetComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CandidateDetailSheetComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
