/**
 * Utility helper to extract clean, user-friendly, and actionable error messages
 * from HTTP responses and unexpected runtime errors.
 */
export function getMeaningfulErrorMessage(
  error: any,
  fallbackMessage: string = 'An unexpected error occurred. Please try again.'
): string {
  if (!error) {
    return fallbackMessage;
  }

  // Connection refused / Network down / CORS failure
  if (error.status === 0 || (error.name === 'HttpErrorResponse' && error.status === 0)) {
    return 'Unable to reach the server. Please check your internet connection or verify the service is running.';
  }

  // 400 Bad Request
  if (error.status === 400) {
    if (typeof error.error === 'string' && error.error.trim().length > 0 && !error.error.includes('<!DOCTYPE')) {
      return error.error.trim();
    }
    if (error.error?.message) {
      return error.error.message;
    }
    if (error.error?.title) {
      return error.error.title;
    }
    if (error.error?.errors && typeof error.error.errors === 'object') {
      const messages: string[] = [];
      for (const key of Object.keys(error.error.errors)) {
        const val = error.error.errors[key];
        if (Array.isArray(val)) {
          messages.push(...val);
        } else if (typeof val === 'string') {
          messages.push(val);
        }
      }
      if (messages.length > 0) {
        return messages.join('. ');
      }
    }
    return 'Invalid request parameters. Please verify your search criteria and try again.';
  }

  // 401 Unauthorized
  if (error.status === 401) {
    return 'Your session has expired or you are not authorized. Please log in again.';
  }

  // 403 Forbidden
  if (error.status === 403) {
    return 'You do not have permission to perform this action.';
  }

  // 404 Not Found
  if (error.status === 404) {
    return 'The requested information could not be found.';
  }

  // 408 Request Timeout
  if (error.status === 408) {
    return 'The request timed out. Please try again.';
  }

  // 429 Too Many Requests
  if (error.status === 429) {
    return 'Too many requests. Please wait a moment before trying again.';
  }

  // 500 Internal Server Error
  if (error.status === 500) {
    if (typeof error.error === 'string' && error.error.trim().length > 0 && !error.error.includes('<!DOCTYPE')) {
      return error.error.trim();
    }
    if (error.error?.message) {
      return error.error.message;
    }
    return 'A server error occurred while processing your request. Please try again shortly.';
  }

  // 502 / 503 / 504 Service Unavailable / Bad Gateway
  if (error.status === 502 || error.status === 503 || error.status === 504) {
    return 'The server is temporarily unavailable. Please try again in a few moments.';
  }

  // Direct error message string
  if (typeof error.error === 'string' && error.error.trim().length > 0 && !error.error.includes('<!DOCTYPE')) {
    return error.error.trim();
  }

  if (error.error?.message) {
    return error.error.message;
  }

  if (error.error?.title) {
    return error.error.title;
  }

  if (typeof error.message === 'string' && !error.message.includes('Http failure response')) {
    return error.message;
  }

  return fallbackMessage;
}
