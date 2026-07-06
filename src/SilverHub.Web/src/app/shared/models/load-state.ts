/** Every API-backed view is in exactly one of these states (Doc 04). */
export type LoadState<T> =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; data: T }
  | { status: 'empty' }
  | { status: 'notFound' }
  | { status: 'error'; message: string };

/** Constructor helpers so components can write `state.set(LoadState.loading())`. */
export const LoadState = {
  idle: (): LoadState<never> => ({ status: 'idle' }),
  loading: (): LoadState<never> => ({ status: 'loading' }),
  success: <T>(data: T): LoadState<T> => ({ status: 'success', data }),
  empty: (): LoadState<never> => ({ status: 'empty' }),
  notFound: (): LoadState<never> => ({ status: 'notFound' }),
  error: (message: string): LoadState<never> => ({ status: 'error', message }),
};
