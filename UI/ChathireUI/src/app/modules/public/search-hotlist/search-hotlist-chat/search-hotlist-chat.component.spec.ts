import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SearchHotlistChatComponent } from './search-hotlist-chat.component';

describe('SearchHotlistChatComponent', () => {
  let component: SearchHotlistChatComponent;
  let fixture: ComponentFixture<SearchHotlistChatComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [SearchHotlistChatComponent]
    });
    fixture = TestBed.createComponent(SearchHotlistChatComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
