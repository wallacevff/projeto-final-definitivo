import {ApplicationConfig, importProvidersFrom, provideZoneChangeDetection} from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import {NgxSpinnerModule} from 'ngx-spinner';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {provideAnimationsAsync} from '@angular/platform-browser/animations/async';
import {provideNgxMask} from 'ngx-mask';
import {MAT_DATE_LOCALE, provideNativeDateAdapter} from '@angular/material/core';
import {DATE_PIPE_DEFAULT_OPTIONS} from '@angular/common';
import {provideToastr} from 'ngx-toastr';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }), provideRouter(routes),
    importProvidersFrom(NgxSpinnerModule.forRoot(/*config*/)),
    importProvidersFrom([BrowserAnimationsModule]),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideNgxMask(),
    provideNativeDateAdapter(),
    {
      provide: DATE_PIPE_DEFAULT_OPTIONS,
      useValue: {
        dateFormat: "dd/MM/yyyy",
      },
    },
    {
      provide:MAT_DATE_LOCALE,
      useValue: 'pt-BR'
    },
    provideToastr({
      autoDismiss: true,
      positionClass: 'toast-top-right',
      closeButton: true,
      enableHtml: true,
      newestOnTop: true,
      timeOut: 5000,
      maxOpened: 3,
      countDuplicates: true,
      progressBar: true,
      tapToDismiss: true,
      onActivateTick: true,
    }),
    provideNgxMask(),
  ]
};
