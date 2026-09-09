import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SearchHotlistComponent } from './search-hotlist.component';

describe('SearchHotlistComponent', () => {
  let component: SearchHotlistComponent;
  let fixture: ComponentFixture<SearchHotlistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SearchHotlistComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SearchHotlistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
