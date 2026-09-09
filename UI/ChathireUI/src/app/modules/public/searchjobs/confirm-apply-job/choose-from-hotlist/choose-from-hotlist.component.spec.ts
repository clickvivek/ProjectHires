import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChooseFromHotlistComponent } from './choose-from-hotlist.component';

describe('ChooseFromHotlistComponent', () => {
  let component: ChooseFromHotlistComponent;
  let fixture: ComponentFixture<ChooseFromHotlistComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ChooseFromHotlistComponent]
    });
    fixture = TestBed.createComponent(ChooseFromHotlistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
