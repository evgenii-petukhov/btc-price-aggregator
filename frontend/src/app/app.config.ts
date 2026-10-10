import { ApplicationConfig, isDevMode, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { API_BASE_URL, Client } from '../api-client/api-client';

export const API_URL = isDevMode() ? 'https://127.0.0.1:5000' : '';

export const appConfig: ApplicationConfig = {
    providers: [
        provideBrowserGlobalErrorListeners(),
        provideRouter(routes),
        provideHttpClient(),
        { provide: API_BASE_URL, useValue: API_URL },
        Client,
    ],
};
