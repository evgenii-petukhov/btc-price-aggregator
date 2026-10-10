import { DecimalPipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Client, InstrumentType, PriceSummaryDto } from '../api-client/api-client';
import { API_URL } from './app.config';
import { toErrorMessage } from './error-message';

@Component({
    imports: [ReactiveFormsModule, DecimalPipe],
    selector: 'app-root',
    styleUrl: './app.scss',
    templateUrl: './app.html',
})
export class App {
    private readonly client = inject(Client);

    protected readonly instruments = Object.values(InstrumentType);
    protected readonly form = inject(FormBuilder).nonNullable.group({
        instrument: [
            InstrumentType.BtcUsd as string,
            [Validators.required, App.instrumentValidator],
        ],
        timestamp: ['', Validators.required],
    });

    protected readonly loading = signal(false);
    protected readonly result = signal<PriceSummaryDto | null>(null);
    protected readonly error = signal<string | null>(null);

    protected readonly aggregatedPrice = computed(() => {
        const prices = (this.result()?.prices ?? []).map((p) => p.price ?? 0);
        return prices.length ? prices.reduce((a, b) => a + b, 0) / prices.length : null;
    });

    protected readonly title = computed(() => {
        const r = this.result();
        if (!r) {
            return '';
        }
        const instrument = (r.instrument ?? '').replace(/^(.{3})(.+)$/, '$1/$2');
        const date = r.date ? r.date.toISOString().slice(0, 16).replace('T', ' ') : '';
        return `${instrument} price at ${date} UTC`;
    });

    private static instrumentValidator(control: { value: string }): ValidationErrors | null {
        const value = (control.value ?? '').trim().toLowerCase();
        return Object.values(InstrumentType).some((i) => i.toLowerCase() === value)
            ? null
            : { instrument: true };
    }

    protected setNow(): void {
        this.form.controls.timestamp.setValue(new Date().toISOString().slice(0, 16));
    }

    protected submit(): void {
        if (this.form.invalid || this.loading()) {
            this.form.markAllAsTouched();
            return;
        }

        const { instrument, timestamp } = this.form.getRawValue();
        const normalized = this.instruments.find(
            (i) => i.toLowerCase() === instrument.trim().toLowerCase(),
        )!;
        const date = new Date(`${timestamp}${timestamp.length === 16 ? ':00' : ''}Z`);

        this.loading.set(true);
        this.error.set(null);
        this.result.set(null);

        this.client.price(date, normalized).subscribe({
            next: (summary) => {
                this.result.set(summary);
                this.loading.set(false);
            },
            error: (err) => {
                this.error.set(toErrorMessage(err, API_URL));
                this.loading.set(false);
            },
        });
    }
}
