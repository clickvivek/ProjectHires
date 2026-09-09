import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SearchSkillLocationComponent } from './search-skill-location.component';

describe('SearchSkillLocationComponent', () => {
  let component: SearchSkillLocationComponent;
  let fixture: ComponentFixture<SearchSkillLocationComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [SearchSkillLocationComponent]
    });
    fixture = TestBed.createComponent(SearchSkillLocationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
