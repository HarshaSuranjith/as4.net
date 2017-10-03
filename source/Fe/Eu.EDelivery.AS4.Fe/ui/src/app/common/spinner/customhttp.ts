import { Injector } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs';
import { Http, XHRBackend, Request, RequestOptionsArgs, RequestOptions, Response } from '@angular/http';
import { Router } from '@angular/router';

import { ErrorResponse } from './../../api/ErrorResponse';
import { SpinnerService } from './spinner.service';
import { DialogService } from './../dialog.service';

export class CustomHttp extends Http {
    public noSpinner: boolean = false;
    // tslint:disable-next-line:max-line-length
    constructor(backend: XHRBackend, options: RequestOptions, private spinnerService: SpinnerService, private _dialogService: DialogService, private _injector: Injector) {
        super(backend, options);
    }
    public request(url: string | Request, options?: RequestOptionsArgs, ): Observable<Response> {
        if (!this.noSpinner) {
            this.spinnerService.show();
        }
        let result = super
            .request(url, options)
            .do(() => {
                if (!this.noSpinner) {
                    this.spinnerService.hide();
                }
            })
            .catch((error: Response) => {
                this.spinnerService.hide();
                if (error.status === 401) {
                    const router = this._injector.get<Router>(Router);
                    router.navigate(['/login']);
                    return Observable.empty();
                }
                const errorResponse = <ErrorResponse>error.json();
                if (errorResponse.Type.toLocaleLowerCase() === 'businessexception') {
                    this._dialogService.error(errorResponse.Message, errorResponse.Exception, false);
                    this.spinnerService.hide();
                    return Observable.empty();
                } else {
                    // tslint:disable-next-line:max-line-length
                    this._dialogService.error(`An error occured while communicating with the API. Please verify that you can reach the API. Please reload the application to try again.`, error, true);
                }
                return Observable.throw(error);
            });
        return result;
    }
}

// tslint:disable-next-line:max-classes-per-file
export class CustomAuthNoSpinnerHttp extends AuthHttp { }
