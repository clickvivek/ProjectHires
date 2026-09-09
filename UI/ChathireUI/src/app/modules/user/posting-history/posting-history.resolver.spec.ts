import { TestBed } from '@angular/core/testing';
import { ResolveFn } from '@angular/router';

import { postingHistoryResolver } from './posting-history.resolver';

describe('postingHistoryResolver', () => {
  const executeResolver: ResolveFn<boolean> = (...resolverParameters) => 
      TestBed.runInInjectionContext(() => postingHistoryResolver(...resolverParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeResolver).toBeTruthy();
  });
});
