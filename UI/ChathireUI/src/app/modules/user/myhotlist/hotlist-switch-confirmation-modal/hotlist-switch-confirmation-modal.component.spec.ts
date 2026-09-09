import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HotlistSwitchConfirmationModalComponent } from './hotlist-switch-confirmation-modal.component';

describe('HotlistSwitchConfirmationModalComponent', () => {
  let component: HotlistSwitchConfirmationModalComponent;
  let fixture: ComponentFixture<HotlistSwitchConfirmationModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ HotlistSwitchConfirmationModalComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(HotlistSwitchConfirmationModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
