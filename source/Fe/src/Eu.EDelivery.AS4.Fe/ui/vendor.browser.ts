import { AppState } from './app/app.service';
// For vendors for example jQuery, Lodash, angular2-jwt just import them here unless you plan on
// chunking vendors files for async loading. You would need to import the async loaded vendors
// at the entry point of the async loaded file. Also see custom-typings.d.ts as you also need to
// run `typings install x` where `x` is your module

// TODO(gdi2290): switch to DLLs

// Angular 2
import '@angular/platform-browser';
import '@angular/platform-browser-dynamic';
import '@angular/core';
import '@angular/common';
import '@angular/forms';
import '@angular/http';
import '@angular/router';

// AngularClass
import '@angularclass/hmr';

// RxJS
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/mergeMap';

// Theme
import 'jquery';

import 'bootstrap/dist/js/bootstrap.js';
import './assets/theme/js/app.js';
import '../node_modules/bootstrap/dist/css/bootstrap.css';
import './assets/theme/css/AdminLTE.css';
import './assets/theme/css/skins/_all-skins.min.css';
import './assets/css/site.scss';

declare var ENV: any;
if ('production' === ENV) {
    // Production


} else {
    // Development

}