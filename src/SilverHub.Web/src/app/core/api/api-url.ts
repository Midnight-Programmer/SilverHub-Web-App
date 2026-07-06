import { environment } from '../../../environments/environment';

/** Builds an absolute API URL from a path like `/api/v1/heroes`. */
export function buildApiUrl(path: string): string {
  const base = environment.apiBaseUrl.replace(/\/$/, '');
  const suffix = path.startsWith('/') ? path : `/${path}`;
  return `${base}${suffix}`;
}

/** Builds an absolute asset URL from an object key like `heroes/joan/portrait.webp`. */
export function buildAssetUrl(key: string): string {
  const base = environment.assetBaseUrl.replace(/\/$/, '');
  const suffix = key.startsWith('/') ? key : `/${key}`;
  return `${base}${suffix}`;
}
