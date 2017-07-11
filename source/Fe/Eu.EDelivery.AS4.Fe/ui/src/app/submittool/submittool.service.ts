import { Observer } from 'rxjs/Observer';
import { Injectable, NgZone } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';

import { TOKENSTORE } from './../authentication/token';

@Injectable()
export class SubmitToolService {
    private _baseUrl = 'api/submittool';
    constructor(private _http: Http, private _ngZone: NgZone) { }
    public upload(submitData: SubmitData): Observable<number> {
        return Observable.create((obs: Observer<number>) => {
            let data = new FormData();
            let counter = 0;
            submitData.files.map((file) => {
                data.append(`file[${counter++}]`, file.some);
            });
            data.append('pmode', submitData.pmode);
            data.append('messages', submitData.messages + '');
            data.append('payloadLocation', submitData.payloadLocation);
            data.append('to', submitData.to);
            let xhr = new XMLHttpRequest();

            xhr.open('POST', `${this._baseUrl}`, true);
            xhr.setRequestHeader('Authorization', 'Bearer ' + localStorage.getItem(TOKENSTORE));
            xhr.onreadystatechange = () => {
                console.log('status ' + xhr.status);
                this._ngZone.run(() => {
                    if (xhr.readyState === 4) {
                        if (xhr.status === 200) {
                            obs.complete();
                        } else if (xhr.status === 417 && !!xhr.responseText) {
                            obs.error(JSON.parse(xhr.responseText));
                        } else {
                            obs.error(JSON.parse(xhr.responseText));
                        }
                    }
                });
            };
            xhr.upload.onprogress = (event) => {
                const progress = Math.round(event.loaded / event.total * 100);
                this._ngZone.run(() => {
                    obs.next(progress);
                });
            };
            xhr.upload.onerror = (error) => {
                this._ngZone.run(() => {
                    obs.error(error);
                });
            };
            xhr.send(data);
        });
    }
}

// tslint:disable-next-line:max-classes-per-file
export class SubmitData {
    public files: any[];
    public pmode: string;
    public messages: number = 1;
    public payloadType: number = 1;
    public submitType: number = 1;
    public payloadLocation: string = 'http://localhost:3000/api/Payload/Upload';
    public toType: number = 0;
    public to: string = 'http://localhost:9000/msh/';
}
