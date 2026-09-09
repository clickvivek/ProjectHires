import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyhotlistListviewComponent } from './myhotlist-listview.component';

describe('MyhotlistListviewComponent', () => {
  let component: MyhotlistListviewComponent;
  let fixture: ComponentFixture<MyhotlistListviewComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [MyhotlistListviewComponent]
    });
    fixture = TestBed.createComponent(MyhotlistListviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
