import { Component, input, signal } from '@angular/core';
import { buildAssetUrl } from '../../core/api/api-url';
import { HeroListItem } from './models/hero-list-item.model';

@Component({
  selector: 'app-hero-card',
  template: `
    <div class="group rounded-xl border border-slate-200 bg-white p-3 shadow-sm transition hover:shadow-md">
      <div class="relative aspect-[3/4] w-full overflow-hidden rounded-lg bg-slate-100">
        @if (portraitUrl() && !imageFailed()) {
          <img
            [src]="portraitUrl()!"
            [alt]="hero().displayName"
            class="h-full w-full object-cover"
            (error)="imageFailed.set(true)"
          />
        } @else {
          <div class="flex h-full w-full items-center justify-center text-4xl font-bold text-slate-300">
            {{ hero().displayName.charAt(0) }}
          </div>
        }
        <span class="absolute left-2 top-2 rounded bg-black/60 px-1.5 py-0.5 text-xs font-semibold text-white">
          {{ hero().rarity }}
        </span>
        @if (hero().limited) {
          <span class="absolute right-2 top-2 rounded bg-amber-500 px-1.5 py-0.5 text-xs font-semibold text-white">
            Limited
          </span>
        }
      </div>
      <h3 class="mt-2 truncate text-sm font-semibold text-slate-900">{{ hero().displayName }}</h3>
      <p class="text-xs text-slate-500">{{ hero().faction }} · {{ hero().class }}</p>
    </div>
  `,
})
export class HeroCard {
  readonly hero = input.required<HeroListItem>();
  protected readonly imageFailed = signal(false);

  protected portraitUrl(): string | null {
    const key = this.hero().portraitImageKey;
    return key ? buildAssetUrl(key) : null;
  }
}
