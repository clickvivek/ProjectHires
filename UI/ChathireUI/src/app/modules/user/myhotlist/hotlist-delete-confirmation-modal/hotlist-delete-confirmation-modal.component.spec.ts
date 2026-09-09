import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HotlistDeleteConfirmationModalComponent } from './hotlist-delete-confirmation-modal.component';

describe('HotlistDeleteConfirmationModalComponent', () => {
  let component: HotlistDeleteConfirmationModalComponent;
  let fixture: ComponentFixture<HotlistDeleteConfirmationModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ HotlistDeleteConfirmationModalComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(HotlistDeleteConfirmationModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
