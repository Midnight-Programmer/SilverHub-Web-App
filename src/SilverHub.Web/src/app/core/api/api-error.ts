import { HttpErrorResponse } from '@angular/common/http';

export interface ApiError {
  status: number;
  message: string;
  traceId?: string;
}

/** Normalizes an unknown error (usually an HttpErrorResponse holding a ProblemDetails body) into an ApiError. */
export function toApiError(err: unknown): ApiError {
  if (err instanceof HttpErrorResponse) {
    const problem = err.error;
    return {
      status: err.status,
      message: problem?.title ?? err.message ?? 'Request failed',
      traceId: problem?.traceId,
    };
  }
  return { status: 0, message: 'Unexpected error' };
}
