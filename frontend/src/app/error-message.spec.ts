import { ApiException } from '../api-client/api-client';
import { toErrorMessage } from './error-message';

const URL = 'https://api.test';

function apiError(status: number, response = ''): ApiException {
    return new ApiException('msg', status, response, {}, null);
}

describe('toErrorMessage', () => {
    it('returns the message of a generic Error', () => {
        expect(toErrorMessage(new Error('oops'), URL)).toBe('oops');
    });

    it('returns a fallback for non-Error values', () => {
        expect(toErrorMessage('x', URL)).toBe('An unexpected error occurred.');
        expect(toErrorMessage(null, URL)).toBe('An unexpected error occurred.');
    });

    it('explains that the API is unreachable for status 0 and includes the URL', () => {
        const message = toErrorMessage(apiError(0), URL);

        expect(message).toContain('Cannot reach the API at https://api.test');
        expect(message).toContain('backend is running');
    });

    describe('400', () => {
        it('uses a default text when there is no body', () => {
            expect(toErrorMessage(apiError(400), URL)).toBe(
                'The request was rejected: invalid instrument or timestamp.',
            );
        });

        it('lists validation errors per field', () => {
            const body = JSON.stringify({
                errors: { instrument: ['Required', 'Invalid'], date: 'Bad date' },
            });

            expect(toErrorMessage(apiError(400, body), URL)).toBe(
                'The request was rejected: instrument: Required, Invalid; date: Bad date',
            );
        });

        it('uses detail when there are no errors', () => {
            const body = JSON.stringify({ title: 'Bad Request', detail: 'Date is invalid' });

            expect(toErrorMessage(apiError(400, body), URL)).toBe(
                'The request was rejected: Date is invalid',
            );
        });

        it('falls back to title, then message', () => {
            expect(toErrorMessage(apiError(400, '{"title":"Bad Request"}'), URL)).toContain(
                'Bad Request',
            );
            expect(toErrorMessage(apiError(400, '{"message":"Nope"}'), URL)).toContain('Nope');
        });

        it('uses a short non-JSON body as is', () => {
            expect(toErrorMessage(apiError(400, 'plain text'), URL)).toBe(
                'The request was rejected: plain text',
            );
        });

        it('ignores a long non-JSON body', () => {
            expect(toErrorMessage(apiError(400, 'x'.repeat(301)), URL)).toBe(
                'The request was rejected: invalid instrument or timestamp.',
            );
        });
    });

    describe('5xx', () => {
        it('reports a server failure without details when the body is empty', () => {
            expect(toErrorMessage(apiError(500), URL)).toBe(
                'The server or an upstream price source failed (HTTP 500). Please try again later.',
            );
        });

        it('appends details when present', () => {
            const message = toErrorMessage(apiError(502, '{"detail":"Bitstamp timeout"}'), URL);

            expect(message).toContain('(HTTP 502)');
            expect(message).toContain('Details: Bitstamp timeout');
        });
    });

    describe('other statuses', () => {
        it('reports the status', () => {
            expect(toErrorMessage(apiError(404), URL)).toBe('Request failed (HTTP 404).');
        });

        it('appends details when present', () => {
            expect(toErrorMessage(apiError(404, '{"title":"Not Found"}'), URL)).toBe(
                'Request failed (HTTP 404). Not Found',
            );
        });
    });
});
