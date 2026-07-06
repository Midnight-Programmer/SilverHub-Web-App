import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { buildApiUrl } from '../../core/api/api-url';
import { HeroListItem } from './models/hero-list-item.model';

@Injectable({ providedIn: 'root' })
export class HeroesApiService {
  private readonly http = inject(HttpClient);

  getHeroes(): Observable<HeroListItem[]> {
    return this.http.get<HeroListItem[]>(buildApiUrl('/api/v1/heroes'));
  }
}
