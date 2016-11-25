import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Observable';

import { RuntimeStore } from './runtime.store';

@Injectable()
export class RuntimeService {
    constructor(private http: AuthHttp, private runtimeStore: RuntimeStore) {

    }
    public getReceivers() {
        this.http
            .get(this.getBaseUrl('getreceivers'))
            .subscribe(type => this.runtimeStore.update('receivers', type.json()));
    }
    public getSteps() {
        this.http
            .get(this.getBaseUrl('getsteps'))
            .subscribe(type => this.runtimeStore.update('steps', type.json()));
    }
    public getTransformers() {
        this.http
            .get(this.getBaseUrl('gettransformers'))
            .subscribe(type => this.runtimeStore.update('transformers', type.json()));
    }
    private getBaseUrl(action: string) {
        return `api/runtime/${action}`;
    }
}

export class ItemType {
    name: string;
    properties: Property[];
}

export class Property {
    friendlyName: string;
    type: string;
    regex: string;
}