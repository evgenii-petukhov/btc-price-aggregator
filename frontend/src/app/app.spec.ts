import { TestBed } from '@angular/core/testing';
import { App } from './app';
import { Client } from '../api-client/api-client';

describe('App', () => {
    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [App],
            providers: [{ provide: Client, useValue: {} }],
        }).compileComponents();
    });

    it('should create the app', () => {
        const fixture = TestBed.createComponent(App);
        expect(fixture.componentInstance).toBeTruthy();
    });
});
