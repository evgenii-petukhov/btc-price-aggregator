import { ApiException } from '../api-client/api-client';

export function toErrorMessage(error: unknown, apiUrl: string): string {
    if (!(error instanceof ApiException)) {
        return error instanceof Error ? error.message : 'An unexpected error occurred.';
    }

    if (error.status === 0) {
        return `Cannot reach the API at ${apiUrl}. Make sure the backend is running and its HTTPS certificate is trusted.`;
    }

    const details = parseProblemDetails(error.response);

    if (error.status === 400) {
        return `The request was rejected: ${details || 'invalid instrument or timestamp.'}`;
    }

    if (error.status >= 500) {
        return `The server or an upstream price source failed (HTTP ${error.status}). Please try again later.${
            details ? ` Details: ${details}` : ''
        }`;
    }

    return `Request failed (HTTP ${error.status}).${details ? ` ${details}` : ''}`;
}

function parseProblemDetails(response: string): string {
    if (!response) {
        return '';
    }

    try {
        const body = JSON.parse(response);
        const messages: string[] = [];

        if (body?.errors && typeof body.errors === 'object') {
            for (const [field, fieldErrors] of Object.entries(body.errors)) {
                messages.push(
                    `${field}: ${([] as string[]).concat(fieldErrors as string[]).join(', ')}`,
                );
            }
        }

        if (messages.length === 0) {
            const text = body?.detail ?? body?.title ?? body?.message;
            if (text) {
                messages.push(String(text));
            }
        }

        return messages.join('; ');
    } catch {
        return response.length <= 300 ? response : '';
    }
}
