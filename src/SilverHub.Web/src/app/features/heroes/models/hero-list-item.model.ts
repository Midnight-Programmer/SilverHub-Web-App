/** Mirrors the backend HeroListItemDto (Doc 04: FE models mirror API DTOs). */
export interface HeroListItem {
  id: string;
  slug: string;
  displayName: string;
  releaseDate: string | null;
  rarity: string;
  faction: string;
  equipType: string;
  class: string;
  moonType: string;
  damageType: string;
  limited: boolean;
  boudoir: boolean;
  hasResonantia: boolean;
  portraitImageKey: string | null;
  synergySlug: string | null;
  synergyName: string | null;
}
