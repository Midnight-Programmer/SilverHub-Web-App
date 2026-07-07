import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, map, of, startWith } from 'rxjs';
import { toApiError } from '../../core/api/api-error';
import { LoadState } from '../../shared/models/load-state';
import { HeroCard } from './hero-card';
import { HeroesApiService } from './heroes-api.service';
import { HeroListItem } from './models/hero-list-item.model';

@Component({
  selector: 'app-heroes-list',
  imports: [HeroCard],
  templateUrl: './heroes-list.page.html',
})
export class HeroesListPage {
  private readonly api = inject(HeroesApiService);

  protected readonly skeletons = Array.from({ length: 8 }, (_, i) => i);

  protected readonly state = toSignal(
    this.api.getHeroes().pipe(
      map((heroes) => (heroes.length ? LoadState.success(heroes) : LoadState.empty())),
      startWith(LoadState.loading()),
      catchError((err) => of(LoadState.error(toApiError(err).message))),
    ),
    { initialValue: LoadState.idle() as LoadState<HeroListItem[]> },
  );
}
