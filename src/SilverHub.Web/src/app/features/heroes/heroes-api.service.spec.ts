import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { HeroesApiService } from './heroes-api.service';
import { HeroListItem } from './models/hero-list-item.model';

describe('HeroesApiService', () => {
  let service: HeroesApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(HeroesApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('GETs the heroes endpoint and returns the list', () => {
    const fake: Partial<HeroListItem>[] = [{ slug: 'ethereal-joan', displayName: 'Ethereal Joan' }];
    let result: HeroListItem[] | undefined;

    service.getHeroes().subscribe((heroes) => (result = heroes));

    const req = httpMock.expectOne((r) => r.url.endsWith('/api/v1/heroes'));
    expect(req.request.method).toBe('GET');
    req.flush(fake);

    expect(result).toBeDefined();
    expect(result!.length).toBe(1);
    expect(result![0].slug).toBe('ethereal-joan');
  });
});
