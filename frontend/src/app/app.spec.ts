import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Observable, of, Subject, throwError } from 'rxjs';
import { App } from './app';
import {
    ApiException,
    Client,
    InstrumentType,
    PriceItemDto,
    PriceSummaryDto,
} from '../api-client/api-client';

function summary(isCached: boolean): PriceSummaryDto {
    return new PriceSummaryDto({
        instrument: 'BTCUSD',
        date: new Date('2023-01-01T00:00:00Z'),
        isCached,
        prices: [
            new PriceItemDto({ sourceName: 'Bitstamp', price: 16531 }),
            new PriceItemDto({ sourceName: 'Bitfinex', price: 16529.5 }),
        ],
    });
}

describe('App', () => {
    let fixture: ComponentFixture<App>;
    let priceMock: ReturnType<typeof vi.fn<(...args: unknown[]) => Observable<PriceSummaryDto>>>;

    const el = () => fixture.nativeElement as HTMLElement;
    const text = () => (el().textContent ?? '').replace(/\s+/g, ' ');

    function setTimestamp(value: string): void {
        const input = el().querySelector<HTMLInputElement>('input[type="datetime-local"]')!;
        input.value = value;
        input.dispatchEvent(new Event('input'));
    }

    function submit(): void {
        el().querySelector('form')!.dispatchEvent(new Event('submit'));
        fixture.detectChanges();
    }

    beforeEach(async () => {
        priceMock = vi.fn();
        await TestBed.configureTestingModule({
            imports: [App],
            providers: [{ provide: Client, useValue: { price: priceMock } }],
        }).compileComponents();

        fixture = TestBed.createComponent(App);
        fixture.detectChanges();
    });

    it('should create the app', () => {
        expect(fixture.componentInstance).toBeTruthy();
    });

    it('lists all instruments in the select', () => {
        const options = Array.from(el().querySelectorAll('select option')).map((o) =>
            o.textContent?.trim(),
        );
        expect(options).toEqual(Object.values(InstrumentType));
    });

    it('shows the placeholder before any request', () => {
        expect(text()).toContain('Submit the form to see the aggregated price.');
    });

    it('does not call the API when the timestamp is missing', () => {
        submit();

        expect(priceMock).not.toHaveBeenCalled();
        expect(text()).toContain('Timestamp is required.');
    });

    it('sets the timestamp when "Now" is clicked', () => {
        el().querySelector<HTMLButtonElement>('button.secondary')!.click();
        fixture.detectChanges();

        const input = el().querySelector<HTMLInputElement>('input[type="datetime-local"]')!;
        expect(input.value).toMatch(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/);
    });

    it('sends the selected instrument and the timestamp interpreted as UTC', () => {
        priceMock.mockReturnValue(of(summary(false)));
        setTimestamp('2023-01-01T00:00');
        submit();

        expect(priceMock).toHaveBeenCalledTimes(1);
        const [date, instrument] = priceMock.mock.calls[0];
        expect((date as Date).toISOString()).toBe('2023-01-01T00:00:00.000Z');
        expect(instrument).toBe(InstrumentType.BtcUsd);
    });

    it('sends another instrument when selected', () => {
        priceMock.mockReturnValue(of(summary(false)));
        const select = el().querySelector<HTMLSelectElement>('select')!;
        select.value = InstrumentType.EthUsd;
        select.dispatchEvent(new Event('change'));
        setTimestamp('2023-01-01T00:00');
        submit();

        expect(priceMock.mock.calls[0][1]).toBe(InstrumentType.EthUsd);
    });

    it('shows a loading state and disables the submit button while the request is pending', () => {
        priceMock.mockReturnValue(new Subject<PriceSummaryDto>());
        setTimestamp('2023-01-01T00:00');
        submit();

        expect(text()).toContain('Fetching prices…');
        expect(el().querySelector<HTMLButtonElement>('button[type="submit"]')!.disabled).toBe(true);
    });

    it('renders the title, aggregated price, source and source prices', () => {
        priceMock.mockReturnValue(of(summary(false)));
        setTimestamp('2023-01-01T00:00');
        submit();

        expect(el().querySelector('h3')?.textContent).toBe('BTC/USD price at 2023-01-01 00:00 UTC');
        expect(text()).toContain('Aggregated price: 16,530.25 USD');
        expect(text()).toContain('Result source: External APIs');
        const items = Array.from(el().querySelectorAll('li')).map((li) => li.textContent?.trim());
        expect(items).toEqual(['Bitstamp: 16,531.00', 'Bitfinex: 16,529.50']);
    });

    it('shows "Cache / datastore" when the result is cached', () => {
        priceMock.mockReturnValue(of(summary(true)));
        setTimestamp('2023-01-01T00:00');
        submit();

        expect(text()).toContain('Result source: Cache / datastore');
    });

    it('shows n/a when there are no prices', () => {
        priceMock.mockReturnValue(of(new PriceSummaryDto({ instrument: 'BTCUSD', prices: [] })));
        setTimestamp('2023-01-01T00:00');
        submit();

        expect(text()).toContain('Aggregated price: n/a');
    });

    it('shows a readable error and no result when the request fails', () => {
        priceMock.mockReturnValue(
            throwError(() => new ApiException('boom', 400, '{"detail":"Bad date"}', {}, null)),
        );
        setTimestamp('2023-01-01T00:00');
        submit();

        expect(text()).toContain('The request was rejected: Bad date');
        expect(el().querySelector('h3')).toBeNull();
        expect(el().querySelector<HTMLButtonElement>('button[type="submit"]')!.disabled).toBe(
            false,
        );
    });

    it('clears a previous error on a successful retry', () => {
        priceMock.mockReturnValueOnce(throwError(() => new ApiException('down', 0, '', {}, null)));
        setTimestamp('2023-01-01T00:00');
        submit();
        expect(text()).toContain('Cannot reach the API');

        priceMock.mockReturnValueOnce(of(summary(false)));
        submit();

        expect(text()).not.toContain('Cannot reach the API');
        expect(text()).toContain('Aggregated price:');
    });
});
